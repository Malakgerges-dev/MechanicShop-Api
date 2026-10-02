using System.Data.Common;

using MechanicShop.Application.Features.WorkOrders.Command.CreateWorkOrder;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Tests.Common.WorkOrders;

public static class WorkOrderCommandFactory
{
    public static CreateWorkOrderCommand CreateCreateWorkOrderCommand(
        Spot? spot = null,
        Guid? vehicleId = null,
        DateTimeOffset? startAt = null,
        List<Guid>? repairTaskIds = null,
        Guid? laborId = null)
    {
            return new CreateWorkOrderCommand(
            vehicleId ?? Guid.NewGuid(),
            spot ?? Spot.A,
            startAt ?? DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(9),
            repairTaskIds ?? [Guid.NewGuid()],
            laborId ?? Guid.NewGuid());
    }
}