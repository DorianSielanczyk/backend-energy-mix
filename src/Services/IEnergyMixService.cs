using EnergyMix.API.DTOs;

namespace EnergyMix.API.Services
{
    public interface IEnergyMixService
    {
        Task<List<DailySummaryResponse>> GetDailySummariesAsync();
        Task<BestWindowResponse?> GetBestChargingWindowAsync(int chargingHours);

    }
}
