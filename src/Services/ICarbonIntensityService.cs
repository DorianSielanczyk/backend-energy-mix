using EnergyMix.API.DTOs;

namespace EnergyMix.API.Services;

public interface ICarbonIntensityService
{
    Task<CarbonIntensityResponse?> GetGenerationAsync(DateTime from, DateTime to);
}