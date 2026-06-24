using System.Text.Json.Serialization;

namespace EnergyMix.API.DTOs;

public record CarbonIntensityResponse
{
    [JsonPropertyName("data")]
    public List<CarbonIntensityData> Data { get; init; } = []; 
}

public record CarbonIntensityData
{
    [JsonPropertyName("from")]
    public string From { get; init; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; init; } = string.Empty;

    [JsonPropertyName("generationmix")]
    public List<GenerationMix> GenerationMix { get; init; } = [];
}

public record GenerationMix
{
    [JsonPropertyName("fuel")]
    public string Fuel { get; init; } = string.Empty;

    [JsonPropertyName("perc")]
    public double Perc { get; init; }
}