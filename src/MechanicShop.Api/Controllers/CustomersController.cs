using Asp.Versioning;
using MechanicShop.Api.Controllers;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Queries.GetCustomer;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace mechanicShop.Api.Controllers;


[Route("api/v{version:apiVersion}/customers")]
[ApiVersion("1.0")]
[EnableRateLimiting("SlidingWindow")]
[Authorize]
public sealed class CustomerController(ISender sender,IOutputCacheStore cache) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a list Of customers.")]
    [EndpointDescription("Returns all customers associated with the current user.")]
    [EndpointName("GetCustomer")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [OutputCache(Duration =60,Tags = ["customers"])]

    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomerQuery(),ct);

        return result.Match(
            response => Ok(response),
            Problem
        );
    }
    

    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(CustomerDto),StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Create a new customer.")]
    [EndpointDescription("Add a new customer to the system.")]
    [EndpointName("CreateCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand request,CancellationToken ct)
    {
        var Vehicles = request.Vehicles
            .ConvertAll(V => new CreateVehicleCommand(V.Make,V.Model,V.Year,V.LicensePlate));
        
        var command = new CreateCustomerCommand(
            request.Name,
            request.PhoneNumber,
            request.Email,
            Vehicles
        );

        var result = await sender.Send(command, ct);

        await cache.EvictByTagAsync("customers",ct);

        return result.Match(
            response=>CreatedAtRoute(
                routeName:"GetCustomerById",
                routeValues: new{Version ="1.0", customerId = response.CustomerId},
                value: response
            ),
            Problem
        ); 
    }

    [HttpGet("{CustomerId:guid}",Name ="GetCustomerById")]
    [ProducesResponseType(typeof(CustomerDto),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a customer by ID.")]
    [EndpointDescription("Returns detailed information about tje specified customer if found")]
    [EndpointName("GetCustomerById")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60, Tags = ["customers"])]
    public async Task<IActionResult> GetById(Guid CustomerId,CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(CustomerId),ct);

        return result.Match(
            response=>Ok(response),
            Problem
        );
    }

    [HttpPut("{CustomerId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(CustomerDto),StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update an existing customer.")]
    [EndpointDescription("Update a customer and its associated vehicle.")]
    [EndpointName("UpdateCustomer")]
    [MapToApiVersion("1.0")]

    public async Task<IActionResult> Update(Guid CustomerId,[FromBody]UpdateCustomerCommand request,CancellationToken ct)
    {
        var Vehicles = request.Vehicles.ConvertAll
        (V=> new UpdateVehicleCommand(V.VehicleId,V.Make,V.Model,V.Year,V.LicensePlate));

        var command = new UpdateCustomerCommand
        (
            CustomerId,
            request.Name,
            request.PhoneNumber,
            request.Email,
            Vehicles
        );

        var result = await sender.Send(command,ct);

        await cache.EvictByTagAsync("customers",ct);


        return result.Match
        (
            response=> Ok(response),
            Problem
        );
    }

    [HttpDelete("{CustomerId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Remove a customer.")]
    [EndpointDescription("Deletes the specified customer for the system.")]
    [EndpointName("RemoveCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid CustomerId,CancellationToken ct)
    {
        var result = await sender.Send(new RemoveCustomerCommand(CustomerId),ct);

        await cache.EvictByTagAsync("customers",ct);


        return result.Match
        (
            _=>NoContent(),
            Problem
        );

    }

}