using EnergyMix.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.ConfigureHttpRequestPipeline();

app.Run();