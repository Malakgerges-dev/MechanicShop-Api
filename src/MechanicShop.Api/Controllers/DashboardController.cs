
using Asp.Versioning;
using MechanicShop.Api.Controllers;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderState;
using MediatR;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace mechanicShop.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/dashboard")]
[EnableRateLimiting("SlidingWindow")]

public sealed class DashboardController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(TodayWorkOrderStatsDto),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
    
    public async Task<IActionResult> GetTodayStats([FromQuery]DateOnly? date,CancellationToken ct)
    {
        var statsDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await sender.Send(new GetWorkOrderStatsQuery(statsDate),ct);

        return result.Match
        (
            response=>Ok(response),
            Problem
        );
    }
}