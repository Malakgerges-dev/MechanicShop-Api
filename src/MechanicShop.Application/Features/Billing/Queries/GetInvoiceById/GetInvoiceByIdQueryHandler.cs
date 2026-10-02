using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dto;
using MechanicShop.Application.Features.Billing.Mapper;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler(
    ILogger<GetInvoiceByIdQueryHandler> logger,
    IAppDbContext context
    )
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly ILogger<GetInvoiceByIdQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery query, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(l => l.LineItems)
            .Include(w => w.workOrder)
                .ThenInclude(v => v.Vehicle)
                    .ThenInclude(c => c.Customer)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, cancellationToken);

        if(invoice is null)
        {
            logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", query.InvoiceId);

            return Error.NotFound("Invoice not found.");
        }

        return invoice.ToDto();
    }
}