

using MechanicShop.Domain.Common;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Domain.WorkOrders;

public sealed class WorkOrder : AuditableEntity
{
    public Guid VehicleId { get; }
    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset EndAtUtc { get; private set; }
    public Guid LaborId { get; private set; }
    public Spot Spot { get; private set; }
    public WorkOrderState State { get; private set; }

    public decimal? Discount { get; private set; }

    public decimal? Tax { get; private set; }
    public Employee? Labor { get; set; }
    public Vehicle? Vehicle { get; set; }
    public Invoice? Invoice { get; set; }

    private readonly List<RepairTask> _RepairTasks = [];
    public IEnumerable<RepairTask> RepairTasks => _RepairTasks.AsReadOnly();

    public decimal? TotalPartCost => _RepairTasks.SelectMany(R => R.Parts).Sum(P => P.Cost * P.Quantity);
    public decimal? TotalLaborCost => _RepairTasks.Sum(R => R.LaborCost);
    public decimal? Total => (TotalLaborCost ?? 0) + (TotalPartCost ?? 0);

    private WorkOrder() { }

    private WorkOrder(Guid id, Guid vehicleId, DateTimeOffset startAt,
        DateTimeOffset endAt, Guid laborId, Spot spot, WorkOrderState state, List<RepairTask> repairTasks)
    : base(id)
    {
        VehicleId = vehicleId;
        StartAtUtc = startAt;
        EndAtUtc = endAt;
        LaborId = laborId;
        Spot = spot;
        State = state;
        _RepairTasks = repairTasks;

    }
    public static Result<WorkOrder> Create(Guid id, Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt,
     Guid laborId, Spot spot, List<RepairTask> repairTasks)
    {
        if (id == Guid.Empty)
        {
            return WorkOrderErrors.WorkOrderIdRequired;
        }

        if(vehicleId == Guid.Empty)
        {
            return WorkOrderErrors.VehicleIdRequired;
        }

        if(repairTasks is null || repairTasks.Count == 0)
        {
            return WorkOrderErrors.RepairTaskRequired;
        }

        if(laborId == Guid.Empty)
        {
            return WorkOrderErrors.LaborIdRequired;
        }

        if(endAt <= startAt)
        {
            return WorkOrderErrors.InvalidTiming;
        }

        if (!Enum.IsDefined(spot))
        {
            return WorkOrderErrors.SpotInvalid;
        }

        return new WorkOrder(id, vehicleId, startAt, endAt, laborId, spot, WorkOrderState.Scheduled, repairTasks);
    }
    public bool IsEditable => State is not(WorkOrderState.InProgress or WorkOrderState.Completed or WorkOrderState.Cancelled);

    public Result<Updated> AddRepairTask (RepairTask repairTask)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.Readonly;
        }

        if(_RepairTasks.Any(r => r.Id == repairTask.Id))
        {
            return WorkOrderErrors.RepairTaskAlreadyAdded;
        }

        _RepairTasks.Add(repairTask);

        return Result.updated;
    }

    public Result<Updated> UpdateTiming(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.TimingReadOnly(Id.ToString(), State);
        }

        if(endAt <= startAt)
        {
            return WorkOrderErrors.InvalidTiming;
        }

        StartAtUtc = startAt;
        EndAtUtc = endAt;

        return Result.updated;
    }

    public Result<Updated> UpdateLabor(Guid laborId)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.Readonly;
        }

        if(laborId == Guid.Empty)
        {
            return WorkOrderErrors.LaborIdEmpty(Id.ToString());
        }

        LaborId = laborId;

        return Result.updated;
    }

    public Result<Updated> UpdateSpot(Spot newSpot)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.Readonly;
        }

        if (!Enum.IsDefined(newSpot))
        {
            return WorkOrderErrors.SpotInvalid;
        }

        Spot = newSpot;

        return Result.updated;
    }

    public bool CanTransitionTo(WorkOrderState NewState)
    {
        return (State, NewState) switch
        {
            (WorkOrderState.Scheduled, WorkOrderState.InProgress)=>true,
            (WorkOrderState.InProgress, WorkOrderState.Completed)=>true,
            (_, WorkOrderState.Cancelled) when State != WorkOrderState.Completed => true,
            _=>false
        };
    }

    public Result<Updated> Cancel()
    {
        if (!CanTransitionTo(WorkOrderState.Cancelled))
        {
            return WorkOrderErrors.InvalidStateTransition(State, WorkOrderState.Cancelled);
        }

        State = WorkOrderState.Cancelled;

        return Result.updated;
    }

    public Result<Updated> ClearRepairTasks()
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.Readonly;
        }

        _RepairTasks.Clear();

        return Result.updated;
    }

    public Result<Updated> UpdateState(WorkOrderState newState)
    {
        if (!CanTransitionTo(newState))
        {
            return WorkOrderErrors.InvalidStateTransition(State, newState);
        }

        State = newState;
        return Result.updated;
    }
}