using EnergyMix.API.DTOs;

namespace EnergyMix.API.Services;

public class CarbonIntensityService(HttpClient httpClient) : ICarbonIntensityService
{
    public async Task<CarbonIntensityResponse?> GetGenerationAsync(DateTime from, DateTime to)
    {
        string fromFormatted = from.ToString("yyyy-MM-ddTHH:mmZ");
        string toFormatted = to.ToString("yyyy-MM-ddTHH:mmZ");

        var url = $"generation/{fromFormatted}/{toFormatted}";
        
        return await httpClient.GetFromJsonAsync<CarbonIntensityResponse>(url);
    }
}