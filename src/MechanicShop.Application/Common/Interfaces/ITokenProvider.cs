using System.Security.Claims;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity.Queries;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto userDto,CancellationToken cancellationToken);
    
    ClaimsPrincipal? GetPrincipalFromExpireToken(string Token);
}