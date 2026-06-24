using EnergyMix.API.DTOs;

namespace EnergyMix.API.Services;

public class CarbonIntensityService(HttpClient httpClient) : ICarbonIntensityService
{
    const string DateFormat = "yyyy-MM-ddTHH:mmZ"; 
    public async Task<CarbonIntensityResponse?> GetGenerationAsync(DateTime from, DateTime to)
    {
        var fromFormatted = from.ToString(DateFormat); 
        var toFormatted = to.ToString(DateFormat); 

        var url = $"generation/{fromFormatted}/{toFormatted}";
        
        return await httpClient.GetFromJsonAsync<CarbonIntensityResponse>(url);
    }
}