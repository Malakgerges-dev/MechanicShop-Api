using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Command.UpdateRepairTask;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Command.CreateRepairTask;

public class UpdateRepairTaskCommandHandler(
    ILogger<CreateRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache

) : IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
{
    private readonly ILogger<CreateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand command, CancellationToken cancellationToken)
    {
            var repairTask = await _context.RepairTasks
            .Include(rt => rt.Parts)
            .FirstOrDefaultAsync(rt => rt.Id == command.RepairTaskId, cancellationToken);

            if (repairTask is null)
        {
            _logger.LogWarning("RepairTask {RepairTaskId} not found for update.", command.RepairTaskId);

            return ApplicationErrors.RepairTaskNotFound;
        }

            var validatedParts = new List<Part>();

            foreach (var p in command.Parts)
        {
            var partId = p.PartId ?? Guid.NewGuid();

            var partResult = Part.Create(partId, p.Name!, p.Cost, p.Quantity);

            if (partResult.IsError)
            {
                return partResult.Errors;
            }

            validatedParts.Add(partResult.Value);
        }

            var updateRepairTaskResult = repairTask.Update(command.Name, command.LaborCost, command.EstimatedDurationInMinutes);

            if (updateRepairTaskResult.IsError)
        {
            return updateRepairTaskResult.Errors;
        }

            var upsertPartsResult = repairTask.UpSertParts(validatedParts);

            if (upsertPartsResult.IsError)
        {
            return upsertPartsResult.Errors;
        }

            await _context.SaveChangeAsync(cancellationToken);
            await _cache.RemoveByTagAsync("repair-task", cancellationToken );

            return Result.updated;
    }
}