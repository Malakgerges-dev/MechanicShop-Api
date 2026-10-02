using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.Services;

public class WorkOrderPolicy(IOptions<AppSettings> options,IAppDbContext appDbContext) : IWorkOrderPolicy
{
    private readonly AppSettings _AppSetting = options.Value;
    private readonly IAppDbContext _appDbContext = appDbContext;

    public async Task<Result<Success>> CheckSpotAvailabilityAsync(Spot spot, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludeWorkOrderId = null, CancellationToken ct = default)
    {
        var IsOccupied =  await _appDbContext.WorkOrders.
            AnyAsync
            (
                a=>
                a.Spot == spot &&
                a.StartAtUtc < endAt &&
                a.EndAtUtc > startAt&&
                (!excludeWorkOrderId.HasValue || a.Id != excludeWorkOrderId.Value),
                ct
            );

        return IsOccupied?
            Error.Conflict("MechanicShop_Spot_Full", "The selected time slot is unavailable for the requested services.")
            : Result.success;
    }

    public async Task<bool> IsLaborOccupied(Guid laborId, Guid excludedWorkOrderId, DateTimeOffset startAt, DateTimeOffset endAt)
    {
        return await _appDbContext.WorkOrders
            .AnyAsync
            (
                a=>
                a.LaborId == laborId&&
                a.Id != excludedWorkOrderId&&
                a.StartAtUtc <endAt&&
                a.EndAtUtc >startAt
            );
    }

    public bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration)
    {
        
        var Opening = startAt.Date.Add(_AppSetting.OpeningTime.ToTimeSpan());
        var Closing = startAt.Date.Add(_AppSetting.ClosingTime.ToTimeSpan());
        var endAt = startAt + duration;

        return startAt < Opening || endAt > Closing;
    }

    public async Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null)
    {
        return await _appDbContext.WorkOrders.
            AnyAsync
            (
                a=>
                (excludedWorkOrderId == null ||a.Id != excludedWorkOrderId)&&
                a.VehicleId == vehicleId&&
                a.StartAtUtc <endAt&&
                a.EndAtUtc > startAt
            );
    }

    public Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if((endAt - startAt) < TimeSpan.FromMinutes(_AppSetting.MinimumAppointmentDurationInMinutes))
        {
            return Error.Conflict(
                 "WorkOrder_TooShort",
                $"WorkOrder duration must be at least {_AppSetting.MinimumAppointmentDurationInMinutes} minutes.");
        }

        return Result.success;
            
        
    }
}