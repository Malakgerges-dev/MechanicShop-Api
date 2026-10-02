using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Command.RelocateWorkOrder;


public class RelocateWorkOrderCommandHandler(
    ILogger<RelocateWorkOrderCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy workOrderPolicy
) :
IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
    private readonly ILogger<RelocateWorkOrderCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;

    public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .Include(W=>W.RepairTasks)
           .Include(W=>W.Labor)
           .Include(W=>W.Vehicle)
           .FirstOrDefaultAsync(W=>W.Id == command.WorkOrderId,cancellationToken);

        if(workOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();

        var endAt = command.NewStartAt.Add(duration);
        
        
        var checkSpotAvailabilityResult = await _workOrderPolicy.CheckSpotAvailabilityAsync(
            workOrder.Spot,
            command.NewStartAt,
            endAt,
            workOrder.Id,
            cancellationToken
        );

        if (checkSpotAvailabilityResult.IsError)
        {
            return checkSpotAvailabilityResult.Errors;
        }

        if (await _workOrderPolicy.IsLaborOccupied(workOrder.LaborId, command.WorkOrderId, command.NewStartAt, endAt))
        {
            _logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", workOrder.LaborId);

            return ApplicationErrors.LaborOccupied;
        }

          if (await _workOrderPolicy.IsVehicleAlreadyScheduled(workOrder.VehicleId, command.NewStartAt, endAt, command.WorkOrderId))
        {
            _logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", workOrder.VehicleId);

            return ApplicationErrors.VehicleSchedulingConflict;
        }

        var UpdateTimingResult = workOrder.UpdateTiming(command.NewStartAt,endAt);

        if (UpdateTimingResult.IsError)
        {
            _logger.LogError("Failed to update timing: {Error}", UpdateTimingResult.TopError.Description);

            return UpdateTimingResult.Errors;
        }

        var UpdateSpotResult = workOrder.UpdateSpot(command.NewSpot);

        if (UpdateSpotResult.IsError)
        {
            _logger.LogError("Failed to update Spot: {Error}", UpdateSpotResult.TopError.Description);

            return UpdateSpotResult.Errors;
        }

        
        workOrder.AddDomainEvent(new WorkOrderCollectionModified());
        
        await _context.SaveChangeAsync(cancellationToken);


        await _cache.RemoveByTagAsync("work-order");

        return Result.updated;


    }
}