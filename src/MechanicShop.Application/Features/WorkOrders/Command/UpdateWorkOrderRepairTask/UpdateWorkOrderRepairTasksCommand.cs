using System.Security.Cryptography.X509Certificates;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.UpdateWorkOrderRepairTasksCommand;



public sealed record UpdateWorkOrderRepairTasksCommand
(
    Guid WorkOrderId,
    Guid[]RepairTasksId
):IRequest<Result<Updated>>;