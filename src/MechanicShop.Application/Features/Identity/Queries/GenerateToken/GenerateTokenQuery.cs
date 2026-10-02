using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateToken;

public record GenerateTokenQuery(
string Email,
string Password
):IRequest<Result<TokenResponse>>;