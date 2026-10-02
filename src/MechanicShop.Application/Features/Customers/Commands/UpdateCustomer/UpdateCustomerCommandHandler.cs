using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
public class UpdateCustomerCommandHandler(
    ILogger<UpdateCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache

) : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
    private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.Include(x => x.Vehicles)
                .FirstOrDefaultAsync(x => x.Id == command.CustomerId, cancellationToken);

        if(customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found for update.", command.CustomerId);

            return ApplicationErrors.CustomerNotFound;
        }

        var validateVehicle = new List<Vehicle>();

        foreach (var v in command.Vehicles)
        {
            var vehicleId = v.VehicleId ?? Guid.NewGuid();

            var vehicleResult = Vehicle.Create(
                vehicleId, v.Make, v.Model, v.Year, v.LicensePlate);

            if (vehicleResult.IsError)
            {
                return vehicleResult.Errors;
            }

            validateVehicle.Add(vehicleResult.Value);
        }

        var updateCustomerResult = customer.Update(command.Name, command.Email, command.PhoneNumber);

        if (updateCustomerResult.IsError)
        {
            return updateCustomerResult.Errors;
        }

        var upsertPartResult = customer.UpsertVehicle(validateVehicle);

        if (upsertPartResult.IsError)
        {
            return upsertPartResult.Errors;
        }

        await _context.SaveChangeAsync(cancellationToken);

        await _cache.RemoveByTagAsync("customer", cancellationToken);

        return Result.updated;
    }
}