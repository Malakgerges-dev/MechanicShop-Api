using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;


public class RemoveCustomerCommandHandler(
    ILogger<RemoveCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
) : IRequestHandler<RemoveCustomerCommand, Result<Deleted>>
{
    private readonly ILogger<RemoveCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Deleted>> Handle(RemoveCustomerCommand command, CancellationToken cancellationToken)
    {
        var Customer =  await _context.Customers
            .FirstOrDefaultAsync(C=>C.Id ==command.CustomerId);

        if(Customer is null)
        {
            _logger.LogError("Customer With Id: {CustomerId} Not Found For Deletion.",command.CustomerId);

            return ApplicationErrors.CustomerNotFound;
        }

        var hasAssociatedWorkOrders = await _context.WorkOrders.Include(c=>c.Vehicle)
            .Where(c=>c.Vehicle != null).AnyAsync(c=>c.Vehicle!.CustomerId == command.CustomerId);

        if (hasAssociatedWorkOrders)
        {
             _logger.LogWarning("Customer {CustomerId} cannot be deleted because they have associated work orders (past, scheduled, or in-progress).", command.CustomerId);
            return CustomerErrors.CannotDeleteCustomerWithWorkOrders;
        }

        _context.Customers.Remove(Customer);

        await _context.SaveChangeAsync(cancellationToken);

        await _cache.RemoveByTagAsync("customer",cancellationToken);

        _logger.LogInformation("Customer {CustomerId} deleted successfully.", command.CustomerId);

        return Result.deleted;
    }
}