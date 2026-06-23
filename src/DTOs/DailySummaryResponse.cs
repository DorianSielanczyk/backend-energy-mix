using Microsoft.AspNetCore.Routing.Constraints;

namespace EnergyMix.API.DTOs
{
    public class DailySummaryResponse
    {
        public DateTime Date { get; set; }
        public double CleanEnergyPercentage { get; set; }

        public Dictionary<string, double> AverageGenerationByFuel { get; set; } = [];
    }
}
