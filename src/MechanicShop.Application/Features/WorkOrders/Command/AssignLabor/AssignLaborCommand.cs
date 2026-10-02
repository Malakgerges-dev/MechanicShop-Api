
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Command.AssignLabor;


public sealed record AssignLaborCommand(Guid WorkOrderId,Guid LaborId)
    :IRequest<Result<Updated>>;