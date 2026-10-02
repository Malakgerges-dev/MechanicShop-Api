
using System.Data;
using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.UpdateWorkOrderRepairTasksCommand;

public sealed class UpdateWorkOrderRepairTasksCommandValidator:AbstractValidator<UpdateWorkOrderRepairTasksCommand>
{
    public UpdateWorkOrderRepairTasksCommandValidator()
    {
        RuleFor(x=>x.WorkOrderId)
            .NotEmpty()
            .WithErrorCode("WorkOrderId_Required")
            .WithMessage("WorkOrderId is required.");

        RuleFor(x=>x.RepairTasksId)
            .NotEmpty()
            .WithErrorCode("RepairTasksId_Required")
            .WithMessage("At least one repair task must be provided.");
    }
}