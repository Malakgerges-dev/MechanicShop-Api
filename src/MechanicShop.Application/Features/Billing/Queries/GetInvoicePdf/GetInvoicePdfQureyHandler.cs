using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dto;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public class GetInvoicePdfQureyHandler(
    ILogger<GetInvoicePdfQureyHandler> logger,
    IAppDbContext context,
    IInvoicePdfGenerator pdfGenerator
    )
    :
    IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
    private readonly ILogger<GetInvoicePdfQureyHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly IInvoicePdfGenerator _pdfGenerator = pdfGenerator;

    public async Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery query, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.AsNoTracking()
        .Include(i => i.LineItems)
        .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, cancellationToken);

        if(invoice is null)
        {
            _logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", query.InvoiceId);
            return Error.NotFound("Invoice not found.");
        }

        try
        {
            var pdfBytes = _pdfGenerator.Generate(invoice);

            var invoicePdf = new InvoicePdfDto
            {
                Content = pdfBytes,
                FileName = $"invoice-{invoice.Id}.pdf"
            };

            return invoicePdf;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate PDF for InvoiceId: {InvoiceId}", query.InvoiceId);
            return Error.Failure("An error occurred while generating the invoice PDF.");
        }
    }
}