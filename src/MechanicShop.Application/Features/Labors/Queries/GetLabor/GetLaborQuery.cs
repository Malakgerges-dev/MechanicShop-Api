using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Labors.Queries.GetLabor;

public sealed record GetLaborQuery() : ICachedQuery<Result<List<LaborDto>>>
{
    public string CacheKey => $"labors";

    public string[] Tags => ["labors"];
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}