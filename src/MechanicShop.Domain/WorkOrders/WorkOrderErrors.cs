using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Domain.WorkOrders;

public static class WorkOrderErrors
{
    public static Error WorkOrderIdRequired => Error.Validation(
        code: "WorkOrderErrors.WorkOrderIdRequired",
        description: "WorkOrder Id is required");

    public static Error VehicleIdRequired => Error.Validation(
        code: "WorkOrderErrors.VehicleIdRequired",
        description: "Vehicle Id is required");

    public static Error RepairTaskRequired => Error.Validation(
        code: "WorkOrderErrors.RepairTaskRequired",
        description: "At Least One Repair Task is required");

    public static Error LaborIdRequired => Error.Validation(
        code: "WorkOrderErrors.LaborIdRequired",
        description: "Labor Id is required");

    public static Error LaborIdEmpty(string Id) => Error.Validation(
        code: "WorkOrderErrors.LaborIdEmpty",
        description: $"Labor '{Id}':Labor Id is Empty");

    public static Error InvalidTiming => Error.Conflict(
        code: "WorkOrderErrors.InvalidTiming",
        description: "End Time Must Be After Start Time");

    public static Error TimingReadOnly(string id, WorkOrderState state) => Error.Conflict(
        code: "WorkOrderErrors.InvalidTiming",
        description: $"WorkOrder '{id}': Can't Modify timing when WorkOrder status is '{state}'.");

    public static Error SpotInvalid => Error.Validation(
        code: "WorkOrderErrors.SpotInvalid",
        description: "The provided spot is invalid");

    public static Error Readonly => Error.Conflict(
        code: "WorkOrderErrors.Readonly",
        description: "WorkOrder is read-only.");

    public static Error RepairTaskAlreadyAdded => Error.Conflict(
        code: "WorkOrderErrors.RepairTaskAlreadyAdded",
        description: "Repair task already exists.");

    public static Error InvalidStateTransition(WorkOrderState current, WorkOrderState next) => Error.Conflict(
        code: "WorkOrderErrors.InvalidStateTransition",
        description: $"WorkOrder Invalid State transition from '{current}' to '{next}'.");

    public static Error StateTransitionNotAllowed(DateTimeOffset startAtUtc) => Error.Conflict(
            code: "WorkOrderErrors.StateTransitionNotAllowed",
            description: $"State transition is not allowed before the work order’s scheduled start time {startAtUtc:yyyy-MM-dd HH:mm} UTC.");
}