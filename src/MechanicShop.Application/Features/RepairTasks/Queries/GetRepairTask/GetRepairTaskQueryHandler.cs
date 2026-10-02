using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTask;



public class GetRepairTaskQueryHandler(IAppDbContext context)
    : IRequestHandler<GetRepairTaskQuery, Result<List<RepairTaskDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<List<RepairTaskDto>>> Handle(GetRepairTaskQuery query, CancellationToken cancellationToken)
    {
        var repairTask = await _context.RepairTasks.Include(r=>r.Parts).AsNoTracking()
            .ToListAsync(cancellationToken);

        return repairTask.ToDtos();
    }

}
