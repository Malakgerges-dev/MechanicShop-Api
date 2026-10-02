
using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Command.AssignLabor;


public class AssignLaborCommandValidator : AbstractValidator<AssignLaborCommand>
{
    public AssignLaborCommandValidator()
    {
        RuleFor(W=>W.WorkOrderId)
            .NotEmpty()
            .WithErrorCode("WorkOrderId_Required")
            .WithMessage("WorkOrder Id is Required");

        RuleFor(W=>W.LaborId)
            .NotEmpty()
            .WithErrorCode("LaborId_Required")
            .WithMessage("LaborId is Required");;
    }
}