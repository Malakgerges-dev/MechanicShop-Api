using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MechanicShop.Infrastructure.Identity;

public class IdentityServices
(
    UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService

) : IIdentityService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IUserClaimsPrincipalFactory<AppUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<AppUserDto>> AuthenticateAsync(string Email, string Password)
    {
        var user = await _userManager.FindByEmailAsync(Email);

        if(user is null)
        {
            return Error.NotFound(
                "User_Not_Found",
                $"User With Email{UtilityService.MaskEmail(Email)} Not Found");
        }

        if (!user.EmailConfirmed)
        {
            return Error.NotFound(
                "Email_Not_Confirmed",
                $"Email {UtilityService.MaskEmail(Email)} Not Confirmed");
        }

        if (!await _userManager.CheckPasswordAsync(user, Password))
        {
            return Error.Conflict("Invalid_Login_Attempt", "Email/Password are incorrect");
        }

        return new AppUserDto(user.Id, user.Email!,
            await _userManager.GetRolesAsync(user), await _userManager.GetClaimsAsync(user));
    }

    public async Task<bool> AuthorizeAsync(string userId, string? policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if(user is null)
        {
            return false;
        }

        var principle = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principle, policyName!);

        return result.Succeeded;
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string UserId)
    {
        var user = await _userManager.FindByIdAsync(UserId)
                        ?? throw new InvalidOperationException(nameof(UserId));

        var roles = await _userManager.GetRolesAsync(user);

        var claims = await _userManager.GetClaimsAsync(user);

        return new AppUserDto(user.Id, user.Email!, roles, claims);
    }

    public async Task<string?> GetUserNameAsync(string UserId)
    {
        var user = await _userManager.FindByIdAsync(UserId)
                        ?? throw new InvalidOperationException(nameof(UserId));

        return user?.UserName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string Role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, Role);
    }
}