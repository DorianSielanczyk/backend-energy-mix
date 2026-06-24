using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EnergyMix.API.ErrorHandling;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Wyst¹pi³ wyj¹tek: {Message}", exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            ArgumentException or InvalidOperationException =>
                (StatusCodes.Status400BadRequest, "Nieprawid³owe ¿¹danie", exception.Message),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Nie znaleziono zasobu", exception.Message),

            HttpRequestException or TaskCanceledException or TimeoutException =>
                (StatusCodes.Status503ServiceUnavailable, "B³¹d us³ugi zewnêtrznej", "Problem z po³¹czeniem z zewnêtrznym dostawc¹ danych"),

            JsonException =>
                (StatusCodes.Status502BadGateway, "B³¹d bramy", "Nieprawid³owy format danych od dostawcy"),

            // Other exceptions
            _ =>
                (StatusCodes.Status500InternalServerError, "Wyst¹pi³ b³¹d serwera", "Nie uda³o siê zrealizowaæ zapytania. Spróbuj ponownie póŸniej.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}