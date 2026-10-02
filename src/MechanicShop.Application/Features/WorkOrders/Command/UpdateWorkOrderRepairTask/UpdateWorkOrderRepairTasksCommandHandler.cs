using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.enums;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.UpdateWorkOrderRepairTasksCommand;

public sealed class UpdateWorkOrderRepairTasksCommandHandler(
    ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy workOrderValidator
) : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
    private readonly ILogger<UpdateWorkOrderRepairTasksCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _workOrderValidator = workOrderValidator;

    public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand command, CancellationToken cancellationToken)
    {
       var workOrder = await _context.WorkOrders
            .Include(w => w.RepairTasks)
            .FirstOrDefaultAsync(w => w.Id == command.WorkOrderId);

       if(workOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

       if(command.RepairTasksId.Length == 0)
        {
            _logger.LogError("Empty RepairTaskIds list submitted.");
            return ApplicationErrors.AtLeastOneRepairTaskIsRequired;
        }

       var repairTask = await _context.RepairTasks
            .Where(r => command.RepairTasksId.Contains(r.Id))
            .ToListAsync();

       if(repairTask.Count != command.RepairTasksId.Length)
        {
            var missingIds = command.RepairTasksId.Except(repairTask.Select(r => r.Id).ToList());

            _logger.LogError("One or More RepairTask Not Found.{ids}", string.Join(",", missingIds));

            return ApplicationErrors.RepairTaskNotFound;
        }

       var clearExistingResult = workOrder.ClearRepairTasks();

       if (clearExistingResult.IsError)
        {
            return clearExistingResult;
        }

       foreach (var task in repairTask)
        {
            var addExistingResult = workOrder.AddRepairTask(task);

            if (addExistingResult.IsError)
            {
                return addExistingResult;
            }
        }

       var totalDuration = TimeSpan.FromMinutes(repairTask.Sum(x => (int)x.EstimatedDurationInMins));

       var newEnd = workOrder.StartAtUtc + totalDuration;

       if (_workOrderValidator.IsOutsideOperatingHours(workOrder.StartAtUtc, totalDuration))
        {
            Error.Conflict("WorkOrder_Outside_OperatingHours", "WorkOrder timing exceeds business hours.");
        }

       var spotCheckResult = await _workOrderValidator.CheckSpotAvailabilityAsync(
            workOrder.Spot,
            workOrder.StartAtUtc,
            newEnd,
            workOrder.Id,
            cancellationToken);

       if (spotCheckResult.IsError)
        {
            return spotCheckResult.Errors;
        }

       if(await _workOrderValidator.IsLaborOccupied(workOrder.LaborId, workOrder.Id, workOrder.StartAtUtc, newEnd))
        {
            return ApplicationErrors.LaborOccupied;
        }

       workOrder.UpdateTiming(workOrder.StartAtUtc, newEnd);

       workOrder.AddDomainEvent(new WorkOrderCollectionModified());

       await _context.SaveChangeAsync(cancellationToken);

       workOrder.AddDomainEvent(new WorkOrderCollectionModified());

       await _cache.RemoveByTagAsync("work-order", cancellationToken);

       return Result.updated;
    }
}