using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Command.IssueInvoice;

public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(I => I.WorkOrderId)
            .NotEmpty()
            .WithErrorCode("WorkOrder_Is_Required")
            .WithMessage("WorkOrder Is Required");
    }
}