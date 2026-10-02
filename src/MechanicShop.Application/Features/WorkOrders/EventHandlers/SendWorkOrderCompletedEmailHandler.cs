
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandler;

public class SendWorkOrderCompletedEmailHandler(
    ILogger<SendWorkOrderCompletedEmailHandler> logger,
    IAppDbContext context,
    INotificationService notificationService
) : INotificationHandler<WorkOrderCompleted>
{
    private readonly ILogger<SendWorkOrderCompletedEmailHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly INotificationService _notificationService = notificationService;

    public async Task Handle(WorkOrderCompleted notification, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .Include(W=>W.Vehicle!).ThenInclude(V=>V.Customer)
            .AsNoTracking().FirstOrDefaultAsync(W=>W.Id == notification.WorkOrderId,cancellationToken);
        if(workOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", notification.WorkOrderId);
            return;
        }

        await _notificationService.SendEmail(workOrder.Vehicle?.Customer?.Email!,cancellationToken);
        await _notificationService.SendSms(workOrder.Vehicle?.Customer?.PhoneNumber!,cancellationToken);
    }
}