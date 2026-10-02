
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.Services;

public class NotificationService(ILogger<NotificationService> logger) : INotificationService
{

    private const string Message = "Your vehicle service is complete. You may collect it from the shop at your earliest convenience.";


    public Task SendEmail(string To, CancellationToken cancellationToken)
    {
        var at = To.IndexOf('@');
        var maskedEmail = at > 1
            ? To[0] +  new string('*',at-2) + To[at-1] + To[at..]
            :"*****";

        logger.LogInformation("[Email] To: {Email} | Message: {Message}", maskedEmail, Message);

        return Task.CompletedTask;
    }

    public Task SendSms(string PhoneNumber, CancellationToken cancellationToken)
    {
         var masked = PhoneNumber.Length >= 4
            ? new string('*', PhoneNumber.Length - 4) + PhoneNumber[^4..]
            : "****";

        logger.LogInformation("[SMS] To: {Phone} | Message: {Message}", masked, Message);

        return Task.CompletedTask;
    }
}