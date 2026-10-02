using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.enums;
using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Command.CreateRepairTask;

public sealed record CreateRepairTaskCommand(
    string Name,
    decimal LaborCost,
    RepairDurationInMinutes ?EstimatedDurationInMinutes,
    List<CreateRepairTaskPartCommand> Parts

): IRequest<Result<RepairTaskDto>> ;