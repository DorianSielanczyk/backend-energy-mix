using EnergyMix.API.DTOs;
using EnergyMix.API.Services;
using Moq;

namespace Tests;

public class EnergyMixTests
{
    [Fact]
    public async Task GetDailySummariesAsync_WithValidData_ReturnsCorrectAverages()
    {
        var mockCarbonService = new Mock<ICarbonIntensityService>();

        var fakeResponse = new CarbonIntensityResponse
        {
            Data =
            [
                new CarbonIntensityData
                {
                    From = "2026-01-01T10:00Z",
                    To = "2026-01-01T10:30Z",
                    GenerationMix = [ new GenerationMix { Fuel = "wind", Perc = 40 }, new GenerationMix { Fuel = "gas", Perc = 60 } ]
                },
                new CarbonIntensityData
                {
                    From = "2026-01-01T10:30Z",
                    To = "2026-01-01T11:00Z",
                    GenerationMix = [ new GenerationMix { Fuel = "wind", Perc = 80 }, new GenerationMix { Fuel = "gas", Perc = 20 } ]
                }
            ]
        };

        mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(fakeResponse);

        var service = new EnergyMixService(mockCarbonService.Object);

        var results = await service.GetDailySummariesAsync();

        Assert.NotNull(results);
        Assert.Single(results); 

        var dayResult = results.First();

        Assert.Equal(60, dayResult.CleanEnergyPercentage);
        Assert.Equal(60, dayResult.AverageGenerationByFuel["wind"]);
        Assert.Equal(40, dayResult.AverageGenerationByFuel["gas"]);
    }

    [Fact]
    public async Task GetBestChargingWindowAsync_WithValidData_ReturnsBestContiguousWindow()
    {
        var mockCarbonService = new Mock<ICarbonIntensityService>();

        var fakeResponse = new CarbonIntensityResponse
        {
            Data =
            [
                new() { From = "2026-01-01T10:00Z", To = "2026-01-01T10:30Z", GenerationMix = [ new() { Fuel = "solar", Perc = 20 } ] },
                new() { From = "2026-01-01T10:30Z", To = "2026-01-01T11:00Z", GenerationMix = [ new() { Fuel = "solar", Perc = 90 } ] },
                new() { From = "2026-01-01T11:00Z", To = "2026-01-01T11:30Z", GenerationMix = [ new() { Fuel = "solar", Perc = 80 } ] },
                new() { From = "2026-01-01T11:30Z", To = "2026-01-01T12:00Z", GenerationMix = [ new() { Fuel = "solar", Perc = 10 } ] }
            ]
        };

        mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(fakeResponse);

        var service = new EnergyMixService(mockCarbonService.Object);

        var bestWindow = await service.GetBestChargingWindowAsync(chargingHours: 1);

        Assert.NotNull(bestWindow);

        Assert.Equal(85.0, bestWindow.AverageCleanEnergyPercentage);
        Assert.Equal(DateTime.Parse("2026-01-01T10:30Z").ToUniversalTime(), bestWindow.StartTime);
        Assert.Equal(DateTime.Parse("2026-01-01T11:30Z").ToUniversalTime(), bestWindow.EndTime);
    }

    [Fact]
    public async Task GetDailySummariesAsync_ApiReturnsNull_ReturnsEmptyList()
    {
        var mockCarbonService = new Mock<ICarbonIntensityService>();

        mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((CarbonIntensityResponse?)null);

        var service = new EnergyMixService(mockCarbonService.Object);

        var result = await service.GetDailySummariesAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(0)]   
    [InlineData(-5)]  
    [InlineData(7)]   
    public async Task GetBestChargingWindowAsync_HoursOutOfRange_ThrowsArgumentException(int invalidHours)
    {
        var mockCarbonService = new Mock<ICarbonIntensityService>();
        var service = new EnergyMixService(mockCarbonService.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetBestChargingWindowAsync(invalidHours));

        Assert.Contains("Czas ładowania musi wynosić od 1 do 6 godzin", exception.Message);
    }

    [Fact]
    public async Task GetBestChargingWindowAsync_NotEnoughIntervalsForRequestedWindow_ThrowsInvalidOperationException()
    {
        var mockCarbonService = new Mock<ICarbonIntensityService>();

        var fakeResponse = new CarbonIntensityResponse
        {
            Data =
            [
                new() { From = "2026-01-01T10:00Z", To = "2026-01-01T10:30Z", GenerationMix = [] },
                new() { From = "2026-01-01T10:30Z", To = "2026-01-01T11:00Z", GenerationMix = [] }
            ]
        };

        mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(fakeResponse);

        var service = new EnergyMixService(mockCarbonService.Object);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetBestChargingWindowAsync(chargingHours: 2));

        Assert.Contains("Niewystarczająca liczba danych", exception.Message);
    }

    [Fact]
    public async Task GetBestChargingWindowAsync_ApiReturnsNullData_ReturnsNull()
    {
        var mockCarbonService = new Mock<ICarbonIntensityService>();

        mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new CarbonIntensityResponse { Data = [] });

        var service = new EnergyMixService(mockCarbonService.Object);

        var result = await service.GetBestChargingWindowAsync(2);

        Assert.Null(result);
    }
}
