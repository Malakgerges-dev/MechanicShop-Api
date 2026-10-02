using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;


public class UpdateVehilcleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehilcleCommandValidator()
    {
        RuleFor(v=>v.Make)
            .NotEmpty().MaximumLength(50);
        
        RuleFor(v=>v.Model)
            .NotEmpty().MaximumLength(50);
        
        RuleFor(v=>v.LicensePlate)
            .NotEmpty().MaximumLength(50);
    }
}