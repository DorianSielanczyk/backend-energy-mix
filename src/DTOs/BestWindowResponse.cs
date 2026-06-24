namespace EnergyMix.API.DTOs

{
    public record BestWindowResponse (
        DateTime StartTime, 
        DateTime EndTime,
        double AverageCleanEnergyPercentage
        );
}
