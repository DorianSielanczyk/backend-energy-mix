namespace EnergyMix.API.DTOs
{
    public class BestWindowResponse
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double AverageCleanEnergyPercentage { get; set; }
    }
}
