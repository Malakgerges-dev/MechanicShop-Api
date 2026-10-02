using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Scheduling.Queries;


public class GetDailyScheduleQueryHandler(
    IAppDbContext context,
    TimeProvider Datetime
) : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _Datetime = Datetime;

    public async Task<Result<ScheduleDto>> Handle(GetDailyScheduleQuery query, CancellationToken cancellationToken)
    {
        var localStart = query.ScheduleDate.ToDateTime(TimeOnly.MinValue);
        var localEnd = localStart.AddDays(1);

        var StartUtc = TimeZoneInfo.ConvertTimeToUtc(localStart,query.TimeZone);
        var EndUtc = TimeZoneInfo.ConvertTimeToUtc(localEnd,query.TimeZone);

        var workOrder = await _context.WorkOrders
                .Where(
                    Wo=>Wo.StartAtUtc < EndUtc 
                    && Wo.EndAtUtc > StartUtc &&
                    (query.LaborId != null || Wo.LaborId == query.LaborId))  
                    
                .Include(Wo=>Wo.RepairTasks)
                .Include(Wo=>Wo.Vehicle)
                .Include(Wo=>Wo.Labor)
                .ToListAsync(cancellationToken);


        var Now = TimeZoneInfo.ConvertTime(_Datetime.GetUtcNow(),query.TimeZone);

        var result = new ScheduleDto
        {
            OnDate = query.ScheduleDate,
            EndOfDay = localEnd< Now,
            spots=[]
        };

        foreach (var spots in Enum.GetValues<Spot>())
        {
            var Current = localStart;

            var slots = new List<AvailabilitySlotDto>();

            var woBySpot = 
                workOrder.Where(Wo=>Wo.Spot == spots)
                .OrderBy(Wo=>Wo.StartAtUtc);


            while (Current <localEnd)
            {
                var next= Current.AddMinutes(15);

                var startUtc = TimeZoneInfo.ConvertTimeToUtc(Current,query.TimeZone);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(Current,query.TimeZone);

                var wo = woBySpot.FirstOrDefault
                    (Wo=>Wo.StartAtUtc<endUtc && Wo.EndAtUtc >startUtc);

                if(wo != null)
                {
                    if(!slots.Any(Wo=>Wo.WorkOrderId == wo.Id))
                    {
                        slots.Add(new AvailabilitySlotDto
                        {
                            WorkOrderId = wo.Id,
                            Spot = spots,
                            StartAt = wo.StartAtUtc,
                            EndAt = wo.EndAtUtc,
                            Vehicle = FormatVehicleInfo(wo.Vehicle!),
                            Labor = wo.Labor!.ToDto(),
                            IsOccupied = true,
                            RepairTasks = [.. wo.RepairTasks.ToList().ConvertAll(rt => rt.ToDto())],
                            WorkOrderLocked = !wo.IsEditable,
                            State = wo.State,
                            IsAvailable = false 
                        });
                    }
                }
                else
                {
                    slots.Add(new AvailabilitySlotDto
                    {
                        Spot = spots,
                        StartAt = startUtc,
                        EndAt = endUtc,
                        WorkOrderLocked = false,
                        IsAvailable = Current >= Now
                    });
                }
                Current =next;
            }

            result.spots.Add(new SpotDto
            {
                spot = spots,
                Slots = slots
            });
                

        }
        return result;        
    }

    public static string? FormatVehicleInfo(Vehicle vehicle)=>
        vehicle != null ?$"{vehicle.Make} | {vehicle.LicensePlate}" : null;
    
}