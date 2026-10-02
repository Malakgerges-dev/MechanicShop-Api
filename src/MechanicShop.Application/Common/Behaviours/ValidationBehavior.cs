using FluentValidation;
using MechanicShop.Domain.Common.Results.Abstractions;
using MediatR;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Behaviours;

public class ValidationBehavior<TRequest, TResponse>(
    IValidator<TRequest>? validator = null
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IValidator<TRequest>? _validator = validator;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if(_validator is null)
        {
           return await next(cancellationToken);
        }

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next();
        }

        var errors = validationResult.Errors.ToList()
            .ConvertAll(error => Error.Validation(
                     code: error.PropertyName,
                     description: error.ErrorMessage));

        return (dynamic)errors;
    }
}