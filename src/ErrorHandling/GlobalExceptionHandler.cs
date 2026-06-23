using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EnergyMix.API.ErrorHandling;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Wyst¹pi³ nieoczekiwany b³¹d w aplikacji: {Message}", exception.Message);

        var statusCode = exception switch
        {
            HttpRequestException => StatusCodes.Status503ServiceUnavailable, 
            JsonException => StatusCodes.Status502BadGateway,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError 
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "Wyst¹pi³ b³¹d po stronie serwera.",
            Detail = "Nie uda³o siê zrealizowaæ zapytania. Spróbuj ponownie póŸniej."
        };

        httpContext.Response.StatusCode = statusCode;
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}