using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateToken;


public class GenerateTokenQueryHandler(
    ILogger<GenerateTokenQueryHandler> logger,
    IIdentityService identityService
    ,ITokenProvider tokenProvider

) : IRequestHandler<GenerateTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<GenerateTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<Result<TokenResponse>> Handle(GenerateTokenQuery query, CancellationToken cancellationToken)
    {
        var UserResponse = await _identityService.AuthenticateAsync(query.Email,query.Password);

        if (UserResponse.IsError)
        {
            return UserResponse.Errors;
        }

        var GenerateTokenResult = await _tokenProvider.GenerateJwtTokenAsync(UserResponse.Value,cancellationToken);

          if (GenerateTokenResult.IsError)
        {
            return GenerateTokenResult.Errors;
        }

        return GenerateTokenResult.Value;
    }
}