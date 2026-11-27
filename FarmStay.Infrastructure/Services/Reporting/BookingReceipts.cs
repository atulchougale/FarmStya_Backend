using System.Data;
using System.Globalization;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.NETCore;

namespace FarmStay.Infrastructure.Services.Reporting;

public sealed class BookingReceipts(AppDbContext db, FarmStay.Application.Interfaces.Common.ICurrentTenantContext tenant) : IBookingReceipts
{
    public async Task<IReadOnlyList<PaymentReceiptSummary>> ListAsync(int farmHouseId, long bookingId, int? customerId, CancellationToken ct)
    {
        await ValidateScope(farmHouseId, customerId, ct);
        return await db.BookingReceipts.AsNoTracking().Where(r =>
            r.FarmHouseId == farmHouseId && r.BookingId == bookingId &&
            r.Booking != null && !r.Booking.IsDelete &&
            (!customerId.HasValue || r.Booking.CustomerUserId == customerId) &&
            r.PaymentTransaction != null && !r.PaymentTransaction.IsDelete && r.PaymentTransaction.PaymentStatus == "Success")
            .OrderBy(r => r.IssuedAt)
            .Select(r => new PaymentReceiptSummary(r.ReceiptId, r.ReceiptNumber,
                r.PaymentTransaction!.PaymentType, r.Amount, r.IssuedAt)).ToListAsync(ct);
    }

    public async Task<ReceiptPdf?> RenderAsync(int farmHouseId, long bookingId, long receiptId, int? customerId, CancellationToken ct)
    {
        await ValidateScope(farmHouseId, customerId, ct);
        var rows = await db.Set<FarmStay.Application.DTOs.Reports.Receipts.Responses.PaymentReceiptReportRow>()
            .FromSqlInterpolated($"EXEC dbo.usp_Report_BookingPaymentReceipt @FarmHouseId={farmHouseId}, @BookingId={bookingId}, @ReceiptId={receiptId}, @CustomerUserId={customerId}")
            .AsNoTracking().ToListAsync(ct);
        var table = new DataTable();
        var properties = typeof(FarmStay.Application.DTOs.Reports.Receipts.Responses.PaymentReceiptReportRow).GetProperties();
        foreach (var property in properties) table.Columns.Add(property.Name, typeof(string));
        foreach (var row in rows) table.Rows.Add(properties.Select(property => property.GetValue(row) ?? DBNull.Value).ToArray());
        if (table.Rows.Count == 0) return null;
        table.Columns.Add("AmountInWords", typeof(string));
        table.Rows[0]["AmountInWords"] = AmountInWords(Convert.ToDecimal(table.Rows[0]["Amount"], CultureInfo.InvariantCulture));
        using var report = new LocalReport();
        using var definition = typeof(BookingReceipts).Assembly.GetManifestResourceStream(
            "FarmStay.Infrastructure.Reports.PaymentReceipt.rdlc")
            ?? throw new InvalidOperationException("Payment receipt report is missing.");
        report.LoadReportDefinition(definition);
        report.DataSources.Add(new ReportDataSource("Receipt", table));
        return new ReceiptPdf(report.Render("PDF"), $"{table.Rows[0]["ReceiptNumber"]}.pdf");
    }
    private async Task ValidateScope(int farm, int? customer, CancellationToken ct)
    {
        var scope = await tenant.RequireAsync(ct);
        if (scope.FarmHouseId != farm || (customer.HasValue && customer != scope.UserId))
            throw new FarmStay.Application.Interfaces.Common.TenantAccessException(403, "Receipt scope conflicts with your membership.");
        if (!customer.HasValue && scope.Role is not ("Admin" or "Farm Owner" or "FarmOwner" or "Super Admin"))
            throw new FarmStay.Application.Interfaces.Common.TenantAccessException(403, "An administration membership is required.");
    }
    private static string AmountInWords(decimal amount)
    {
        var rupees = decimal.ToInt64(decimal.Truncate(amount));
        var paise = decimal.ToInt32(decimal.Round((amount - rupees) * 100));
        return $"Rupees {Words(rupees)}{(paise > 0 ? $" and {Words(paise)} Paise" : "")} Only";
    }

    private static string Words(long number)
    {
        string[] units = ["Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"];
        string[] tens = ["", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"];
        if (number < 20) return units[number];
        if (number < 100) return tens[number / 10] + (number % 10 > 0 ? " " + Words(number % 10) : "");
        foreach (var scale in new (long Value, string Name)[] {
            (10000000, "Crore"), (100000, "Lakh"), (1000, "Thousand"), (100, "Hundred") })
        {
            if (number >= scale.Value)
                return Words(number / scale.Value) + " " + scale.Name +
                    (number % scale.Value > 0 ? " " + Words(number % scale.Value) : "");
        }
        throw new ArgumentOutOfRangeException(nameof(number));
    }

}
