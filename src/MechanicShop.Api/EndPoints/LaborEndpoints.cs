
using Asp.Versioning.Builder;
using mechanicShop.Api.Extension;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Application.Features.Labors.Queries.GetLabor;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace mechanicShop.Api.EndPoints;

public static class LaborEndpoints
{
    public static void MapLaborEndPoints(this IEndpointRouteBuilder app,ApiVersionSet apiVersion)
    {
        var endpoint = app.MapGroup("/api/v{apiVersion:apiVersion}/Labors")
        .WithApiVersionSet(apiVersion)
        .HasApiVersion(1.0)
        .WithOpenApi()
        .RequireRateLimiting("SlidingWindow")
        .RequireAuthorization(policy => policy.RequireRole(Role.Manager.ToString()));
        
        endpoint.MapGet("/",GetLabor)
            .WithName("GetLabor")
            .MapToApiVersion(1.0)
            .WithSummary("Retrieves all labor definitions.")
            .WithDescription("Returns a list of available labor types that can be assigned to work orders.")
            .Produces<List<LaborDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
    private static async Task<IResult> GetLabor(ISender sender,CancellationToken ct)
    {
        var result = await sender.Send(new GetLaborQuery(),ct);

        return result.Match(value=>Results.Ok(value),error=>error.ToProblem());
    }
}