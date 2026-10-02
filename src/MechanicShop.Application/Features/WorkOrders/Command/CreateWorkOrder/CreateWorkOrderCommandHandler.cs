using System.Net.Mail;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.EventHandler;
using MechanicShop.Application.Features.WorkOrders.Mapper;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Command.CreateWorkOrder;
public class CreateWorkOrderCommandHandler(
    ILogger<CreateWorkOrderCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy policy
) : IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
{
    private readonly ILogger<CreateWorkOrderCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _policy = policy;

    public async Task<Result<WorkOrderDto>> Handle(CreateWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var repairTask = await _context.RepairTasks
            .Where(t => command.RepairTaskIds.Contains(t.Id)).ToListAsync();

        if(repairTask.Count != command.RepairTaskIds.Count)
        {
            var missingIds = command.RepairTaskIds.Except(repairTask.Select(t => t.Id)).ToArray();
            _logger.LogError("Some RepairTaskIds not found: {MissingIds}", string.Join(", ", missingIds));
            return ApplicationErrors.RepairTaskNotFound;
        }

        var totalEstimatedDuration = TimeSpan
            .FromMinutes(repairTask.Sum(r => (int)r.EstimatedDurationInMins));

        var endAt = command.StartAt.Add(totalEstimatedDuration);

        if (_policy.IsOutsideOperatingHours(command.StartAt, totalEstimatedDuration))
        {
            _logger.LogError("The WorkOrder time ({StartAt} ? {EndAt}) is outside of store operating hours.", command.StartAt, endAt);

            return ApplicationErrors.WorkOrderOutsideOperatingHour(command.StartAt, endAt);
        }

        var checkMinimumRequirementResult = _policy.ValidateMinimumRequirement(command.StartAt, endAt);

        if (checkMinimumRequirementResult.IsError)
        {
            _logger.LogError("WorkOrder duration is shorter than the configured minimum.");

            return checkMinimumRequirementResult.Errors;
        }

        var checkSpotAvailabilityResult = await _policy.CheckSpotAvailabilityAsync(
            command.Spot, command.StartAt, endAt, excludeWorkOrderId: null, cancellationToken);

        if (checkSpotAvailabilityResult.IsError)
        {
            _logger.LogError("Spot: {Spot} is not available.", command.Spot.ToString());
            return checkSpotAvailabilityResult.Errors;
        }

        var vehicle = await _context.Vehicles.Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == command.VehicleId, cancellationToken);

        if(vehicle is null)
        {
            _logger.LogError("Vehicle with Id '{VehicleId}' does not exist.", command.VehicleId);
            return ApplicationErrors.VehicleNotFound;
        }

        var labor = await _context.Employees.FirstOrDefaultAsync(e => e.Id == command.LaborId, cancellationToken);

        if(labor is null)
        {
            _logger.LogError("Labor with Id '{VehicleId}' does not exist.", command.LaborId);
            return ApplicationErrors.LaborNotFound;
        }

        var hasVehicleConflict = await _context.WorkOrders
        .AnyAsync(
            a =>
            a.VehicleId == command.VehicleId &&
            a.StartAtUtc.Date == command.StartAt.Date &&
            a.StartAtUtc < endAt &&
            a.EndAtUtc > command.StartAt,
            cancellationToken);

        if (hasVehicleConflict)
        {
            _logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", command.VehicleId);
            return Error.Conflict(
                code: "Vehicle_Overlapping_WorkOrders",
                description: "The vehicle already has an overlapping WorkOrder.");
        }

        var isLaborOccupied = await _context.WorkOrders
        .AnyAsync(
            a =>
            a.LaborId == command.LaborId &&
            a.StartAtUtc.Date == command.StartAt.Date &&
            a.StartAtUtc < endAt &&
            a.EndAtUtc > command.StartAt,
            cancellationToken);

        if (isLaborOccupied)
        {
            _logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", command.LaborId);
            return Error.Conflict(
                code: "Labor_Occupied",
                description: "Labor is already occupied during the requested time.");
        }

        var createWorkOrderResult = WorkOrder.Create(
            Guid.NewGuid(),
            command.VehicleId,
            command.StartAt,
            endAt,
            command.LaborId!.Value,
            command.Spot,
            repairTask);

        if (createWorkOrderResult.IsError)
        {
            _logger.LogError("Failed to create WorkOrder: {Error}", createWorkOrderResult.TopError.Description);

            return createWorkOrderResult.Errors;
        }

        var workOrder = createWorkOrderResult.Value;

        _context.WorkOrders.Add(workOrder);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangeAsync(cancellationToken);

        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;

        _logger.LogInformation("WorkOrder with Id '{WorkOrderId}' created successfully.", workOrder.Id);

        await _cache.RemoveByTagAsync("work-order", cancellationToken);

        return workOrder.ToDto();
    }
}