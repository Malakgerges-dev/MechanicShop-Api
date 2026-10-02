using System.Security.Claims;

namespace MechanicShop.Application.Features.Identity.Dtos;

public sealed record AppUserDto(
    string UserId,
    string Email,
    IList<string> Role,
    IList<Claim> Claims
);