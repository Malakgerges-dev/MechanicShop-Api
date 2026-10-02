using System;

using MechanicShop.Infrastructure;
using MechanicShop.Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiVersioning();

builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(option =>
    {
        option.SwaggerEndpoint("/openapi/v1.json", "MechanicShop API v1");

        option.EnableDeepLinking();
        option.EnableFilter();
        option.DisplayRequestDuration();
    });

    app.MapScalarApiReference();

    await app.InitializeDatabaseAsync();

}
else
{
    app.UseHsts();
}

app.UseCoreMiddlewares(builder.Configuration);

app.MapControllers();

app.Run();
