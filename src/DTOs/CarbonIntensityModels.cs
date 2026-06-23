using System.Text.Json.Serialization;

namespace EnergyMix.API.DTOs;

public class CarbonIntensityResponse
{
    [JsonPropertyName("data")]
    public List<CarbonIntensityData> Data { get; set; } = [];
}

public class CarbonIntensityData
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("generationmix")]
    public List<GenerationMix> GenerationMix { get; set; } = [];
}

public class GenerationMix
{
    [JsonPropertyName("fuel")]
    public string Fuel { get; set; } = string.Empty;

    [JsonPropertyName("perc")]
    public double Perc { get; set; }
}