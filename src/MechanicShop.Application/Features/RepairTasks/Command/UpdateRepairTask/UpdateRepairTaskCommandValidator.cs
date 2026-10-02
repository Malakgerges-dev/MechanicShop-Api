using FluentValidation;
using MechanicShop.Application.Features.RepairTasks.Command.UpdateRepairTask;

namespace MechanicShop.Application.Features.RepairTasks.Command.CreateRepairTask;

public sealed class UpdateRepairTaskCommandValidator : AbstractValidator<UpdateRepairTaskCommand>
{
    public UpdateRepairTaskCommandValidator()
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

        RuleForEach(r=>r.Parts).SetValidator(new UpdateRepairTaskPartCommandValidator());

        
    }
}