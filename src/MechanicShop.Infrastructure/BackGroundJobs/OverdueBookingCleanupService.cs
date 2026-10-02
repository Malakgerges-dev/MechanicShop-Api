using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.BackGroundJobs;

public class OverdueBookingCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<OverdueBookingCleanupService> logger,
    IOptions<AppSettings> options,
    TimeProvider dateTime
) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<OverdueBookingCleanupService> _logger = logger;
    private readonly TimeProvider _dateTime = dateTime;
    private readonly AppSettings _appSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Checking Overdue Work Orders at {Now}",_dateTime.GetUtcNow());
        
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var cutoff = _dateTime.GetUtcNow().AddMinutes(-_appSettings.BookingCancellationThresholdMinutes);

            var overdue = await db.WorkOrders
                .Where(wo=>wo.State == WorkOrderState.Scheduled && wo.StartAtUtc <= cutoff)
                .ToListAsync(stoppingToken);

            if(overdue.Count > 0)
            {
                foreach (var wo in overdue)
                {
                    var result = wo.Cancel();

                    if (result.IsError)
                    {
                        _logger.LogError("Failed To Cancel Work Order {Id}: {Error}",wo.Id,result.Errors );
                    }

                    await db.SaveChangeAsync(stoppingToken);
                     _logger.LogInformation("Cancelled {Count} overdue work orders: {Ids}", overdue.Count, overdue.Select(w => w.Id));

                }
            }
            else
            {
                _logger.LogInformation("No Overdue Work Orders Found");
            }
                    
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex,"Error Cleaning up overdue Work orders ");
        }
    }
}
