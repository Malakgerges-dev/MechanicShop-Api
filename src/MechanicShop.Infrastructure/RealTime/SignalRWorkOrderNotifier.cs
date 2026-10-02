using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MechanicShop.Infrastructure.RealTime;

public class SignalRWorkOrderNotifier(IHubContext<WorkOrderHub> hubContext) : IWorkOrderNotifier
{
    private readonly IHubContext<WorkOrderHub> _hubContext = hubContext;

    public Task NotifyWorkOrderChangeAsync(CancellationToken cancellationToken = default)
        => _hubContext.Clients.All.SendAsync("WorkOrderChanged",cancellationToken: cancellationToken);
  
}