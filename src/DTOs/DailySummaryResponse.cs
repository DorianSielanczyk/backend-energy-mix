namespace EnergyMix.API.DTOs

{
    public record DailySummaryResponse(
        DateTime Date, 
        double CleanEnergyPercentage, 
        Dictionary<string, double> AverageGenerationByFuel
    );
}
