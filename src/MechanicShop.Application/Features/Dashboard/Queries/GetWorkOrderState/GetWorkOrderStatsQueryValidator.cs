
using FluentValidation;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderState;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats
{
    internal class GetWorkOrderStatsQueryValidator : AbstractValidator<GetWorkOrderStatsQuery>
    {
        public GetWorkOrderStatsQueryValidator()
        {
            RuleFor(request => request.Date)
                .NotEmpty()
                .WithErrorCode("Date_Is_Required")
                .WithMessage("Date is required.");
        }
    }
}