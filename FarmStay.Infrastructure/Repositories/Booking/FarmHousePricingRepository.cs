using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Booking
{
    public class FarmHousePricingRepository : IFarmHousePricingRepository
    {
        private readonly AppDbContext _context;

        public FarmHousePricingRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<FarmStay.Application.DTOs.Booking.PricingEntry>> GetEntriesAsync(int farmHouseId) =>
            _context.FarmHousePricings.AsNoTracking().Where(p => p.FarmHouseId == farmHouseId && !p.IsDelete)
                .OrderBy(p => p.PricingName).Select(p => new FarmStay.Application.DTOs.Booking.PricingEntry(
                    p.PricingId, p.PricingKey, p.PricingName, p.Price, p.CurrencyCode, p.IsActive)).ToListAsync();

        public async Task<FarmStay.Application.DTOs.Booking.PricingChangeResult> ChangeEntryAsync(int farmHouseId,
            long userId, int? pricingId, FarmStay.Application.DTOs.Booking.PricingEntryRequest? request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var resource = $"farmstay:pricing:{farmHouseId}";
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive',
                    @LockOwner='Transaction', @LockTimeout=15000;
                IF @result < 0 THROW 50021, 'Pricing is busy. Please retry.', 1;");
            var rows = await _context.FarmHousePricings.Where(p => p.FarmHouseId == farmHouseId && !p.IsDelete).ToListAsync();
            var row = pricingId.HasValue ? rows.SingleOrDefault(p => p.PricingId == pricingId) : null;
            if (pricingId.HasValue && row == null) return new(404, "Pricing record not found.");
            var now = DateTime.Now;
            if (request == null)
            {
                if (row == null) return new(404, "Pricing record not found.");
                row.IsDelete = true;
                row.IsActive = false;
            }
            else
            {
                if (rows.Any(p => p.PricingId != pricingId && p.PricingKey == request.PricingKey))
                    return new(409, "A pricing record with this key already exists. Edit the existing record.");
                var baseRate = request.PricingKey == "BOOKING_DAILY"
                    ? request.IsActive ? request.Price : (decimal?)null
                    : rows.SingleOrDefault(p => p.PricingId != pricingId && p.PricingKey == "BOOKING_DAILY" && p.IsActive)?.Price;
                var advance = request.PricingKey == "ADVANCE_DAILY"
                    ? request.IsActive ? request.Price : (decimal?)null
                    : rows.SingleOrDefault(p => p.PricingId != pricingId && p.PricingKey == "ADVANCE_DAILY" && p.IsActive)?.Price;
                if (baseRate.HasValue && advance > baseRate) return new(400, "Advance per day cannot exceed the booking rate per day.");
                if (row == null)
                {
                    row = new FarmHousePricing { FarmHouseId = farmHouseId, CreatedBy = userId, CreatedDate = now };
                    _context.FarmHousePricings.Add(row);
                }
                row.PricingKey = request.PricingKey;
                row.PricingName = request.PricingName;
                row.Price = request.Price;
                row.IsActive = request.IsActive;
                row.CurrencyCode = "INR";
            }
            row.ModifiedBy = userId;
            row.ModifiedDate = now;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new(request == null ? 200 : pricingId == null ? 201 : 200,
                request == null ? "Pricing deleted successfully." : "Pricing saved successfully.",
                request == null ? null : new(row.PricingId, row.PricingKey, row.PricingName, row.Price, row.CurrencyCode, row.IsActive));
        }

        public async Task SaveRatesAsync(int farmHouseId, long userId, FarmStay.Application.DTOs.Booking.PricingRates rates)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var resource = $"farmstay:pricing:{farmHouseId}";
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive',
                    @LockOwner='Transaction', @LockTimeout=15000;
                IF @result < 0 THROW 50021, 'Pricing is busy. Please retry.', 1;");
            var existing = await _context.FarmHousePricings.Where(p =>
                p.FarmHouseId == farmHouseId && p.IsActive && !p.IsDelete).ToListAsync();
            var now = DateTime.Now;
            foreach (var rate in new (string Key, string Name, decimal Price)[] {
                ("BOOKING_DAILY", "Booking per day", rates.BookingDaily),
                ("NORMAL_EVENT_DAILY", "Normal event per day", rates.NormalEventDaily),
                ("DECORATION_EVENT_DAILY", "Event with decoration per day", rates.DecorationEventDaily),
                ("ADVANCE_DAILY", "Advance per day", rates.AdvanceDaily) })
            {
                var row = existing.SingleOrDefault(p => p.PricingKey == rate.Key);
                if (row == null)
                {
                    row = new FarmHousePricing { FarmHouseId = farmHouseId, PricingKey = rate.Key,
                        PricingName = rate.Name, CreatedBy = userId, CreatedDate = now };
                    _context.FarmHousePricings.Add(row);
                }
                row.Price = rate.Price;
                row.CurrencyCode = "INR";
                row.ModifiedBy = userId;
                row.ModifiedDate = now;
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task<List<FarmHousePricing>>
            GetActivePricingByFarmHouseIdAsync(int farmHouseId)
        {
            return await _context.FarmHousePricings
                .AsNoTracking()
                .Where(p =>
                    p.FarmHouseId == farmHouseId &&
                    p.IsActive &&
                    !p.IsDelete)
                .OrderBy(p => p.PricingName)
                .ToListAsync();
        }
    }
}