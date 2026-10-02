namespace MechanicShop.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendEmail(string To,CancellationToken cancellationToken);

    Task SendSms(string PhoneNumber,CancellationToken cancellationToken);
}