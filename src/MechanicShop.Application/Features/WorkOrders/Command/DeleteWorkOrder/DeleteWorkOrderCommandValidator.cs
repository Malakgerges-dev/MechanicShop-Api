
using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Command.DeleteWorkOrder;

public sealed class DeleteWorkOrderCommandValidators : AbstractValidator<DeleteWorkOrderCommand>
{
    public DeleteWorkOrderCommandValidators()
    {
        RuleFor(W=>W.WorkOrderId)
            .NotEmpty()
            .WithErrorCode("WorkOrder_Required")
            .WithMessage("Work Order Id Is Required");
    }
}