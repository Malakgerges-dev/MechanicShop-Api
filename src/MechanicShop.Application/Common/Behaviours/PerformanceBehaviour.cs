using System.Diagnostics;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse>(
ILogger<TRequest> logger,
IUser user,
IIdentityService identityService
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger;
    private readonly IUser _user = user;
    private readonly IIdentityService _identityService = identityService;

    private readonly Stopwatch _Timer = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _Timer.Start();
        var Response =  await next();
        _Timer.Stop();

        var elapsedMilliseconds = _Timer.ElapsedMilliseconds;

        if(elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var UserId = _user.Id ?? string.Empty;
            var UserName= string.Empty;

            if (!string.IsNullOrWhiteSpace(UserId))
            {
                UserName = await _identityService.GetUserNameAsync(UserId);
            }

            _logger.LogWarning
            ("Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {UserId} {UserName} {@Request}",
              requestName,elapsedMilliseconds,UserId,UserName,request );
       }
        return Response;
    }
}