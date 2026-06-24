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

            var dailySummaries = new List<DailySummaryResponse>();

            var groupedByDay = response.Data.GroupBy(interval => ParseApiTime(interval.From).Date);

            foreach (var dayGroup in groupedByDay)
            {
                var dailyDictionary = new Dictionary<string, double>();
                var fuelTypes = dayGroup.SelectMany(i => i.GenerationMix).Select(f => f.Fuel).Distinct();

                foreach (var fuel in fuelTypes)
                {
                    double averageForFuel = dayGroup
                        .SelectMany(i => i.GenerationMix)
                        .Where(f => f.Fuel == fuel)
                        .Average(f => f.Perc);

                    dailyDictionary[fuel] = Math.Round(averageForFuel, 2);
                }

                var cleanEnergyPercentage = dailyDictionary
               .Where(kvp => Enum.TryParse<CleanEnergySource>(kvp.Key, ignoreCase: true, out _))
               .Sum(kvp => kvp.Value);

                var summary = new DailySummaryResponse(dayGroup.Key, cleanEnergyPercentage, dailyDictionary);
                dailySummaries.Add(summary);
            }

            return dailySummaries;
        }


        public async Task<BestWindowResponse?> GetBestChargingWindowAsync(int chargingHours)
        {
            if (chargingHours < 1 || chargingHours > 6)
            {
                throw new ArgumentException("Czas ładowania musi wynosić od 1 do 6 godzin.");
            }

            var now = DateTime.UtcNow;
            var response = await carbonService.GetGenerationAsync(now, now.AddHours(48));

            var intervals = response?.Data;
            if (intervals == null || intervals.Count == 0) return null;

            int windowSize = chargingHours * 2;

            if (intervals.Count < windowSize)
            {
                throw new InvalidOperationException("Niewystarczająca liczba danych do znalezienia okna.");
            }

            double maxCleanEnergyAvg = -1;
            BestWindowResponse? bestWindow = null;

            for (int i = 0; i <= intervals.Count - windowSize; i++)
            {
                var windowIntervals = intervals.Skip(i).Take(windowSize).ToList();
                double sumOfCleanEnergyInWindow = 0;

                foreach (var interval in windowIntervals)
                {
                    var cleanEnergyInInterval = interval.GenerationMix
                        .Where(x => Enum.TryParse<CleanEnergySource>(x.Fuel, ignoreCase: true, out _))
                        .Sum(x => x.Perc);

                    sumOfCleanEnergyInWindow += cleanEnergyInInterval;
                }

                double windowAverage = sumOfCleanEnergyInWindow / windowSize;

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

        private static DateTime ParseApiTime(string isoTimeString)
        {
            return DateTime.Parse(isoTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }
}
