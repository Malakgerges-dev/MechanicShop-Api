using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Command.UpdateOrderStates;

public sealed class UpdateWorkOrderStateCommandValidator: AbstractValidator<UpdateWorkOrderStateCommand>
{
    public UpdateWorkOrderStateCommandValidator()
    {
        RuleFor(W => W.State)
            .IsInEnum()
            .WithErrorCode("WorkOrderStatus_Invalid")
            .WithMessage("Status must be a valid WorkOrderStatus value.");
    }
}