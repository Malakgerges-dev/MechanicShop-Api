
using MechanicShop.Contracts.Response;
using MechanicShop.Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace mechanicShop.Api.EndPoints;


public static class SettingsEndpoints
{

    public static void MapSettingsEndPoints(this IEndpointRouteBuilder app)
    {
        var endpoint = app.MapGroup("/api/settings")
            .WithOpenApi()
            .RequireRateLimiting("SlidingWindow")
            .RequireAuthorization();

        endpoint.MapGet("/operating-hours",GetOperateHours)
            .WithName("GetOperatingHours")
            .WithSummary("Retrieves the application's operating hours.")
            .WithDescription("Returns the configured opening and closing times for the system.")
            .Produces<OperatingHoursResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
 
    private static IResult GetOperateHours(IOptions<AppSettings> options)
    {
        return Results.Ok(new OperatingHoursResponse( options.Value.OpeningTime,options.Value.ClosingTime));
    }
}