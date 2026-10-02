using System.ComponentModel;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.EventHandler;

public class WorkOrderCollectionModifiedEventHandler(IWorkOrderNotifier workOrderNotifier)
: INotificationHandler<WorkOrderCollectionModified>
{
    private readonly IWorkOrderNotifier _workOrderNotifier = workOrderNotifier;

    public async Task Handle(WorkOrderCollectionModified notification, CancellationToken cancellationToken)
    {
        await _workOrderNotifier.NotifyWorkOrderChangeAsync(cancellationToken);
    }
}