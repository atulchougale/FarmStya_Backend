using FarmStay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.Interfaces.Repositories
{
  public interface IAboutUsRepository
    {
        // AboutUs
        Task AddAboutUsAsync(AboutUs aboutUs);
        Task<AboutUs?> GetAboutUsByFarmHouseIdAsync(int farmHouseId);
        Task<AboutUs?> GetAboutUsAsync(int aboutUsId);
        Task UpdateAboutUsAsync(AboutUs aboutUs);
        Task DeleteAboutUsAsync(int aboutUsId);

        // AboutUs Feature
        Task AddFeatureAsync(AboutUsFeature feature);
        Task<AboutUsFeature?> GetFeatureByIdAsync(int featureId);
        Task<List<AboutUsFeature>> GetFeaturesAsync(int aboutUsId);
        Task UpdateFeatureAsync(AboutUsFeature feature);
        Task DeleteFeatureAsync(int featureId);

    }
}
