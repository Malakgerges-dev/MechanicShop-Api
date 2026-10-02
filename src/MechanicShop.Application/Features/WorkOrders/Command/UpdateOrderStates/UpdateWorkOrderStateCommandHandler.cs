
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Command.UpdateOrderStates;

public class UpdateWorkOrderStateCommandHandler(
    ILogger<UpdateWorkOrderStateCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    TimeProvider provider
) : IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
{
    private readonly ILogger<UpdateWorkOrderStateCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly TimeProvider _provider = provider;

    public async Task<Result<Updated>> Handle(UpdateWorkOrderStateCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(W=>W.Id == command.WorkOrderId);

        if(workOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        if(workOrder.StartAtUtc > _provider.GetUtcNow())
        {
            _logger.LogError("State transition for WorkOrder Id '{WorkOrderId}` is not allowed before the work order�s scheduled start time.", command.WorkOrderId);
            return WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc);
        }

        var UpdateState = workOrder.UpdateState(command.State);

        if (UpdateState.IsError)
        {
            return UpdateState.Errors;
        }

        if(command.State == WorkOrderState.Completed)
        {
            workOrder.AddDomainEvent(new WorkOrderCompleted{WorkOrderId = command.WorkOrderId});
        }

        await _context.SaveChangeAsync(cancellationToken);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _cache.RemoveByTagAsync("work-order",cancellationToken);

        return Result.updated;
    }
}