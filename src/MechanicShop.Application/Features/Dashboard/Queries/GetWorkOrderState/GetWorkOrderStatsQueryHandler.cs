using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderState;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Dashboard.Queries;

public class GetWorkOrderStatsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetWorkOrderStatsQuery, Result<TodayWorkOrderStatsDto>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<TodayWorkOrderStatsDto>> Handle(GetWorkOrderStatsQuery request, CancellationToken cancellationToken)
    {
       var start = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
       var end = request.Date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

       var query = _context.WorkOrders
            .Include(Wo => Wo.Vehicle)
            .Include(Wo => Wo.RepairTasks)
            .Include(Wo => Wo.Invoice)
            .Where(Wo => Wo.StartAtUtc >= start && Wo.EndAtUtc <= end);

       var total = await query.CountAsync(cancellationToken: cancellationToken);

       if(total == 0)
        {
            return new TodayWorkOrderStatsDto
            {
                Date = request.Date,
                Total = 0,
                Scheduled = 0,
                InProgress = 0,
                Completed = 0,
                Cancelled = 0,
                TotalRevenue = 0,
                TotalPartsCost = 0,
                TotalLaborCost = 0,
                UniqueVehicles = 0,
                UniqueCustomers = 0
            };
        }

       var stats = await query.ToListAsync(cancellationToken: cancellationToken);
       var totalRevenue = stats.Sum(Wo => Wo.Invoice?.Total ?? 0);
       var totalPartCost = stats.Where(Wo => Wo.Invoice != null).Sum(I => I.TotalPartCost ?? 0);
       var totalLaborCost = stats.Where(Wo => Wo.Invoice != null).Sum(I => I.TotalLaborCost ?? 0);
       var uniqVehicles = stats.Select(Wo => Wo.VehicleId).Distinct().Count();
       var uniqCustomers = stats.Select(Wo => Wo.Vehicle!.CustomerId).Distinct().Count();
       var netProfit = totalRevenue - totalLaborCost - totalPartCost;

       return new TodayWorkOrderStatsDto
        {
            Date = request.Date,
            Total = total,
            Scheduled = stats.Count(Wo => Wo.State == WorkOrderState.Scheduled),
            InProgress = stats.Count(Wo => Wo.State == WorkOrderState.InProgress),
            Completed = stats.Count(Wo => Wo.State == WorkOrderState.Completed),
            Cancelled = stats.Count(Wo => Wo.State == WorkOrderState.Cancelled),
            TotalRevenue = totalRevenue,
            TotalPartsCost = totalPartCost,
            TotalLaborCost = totalLaborCost,
            UniqueVehicles = uniqVehicles,
            UniqueCustomers = uniqCustomers,
            NetProfit = netProfit,

            ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0,
            CompletionRate = total > 0 ? ((decimal)stats.Count(wo => wo.State == WorkOrderState.Completed) / total) * 100 : 0,
            AverageRevenuePerOrder = total > 0 ? total / totalRevenue : 0,
            OrdersPerVehicle = uniqVehicles > 0 ? (decimal)total / uniqVehicles : 0,
            PartsCostRatio = totalRevenue > 0 ? (totalPartCost / totalRevenue) * 100 : 0,
            LaborCostRatio = totalRevenue > 0 ? (totalLaborCost / totalRevenue) * 100 : 0,
            CancellationRate = total > 0 ? ((decimal)stats.Count(x => x.State == WorkOrderState.Cancelled) / total) * 100 : 0
        };
    }
}