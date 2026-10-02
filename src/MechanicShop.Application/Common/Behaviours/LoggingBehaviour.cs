using MechanicShop.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest>(
    ILogger<TRequest> logger,
    IUser user,
    IIdentityService identityService
) : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger;
    private readonly IUser _user = user;
    private readonly IIdentityService _identityService = identityService;

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var RequestName = typeof(TRequest).Name;

        var UserId = _user.Id?? string.Empty;

        string ? UserName = string.Empty;

        if (!string.IsNullOrWhiteSpace(UserId))
        {
            UserName = await _identityService.GetUserNameAsync(UserId);
        }

        _logger.LogInformation(
        
        "Request: {Name} {@UserId} {@UserName} {Request}", RequestName,UserId,UserName,request);
    }
}