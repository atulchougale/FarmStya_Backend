using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Admin
{
    public class AboutUsRepository : IAboutUsRepository
    {
        private readonly AppDbContext _context;

        public AboutUsRepository(
            AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ADD ABOUT US
        // =========================================================

        public async Task AddAboutUsAsync(
            AboutUs aboutUs)
        {
            await _context.AboutUs.AddAsync(aboutUs);
        }

        // =========================================================
        // GET ACTIVE ABOUT US BY FARMHOUSE
        // =========================================================

        public async Task<AboutUs?>
            GetAboutUsByFarmHouseIdAsync(
                int farmHouseId)
        {
            return await _context.AboutUs
                .FirstOrDefaultAsync(x =>
                    x.FarmHouseId == farmHouseId &&
                    !x.IsDelete);
        }

        // =========================================================
        // GET ABOUT US BY ID
        // =========================================================

        public async Task<AboutUs?>
            GetAboutUsAsync(
                int aboutUsId)
        {
            return await _context.AboutUs
                .Include(x => x.Features
                    .Where(f => !f.IsDelete))
                .FirstOrDefaultAsync(x =>
                    x.AboutUsId == aboutUsId &&
                    !x.IsDelete);
        }

        // =========================================================
        // ADD FEATURE
        // =========================================================

        public async Task AddFeatureAsync(
            AboutUsFeature feature)
        {
            await _context.AboutUsFeatures
                .AddAsync(feature);
        }

        // =========================================================
        // GET FEATURE
        // =========================================================

        public async Task<AboutUsFeature?>
            GetFeatureByIdAsync(
                int featureId)
        {
            return await _context.AboutUsFeatures
                .FirstOrDefaultAsync(x =>
                    x.FeatureId == featureId &&
                    !x.IsDelete);
        }

        // =========================================================
        // GET FEATURES
        // =========================================================

        public async Task<List<AboutUsFeature>>
            GetFeaturesAsync(
                int aboutUsId)
        {
            return await _context.AboutUsFeatures
                .AsNoTracking()
                .Where(x =>
                    x.AboutUsId == aboutUsId &&
                    !x.IsDelete)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        // =========================================================
        // UPDATE ABOUT US
        // =========================================================

        public Task UpdateAboutUsAsync(
            AboutUs aboutUs)
        {
            _context.AboutUs.Update(aboutUs);

            return Task.CompletedTask;
        }

        // =========================================================
        // UPDATE FEATURE
        // =========================================================

        public Task UpdateFeatureAsync(
            AboutUsFeature feature)
        {
            _context.AboutUsFeatures.Update(feature);

            return Task.CompletedTask;
        }

        // =========================================================
        // DELETE ABOUT US
        // =========================================================

        public async Task DeleteAboutUsAsync(
            int aboutUsId)
        {
            var aboutUs =
                await _context.AboutUs
                    .FirstOrDefaultAsync(x =>
                        x.AboutUsId == aboutUsId &&
                        !x.IsDelete);

            if (aboutUs == null)
                return;

            aboutUs.IsDelete = true;
            aboutUs.ModifyDate = DateTime.UtcNow;
        }

        // =========================================================
        // DELETE FEATURE
        // =========================================================

        public async Task DeleteFeatureAsync(
            int featureId)
        {
            var feature =
                await _context.AboutUsFeatures
                    .FirstOrDefaultAsync(x =>
                        x.FeatureId == featureId &&
                        !x.IsDelete);

            if (feature == null)
                return;

            feature.IsDelete = true;
        }
    }
}