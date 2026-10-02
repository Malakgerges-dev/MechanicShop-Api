using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrder;

public class GetWorkOrderQueryHandler(IAppDbContext context)
    : IRequestHandler<GetWorkOrderQuery, Result<PaginatedList<WorkOrderListItemDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<PaginatedList<WorkOrderListItemDto>>> Handle(GetWorkOrderQuery query, CancellationToken cancellationToken)
    {
        var workOrdersQuery = _context.WorkOrders.AsNoTracking()
            .Include(wo => wo.Vehicle!)
                .ThenInclude(v => v.Customer)
            .Include(wo => wo.Labor)
            .Include(wo => wo.RepairTasks)
                .ThenInclude(rt => rt.Parts)
            .Include(a => a.Invoice)
            .AsQueryable();

        workOrdersQuery = ApplyFilters(workOrdersQuery, query);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            workOrdersQuery = ApplySearchTerm(workOrdersQuery, query.SearchTerm);
        }

        workOrdersQuery = ApplySorting(workOrdersQuery, query.SortColumn, query.SortDirection);

        var count = await workOrdersQuery.CountAsync(cancellationToken: cancellationToken);

        var items = await workOrdersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize )
            .Select(
                wo => new WorkOrderListItemDto
                {
                  WorkOrderId = wo.Id,
                  InvoiceId = wo.Invoice == null ? null : wo.Invoice.Id,
                  Spot = wo.Spot,
                  StartAtUtc = wo.StartAtUtc,
                  EndAtUtc = wo.EndAtUtc,
                  Vehicle = wo.Vehicle!.ToDto(),
                  Customer = wo.Vehicle!.Customer!.Name,
                  Labor = wo.Labor != null
                    ? wo.Labor.FirstName + " " + wo.Labor.LastName
                    : null,
                  State = wo.State,
                  RepairTasks = wo.RepairTasks.Select(rt => rt.Name).ToList()
                }).ToListAsync();

        return new PaginatedList<WorkOrderListItemDto>
        {
            Items = items,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = count,
            TotalPage = (int)Math.Ceiling(count / (double)query.PageSize)
        };
    }

    private static IQueryable<WorkOrder> ApplyFilters(IQueryable<WorkOrder> query, GetWorkOrderQuery SearchQuery)
    {
        if (SearchQuery.State.HasValue)
        {
            query = query.Where(q => q.State == SearchQuery.State.Value);
        }

        if (SearchQuery.VehicleId.HasValue)
        {
            query = query.Where(q => q.VehicleId == SearchQuery.VehicleId.Value);
        }

        if (SearchQuery.LaborId.HasValue)
        {
            query = query.Where(q => q.LaborId == SearchQuery.LaborId.Value);
        }

        if (SearchQuery.StartDateFrom.HasValue)
        {
            query = query.Where(q => q.StartAtUtc >= SearchQuery.StartDateFrom.Value);
        }

        if (SearchQuery.StartDateTo.HasValue)
        {
            query = query.Where(q => q.StartAtUtc <= SearchQuery.StartDateTo.Value);
        }

        if (SearchQuery.EndDateFrom.HasValue)
        {
            query = query.Where(q => q.EndAtUtc >= SearchQuery.EndDateFrom.Value);
        }

        if (SearchQuery.EndDateTo.HasValue)
        {
            query = query.Where(q => q.EndAtUtc <= SearchQuery.EndDateTo.Value);
        }

        if (SearchQuery.Spot.HasValue)
        {
            query = query.Where(q => q.Spot == SearchQuery.Spot.Value);
        }

        return query;
    }

    private static IQueryable<WorkOrder> ApplySearchTerm(IQueryable<WorkOrder> query, string SearchTerm)
    {
        var normalized = SearchTerm.Trim().ToLower();

        return query.Where(
            q => (q.Vehicle != null &&
            (
                q.Vehicle.Make.ToLower().Contains(normalized) ||
                q.Vehicle.Model.ToLower().Contains(normalized) ||
                q.Vehicle.LicensePlate.ToLower().Contains(normalized)))
                 ||
            (
                q.Labor != null && (
                q.Labor.FirstName.ToLower().Contains(normalized) ||
                q.Labor.LastName.ToLower().Contains(normalized) ||
                (q.Labor.FirstName + " " + q.Labor.LastName).ToLower().Contains(normalized)))
                ||
            (
                q.RepairTasks != null &&
                q.RepairTasks.Any(rt => rt.Name.ToLower().Contains(normalized) ||
                rt.Id.ToString().ToLower().Contains(normalized))));
    }

    private static IQueryable<WorkOrder> ApplySorting(IQueryable<WorkOrder> query, string SortColumn, string SortDirection)
    {
        var isDescending = SortDirection.Equals("des", StringComparison.CurrentCultureIgnoreCase);

        return SortColumn.ToLower() switch
        {
            "createdat"=>isDescending ? query.OrderByDescending(Wo => Wo.CreatedAtUtc) : query.OrderBy(Wo => Wo.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(wo => wo.LastModifiedUtc) : query.OrderBy(wo => wo.LastModifiedUtc),
            "startat" => isDescending ? query.OrderByDescending(wo => wo.StartAtUtc) : query.OrderBy(wo => wo.StartAtUtc),
            "endat" => isDescending ? query.OrderByDescending(wo => wo.EndAtUtc) : query.OrderBy(wo => wo.EndAtUtc),
            "state" => isDescending ? query.OrderByDescending(wo => wo.State) : query.OrderBy(wo => wo.State),
            "spot" => isDescending ? query.OrderByDescending(wo => wo.Spot) : query.OrderBy(wo => wo.Spot),
            "total" => isDescending ? query.OrderByDescending(wo => wo.Total) : query.OrderBy(wo => wo.Total),
            "vehicleid" => isDescending ? query.OrderByDescending(wo => wo.VehicleId) : query.OrderBy(wo => wo.VehicleId),
            "laborid" => isDescending ? query.OrderByDescending(wo => wo.LaborId) : query.OrderBy(wo => wo.LaborId),
            _ => query.OrderByDescending(wo => wo.CreatedAtUtc)
        };
    }
}

