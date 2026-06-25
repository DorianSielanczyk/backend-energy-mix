using EnergyMix.API.ErrorHandling;
using EnergyMix.API.Services;

namespace EnergyMix.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("StrictCorsPolicy", builder =>
            {
                builder.WithOrigins(allowedOrigins)
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        services.AddHttpClient<ICarbonIntensityService, CarbonIntensityService>(client =>
        {
            client.BaseAddress = new Uri("https://api.carbonintensity.org.uk/");
        });

        services.AddScoped<IEnergyMixService, EnergyMixService>();

        return services;
    }
}