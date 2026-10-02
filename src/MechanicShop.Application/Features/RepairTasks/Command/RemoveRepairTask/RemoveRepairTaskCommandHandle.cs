using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Command.RemoveRepairTask;

public class RemoveRepairTaskCommandHandler(
    ILogger<RemoveRepairTaskCommand> logger,
    IAppDbContext context,
    HybridCache cache
) : IRequestHandler<RemoveRepairTaskCommand, Result<Deleted>>
{
    private readonly ILogger<RemoveRepairTaskCommand> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Deleted>> Handle(RemoveRepairTaskCommand command, CancellationToken cancellationToken)
    {
        var RepairTask = await _context.RepairTasks
            .FirstOrDefaultAsync(R=>R.Id == command.RepairTaskId);

        if(RepairTask is null)
        {
            _logger.LogError("Repair Task has Id: {RepairTask} not found for deletion.",command.RepairTaskId);
            return ApplicationErrors.RepairTaskNotFound;
        }

        var isInUse = await _context.WorkOrders.AsNoTracking()
            .SelectMany(Wo=>Wo.RepairTasks)
            .AnyAsync(rt=>rt.Id == command.RepairTaskId);

        if (isInUse)
        {
            _logger.LogWarning("RepairTask {RepairTaskId} cannot be deleted — in use by work orders.", command.RepairTaskId);

            return RepairTaskErrors.InUse;
        }

         _context.RepairTasks.Remove(RepairTask);
        await _context.SaveChangeAsync(cancellationToken);

        await _cache.RemoveByTagAsync("repair-task", cancellationToken);

        _logger.LogInformation("RepairTask {RepairTaskId} deleted successfully.", command.RepairTaskId);

        return Result.deleted;
    }
}
