using EnergyMix.API.DTOs;

namespace EnergyMix.API.Services;

public class CarbonIntensityService(HttpClient httpClient) : ICarbonIntensityService
{
    public async Task<CarbonIntensityResponse?> GetGenerationAsync(DateTime from, DateTime to)
    {
        string fromFormatted = ConvertPolishTimeToUtc(from).ToString("yyyy-MM-ddTHH:mmZ");
        string toFormatted = ConvertPolishTimeToUtc(to).ToString("yyyy-MM-ddTHH:mmZ");

        var url = $"generation/{fromFormatted}/{toFormatted}";
        
        return await httpClient.GetFromJsonAsync<CarbonIntensityResponse>(url);
    }

    private static DateTime ConvertPolishTimeToUtc(DateTime date)
    {
        if (date.Kind == DateTimeKind.Utc)
        {
            return date;
        }

        var unspecifiedDate = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);

        var timeZoneId = OperatingSystem.IsWindows() 
            ? "Central European Standard Time" 
            : "Europe/Warsaw";

        try
        {
            var polishTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(unspecifiedDate, polishTimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return unspecifiedDate.ToUniversalTime();
        }
    }
}