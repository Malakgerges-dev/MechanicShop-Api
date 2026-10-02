using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTask;

public sealed record GetRepairTaskQuery() : ICachedQuery<Result<List<RepairTaskDto>>>
{
    public string CacheKey => $"repair-Tasks";
    public string[] Tags => ["repair-tasks"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
