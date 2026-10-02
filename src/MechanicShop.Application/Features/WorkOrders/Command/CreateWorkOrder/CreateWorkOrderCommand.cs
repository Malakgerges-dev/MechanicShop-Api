
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Command.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(
    Guid VehicleId,
    Spot Spot,
    DateTimeOffset StartAt,
    List<Guid> RepairTaskIds,
    Guid?LaborId
) : IRequest<Result<WorkOrderDto>>;