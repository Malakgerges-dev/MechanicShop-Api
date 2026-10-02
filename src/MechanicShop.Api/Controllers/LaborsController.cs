
using Asp.Versioning;
using MechanicShop.Api.Controllers;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Application.Features.Labors.Queries.GetLabor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace mechanicShop.Api.Controllers;


[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/Labors")]
[EnableRateLimiting("SlidingWindow")]

public sealed class LaborsController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<LaborDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves the list of available labor definitions.")]
    [EndpointDescription("Returns all labor records associated with the system, accessible only to users with the Manager role.")]
    [EndpointName("GetLabors")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60)]

    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetLaborQuery(),ct);

        return result.Match
        (
            response=>Ok(response),
            Problem
        );
    }
}

