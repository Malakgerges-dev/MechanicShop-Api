using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Command.CreateRepairTask;

public sealed class CreateRepairTaskCommandValidator : AbstractValidator<CreateRepairTaskCommand>
{
    public CreateRepairTaskCommandValidator()
    {
        RuleFor(r=>r.Name)
            .NotEmpty()
            .WithMessage("Name Is Required")
            .MaximumLength(100);

        RuleFor(r=>r.LaborCost)
            .GreaterThan(0)
            .WithMessage("Labor Cost Must be Grater Than 0");

        RuleFor(r=>r.EstimatedDurationInMinutes)
            .NotNull()
            .WithMessage("Estimated Duration In Minutes Is Required")
            .IsInEnum();

        RuleFor(r=>r.Parts)
            .NotNull()
            .WithMessage("Parts List Cannot null")
            .Must(p=>p.Count>0).WithMessage("At Least One Part Is Required");

        RuleForEach(r=>r.Parts).SetValidator(new CreateRepairTaskPartCommandValidator());

        
    }
}