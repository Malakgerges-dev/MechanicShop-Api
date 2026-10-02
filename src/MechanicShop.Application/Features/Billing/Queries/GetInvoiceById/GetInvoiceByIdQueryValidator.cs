using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;


public class GetInvoiceByIdQueryValidator: AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(I => I.InvoiceId)
            .NotEmpty()
            .WithErrorCode("InvoiceId_Is_Required")
            .WithMessage("InvoiceId Is Required");
    }
}