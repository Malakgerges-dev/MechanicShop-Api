using Asp.Versioning;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.Features.RepairTasks.Command.CreateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Command.RemoveRepairTask;
using MechanicShop.Application.Features.RepairTasks.Command.UpdateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTask;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks.enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace MechanicShop.Api.Controllers;


[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/repair-tasks")]
[EnableRateLimiting("SlidingWindow")]
[Authorize]

public sealed class RepairTasksController(ISender sender,IOutputCacheStore cach) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<RepairTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves all repair tasks.")]
    [EndpointDescription("Returns a list of all repair tasks available in the system.")]
    [EndpointName("GetRepairTasks")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60, Tags =["RepairTasks"])]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTaskQuery(),ct);

        return result.Match
        (
            response=>Ok(response),
            Problem
        );
    }

    [HttpGet("{repairTaskId:guid}",Name =nameof(GetById))]
    [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a repair task by ID.")]
    [EndpointDescription("Returns detailed information for the specified repair task if it exists.")]
    [EndpointName("GetRepairTaskById")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60, Tags =["RepairTasks"])]
    public async Task<IActionResult> GetById(Guid repairTaskId,CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTaskByIdQuery(repairTaskId),ct);

        return result.Match
        (
            response=>Ok(response),
            Problem
        );
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new repair task.")]
    [EndpointDescription("Creates a repair task and optionally includes parts.")]
    [EndpointName("CreateRepairTask")]
    [MapToApiVersion("1.0")]

    public async Task<IActionResult> CreateRepairTask([FromBody]CreateRepairTaskCommand request,CancellationToken ct)
    {
        var parts = request.Parts
            .ConvertAll(P=>new CreateRepairTaskPartCommand(P.Name,P.Cost,P.Quantity));
        
        var command = new CreateRepairTaskCommand
        (
            request.Name,
            request.LaborCost,
            request.EstimatedDurationInMinutes is not null ?
                 (RepairDurationInMinutes)request.EstimatedDurationInMinutes : null,
            parts);

        var result = await sender.Send(command,ct);

        await cach.EvictByTagAsync("RepairTasks",ct);

        return result.Match
        (
            response=>CreatedAtAction(nameof(GetById),new{repairTaskId = response.RepairTaskId,response}),
            Problem
        );
    }

    [HttpPut("{repairTaskId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Updates an existing repair task.")]
    [EndpointDescription("Updates a repair task and its associated parts.")]
    [EndpointName("UpdateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update
    (Guid repairTaskId,
    [FromBody] UpdateRepairTaskCommand request,
    CancellationToken ct)
    {
        var parts = request.Parts
            .ConvertAll(P=>new UpdateRepairTaskPartCommand(P.PartId,P.Name,P.Cost,P.Quantity));
        
        var command = new UpdateRepairTaskCommand
        (
            repairTaskId,
            request.Name,
            request.LaborCost,
            (RepairDurationInMinutes)request.EstimatedDurationInMinutes!,
            parts);

        var result = await sender.Send(command,ct);   

        await cach.EvictByTagAsync("RepairTasks",ct);


        return result.Match
        (
            response=>Ok(response),
            Problem
        );
    }

    [HttpDelete("{repairTaskId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Removes a repair task.")]
    [EndpointDescription("Deletes the specified repair task from the system.")]
    [EndpointName("RemoveRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid repairTaskId,CancellationToken ct)
    {
        var result = await sender.Send(new RemoveRepairTaskCommand(repairTaskId),ct);

        await cach.EvictByTagAsync("RepairTasks",ct);


        return result.Match
        (
            _=>NoContent(),
            Problem
        );
    }
}