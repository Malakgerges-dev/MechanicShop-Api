namespace MechanicShop.Application.Common.Interfaces;

public interface IWorkOrderNotifier
{
    Task NotifyWorkOrderChangeAsync(CancellationToken cancellationToken = default);
}