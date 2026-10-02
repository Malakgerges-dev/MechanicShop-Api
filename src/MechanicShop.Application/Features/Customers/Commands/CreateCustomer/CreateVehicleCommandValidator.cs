using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;


public class CreateVehilcleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehilcleCommandValidator()
    {
        RuleFor(v=>v.Make)
            .NotEmpty().MaximumLength(50);
        
        RuleFor(v=>v.Model)
            .NotEmpty().MaximumLength(50);
        
        RuleFor(v=>v.LicensePlate)
            .NotEmpty().MaximumLength(50);
    }
}