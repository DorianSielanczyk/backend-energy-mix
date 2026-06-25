using EnergyMix.API.DTOs;
using EnergyMix.API.Services;
using Moq;

namespace Tests;

public class EnergyMixTests
{
    private readonly Mock<ICarbonIntensityService> _mockCarbonService;
    private readonly EnergyMixService _service;

    public EnergyMixTests()
    {
        _mockCarbonService = new Mock<ICarbonIntensityService>();
        _service = new EnergyMixService(_mockCarbonService.Object);
    }

    [Fact]
    public async Task GetDailySummariesAsync_WithValidData_ReturnsCorrectAverages()
    {
        // Arrange
        var fakeResponse = new CarbonIntensityResponse(
        [
            new("2099-01-01T10:00Z", "2099-01-01T10:30Z", [ new("wind", 40), new("gas", 60) ]),
            new("2099-01-01T10:30Z", "2099-01-01T11:00Z", [ new("wind", 80), new("gas", 20) ])
        ]);

        _mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(fakeResponse);

        // Act
        var results = await _service.GetDailySummariesAsync();

        // Assert
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
        // Arrange
        var fakeResponse = new CarbonIntensityResponse(
        [
            new("2026-01-01T10:00Z", "2026-01-01T10:30Z", [ new("solar", 20) ]),
            new("2026-01-01T10:30Z", "2026-01-01T11:00Z", [ new("solar", 90) ]),
            new("2026-01-01T11:00Z", "2026-01-01T11:30Z", [ new("solar", 80) ]),
            new("2026-01-01T11:30Z", "2026-01-01T12:00Z", [ new("solar", 10) ])
        ]);

        _mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(fakeResponse);

        // Act
        var bestWindow = await _service.GetBestChargingWindowAsync(chargingHours: 1);

        // Assert
        Assert.NotNull(bestWindow);
        Assert.Equal(85.0, bestWindow.AverageCleanEnergyPercentage);
        Assert.Equal(DateTime.Parse("2026-01-01T10:30Z").ToUniversalTime(), bestWindow.StartTime);
        Assert.Equal(DateTime.Parse("2026-01-01T11:30Z").ToUniversalTime(), bestWindow.EndTime);
    }

    [Fact]
    public async Task GetDailySummariesAsync_ApiReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        _mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((CarbonIntensityResponse?)null);

        // Act
        var result = await _service.GetDailySummariesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(7)]
    public async Task GetBestChargingWindowAsync_HoursOutOfRange_ThrowsArgumentException(int invalidHours)
    {
        // Arrange
        // There is no need to set up mock

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetBestChargingWindowAsync(invalidHours));

        // Assert
        Assert.Contains("Czas ładowania musi wynosić od 1 do 6 godzin", exception.Message);
    }

    [Fact]
    public async Task GetBestChargingWindowAsync_NotEnoughIntervalsForRequestedWindow_ThrowsInvalidOperationException()
    {
        // Arrange
        var fakeResponse = new CarbonIntensityResponse(
        [
            new("2026-01-01T10:00Z", "2026-01-01T10:30Z", []),
            new("2026-01-01T10:30Z", "2026-01-01T11:00Z", [])
        ]);

        _mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(fakeResponse);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetBestChargingWindowAsync(chargingHours: 2));

        // Assert
        Assert.Contains("Niewystarczająca liczba danych", exception.Message);
    }

    [Fact]
    public async Task GetBestChargingWindowAsync_ApiReturnsNullData_ReturnsNull()
    {
        // Arrange
        _mockCarbonService
            .Setup(s => s.GetGenerationAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new CarbonIntensityResponse([]));

        // Act
        var result = await _service.GetBestChargingWindowAsync(2);

        // Assert
        Assert.Null(result);
    }
}