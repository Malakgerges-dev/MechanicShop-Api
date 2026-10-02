
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator: AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x=>x.Name)
            .NotEmpty().WithMessage("Name is Required")
            .MaximumLength(100);
        
        RuleFor(x=>x.Email)
            .EmailAddress().WithMessage("Invalid Email")
            .MaximumLength(100);

        RuleFor(x=>x.PhoneNumber)
            .NotEmpty().WithMessage("Phone Number Is Required")
            .Matches(@"^\+?\d{7,15}$").WithMessage("Phone number must be 7–15 digits and may start with '+'.");

        RuleFor(x=>x.Vehicles)
            .NotEmpty().WithMessage("List Vehicle Cannot Be Null")
            .Must(v=>v.Count>0).WithMessage("At Least One Vehicle Is Required");

        RuleForEach(x=>x.Vehicles).SetValidator(new CreateVehilcleCommandValidator());
    }
            
}