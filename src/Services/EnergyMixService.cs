using EnergyMix.API.DTOs;
using System.Globalization;
using EnergyMix.API.Enums;

namespace EnergyMix.API.Services
{
    public class EnergyMixService(ICarbonIntensityService carbonService) : IEnergyMixService
    {
        public async Task<List<DailySummaryResponse>> GetDailySummariesAsync()
        {
            var today = DateTime.UtcNow.Date;
            var endOfNextTwoDays = today.AddDays(3);

            var response = await carbonService.GetGenerationAsync(today, endOfNextTwoDays);
            if (response?.Data == null) return [];

            return response.Data
                .Where(interval => ParseApiTime(interval.From).Date >= today)
                .GroupBy(interval => ParseApiTime(interval.From).Date)
                .Select(CalculateDailySummary)
                .ToList();
        }

        public async Task<BestWindowResponse?> GetBestChargingWindowAsync(int chargingHours)
        {
            if (chargingHours is < 1 or > 6)
                throw new ArgumentException("Czas ładowania musi wynosić od 1 do 6 godzin.");

            var now = DateTime.UtcNow;
            var response = await carbonService.GetGenerationAsync(now, now.AddHours(48));
            var intervals = response?.Data;

            if (intervals == null || intervals.Count == 0) return null;

            int windowSize = chargingHours * 2;
            if (intervals.Count < windowSize)
                throw new InvalidOperationException("Niewystarczająca liczba danych do znalezienia okna.");

            return FindBestContiguousWindow(intervals, windowSize);
        }

        private DailySummaryResponse CalculateDailySummary(IGrouping<DateTime, CarbonIntensityData> dayGroup)
        {
            var dailyDictionary = new Dictionary<string, double>();
            var mixesGroupedByFuel = dayGroup
                  .SelectMany(i => i.GenerationMix)
                  .GroupBy(m => m.Fuel);

            foreach (var fuelGroup in mixesGroupedByFuel)
            {
                double averageForFuel = fuelGroup.Average(f => f.Perc);
                dailyDictionary[fuelGroup.Key] = Math.Round(averageForFuel, 2);
            }

            var cleanEnergyPercentage = Math.Round(
                dailyDictionary
                    .Where(kvp => IsCleanEnergy(kvp.Key))
                    .Sum(kvp => kvp.Value), 2);

            return new DailySummaryResponse(dayGroup.Key, cleanEnergyPercentage, dailyDictionary);
        }

        private BestWindowResponse? FindBestContiguousWindow(List<CarbonIntensityData> intervals, int windowSize)
        {
            double maxCleanEnergyAvg = -1;
            BestWindowResponse? bestWindow = null;

            for (int i = 0; i <= intervals.Count - windowSize; i++)
            {
                var windowIntervals = intervals.Skip(i).Take(windowSize).ToList();

                double windowAverage = CalculateWindowAverage(windowIntervals) / windowSize;

                if (windowAverage > maxCleanEnergyAvg)
                {
                    maxCleanEnergyAvg = windowAverage;
                    bestWindow = new BestWindowResponse(
                        ParseApiTime(windowIntervals.First().From),
                        ParseApiTime(windowIntervals.Last().To),
                        Math.Round(windowAverage, 2)
                    );
                }
            }

            return bestWindow;
        }

        private double CalculateWindowAverage(List<CarbonIntensityData> windowIntervals)
        {
            return windowIntervals.Sum(interval =>
                interval.GenerationMix
                    .Where(x => IsCleanEnergy(x.Fuel))
                    .Sum(x => x.Perc));
        }

        private static DateTime ParseApiTime(string isoTimeString)
        {
            return DateTime.Parse(isoTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        private static bool IsCleanEnergy(string fuel)
        {
            return Enum.TryParse<CleanEnergySource>(fuel, ignoreCase: true, out _);
        }
    }
}
