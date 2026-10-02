using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.enums;
using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Command.UpdateRepairTask;

public sealed record UpdateRepairTaskCommand(
    Guid RepairTaskId,
    string Name,
    decimal LaborCost,
    RepairDurationInMinutes EstimatedDurationInMinutes,
    List<UpdateRepairTaskPartCommand> Parts

): IRequest<Result<Updated>> ;