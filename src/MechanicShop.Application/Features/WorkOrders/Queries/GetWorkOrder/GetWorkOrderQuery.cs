using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrder;

public sealed record GetWorkOrderQuery(

int Page,
int PageSize,
string? SearchTerm,
string SortColumn = "CreatedAt",
string SortDirection = "desc",
WorkOrderState? State = null,
Guid? VehicleId = null,
Guid? LaborId = null,
DateTime? StartDateFrom = null,
DateTime? StartDateTo = null,
DateTime? EndDateFrom = null,
DateTime? EndDateTo = null,
Spot? Spot = null
) : ICachedQuery<Result<PaginatedList<WorkOrderListItemDto>>>
{
    public string CacheKey => $"work-orders:p={Page}:ps={PageSize}"+
    $":q={SearchTerm??"-"}"+
    $":Sort={SortColumn} {SortDirection}"+
    $":State={State?.ToString()??"-"}"+
    $"Veh={VehicleId.ToString()??"-"}"+
    $"Lab={LaborId.ToString()??"-"}"+
    $"SdFrom={StartDateFrom?.ToString("yyyymmdd")??"-"}"+
    $":SdTo={StartDateTo?.ToString("yyyyMMdd") ?? "-"}" +
    $":EdFrom={EndDateFrom?.ToString("yyyyMMdd") ?? "-"}" +
    $":EdTo={EndDateTo?.ToString("yyyyMMdd") ?? "-"}" +
    $":spot={Spot?.ToString() ?? "-"}";

    public string[] Tags =>["work-order"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}