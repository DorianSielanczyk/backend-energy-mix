namespace EnergyMix.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureHttpRequestPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();
        app.UseHttpsRedirection();

        app.UseCors("StrictCorsPolicy");

        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}