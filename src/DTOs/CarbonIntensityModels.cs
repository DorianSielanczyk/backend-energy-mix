using System.Text.Json.Serialization;

namespace EnergyMix.API.DTOs;

public record CarbonIntensityResponse(
    [property: JsonPropertyName("data")] List<CarbonIntensityData> Data
);

public record CarbonIntensityData(
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("generationmix")] List<GenerationMix> GenerationMix
);

public record GenerationMix(
    [property: JsonPropertyName("fuel")] string Fuel,
    [property: JsonPropertyName("perc")] double Perc
);