using System.Security.Claims;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

public class RefreshTokenQueryHandler(
    ILogger<RefreshTokenQueryHandler> logger,
    IAppDbContext context,
    ITokenProvider tokenProvider,
    IIdentityService identityService
) : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IIdentityService _identityService = identityService;

    public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery query, CancellationToken cancellationToken)
    {
        var principle = _tokenProvider.GetPrincipalFromExpireToken(query.ExpiredAccessToken);

        if(principle is null)
        {
            _logger.LogError("Expired Access Token Not Valid");

            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }

        var userId = principle.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if(userId is null)
        {
            _logger.LogError("Invalid UserId Claim");

            return ApplicationErrors.UserIdClaimInvalid;
        }

        var getUserResult = await _identityService.GetUserByIdAsync(userId);

        if (getUserResult.IsError)
        {
            _logger.LogError("Get user by id error occurred: {ErrorDescription}", getUserResult.TopError.Description);
            return getUserResult.Errors;
        }

        var refreshToKen = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == query.RefreshToken && r.UserId == userId, cancellationToken);

        if(refreshToKen is null)
        {
            _logger.LogError("Refresh Token Has Expired");

            return ApplicationErrors.RefreshTokenExpired;
        }

        var generateTokenResult = await _tokenProvider.GenerateJwtTokenAsync(getUserResult.Value, cancellationToken);

        if (generateTokenResult.IsError)
        {
            _logger.LogError("Generate Token Error Occurred:{Error Description}", generateTokenResult.TopError.Description);

            return generateTokenResult.Errors;
        }

        return generateTokenResult.Value;
    }
}