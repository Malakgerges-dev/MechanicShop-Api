using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateToken;


public sealed class GenerateTokenQueryValidator : AbstractValidator<GenerateTokenQuery>
{
    public GenerateTokenQueryValidator()
    {
        RuleFor(TN=>TN.Email)
            .NotNull().NotEmpty()
            .WithErrorCode("Email_Null_Or_Empty")
            .WithMessage("Email Cannot Be Null Or Empty");

        RuleFor(TN=>TN.Password)
            .NotNull().NotEmpty()
            .WithErrorCode("Password_Null_Or_Empty")
            .WithMessage("Password Cannot Be Null Or Empty");

    }
}