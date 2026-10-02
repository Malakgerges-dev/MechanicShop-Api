using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces;

public interface IIdentityService
{

    Task<bool> IsInRoleAsync(string userId, string Role);

    Task<bool> AuthorizeAsync(string userId, string? policyName);

    Task<Result<AppUserDto>> AuthenticateAsync(string Email,String Password);

    Task<string?> GetUserNameAsync(string UserId);

    Task<Result<AppUserDto>> GetUserByIdAsync(string UserId);


}