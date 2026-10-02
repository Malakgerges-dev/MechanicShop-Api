using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dto;
using MechanicShop.Application.Features.Billing.Mapper;
using MechanicShop.Domain.Common.Constant;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Command.IssueInvoice;

public class IssueInvoiceCommandHandler(
    ILogger<IssueInvoiceCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    TimeProvider datetime
    )
    : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
    private readonly ILogger<IssueInvoiceCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly TimeProvider _datetime = datetime;

    public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var workorder = await _context.WorkOrders
            .Include(v => v.Vehicle)
                .ThenInclude(c => c.Customer)
            .Include(r => r.RepairTasks)
                .ThenInclude(p => p.Parts)
            .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

        if(workorder is null)
        {
            _logger.LogWarning("Invoice issuance failed. WorkOrder {WorkOrderId} not found.", request.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        if(workorder.State != WorkOrderState.Completed)
        {
            _logger.LogWarning("Invoice issuance rejected. WorkOrder {WorkOrderId} is not in completed.", request.WorkOrderId);
            return ApplicationErrors.WorkOrderMustBeCompletedForInvoicing;
        }

        Guid invoiceId = Guid.NewGuid();

        var lineItems = new List<InvoiceLineItem>();

        var lineNumber = 1;

        foreach (var (task, taskIndex) in workorder.RepairTasks.Select((t, i) => (t, i + 1)))
        {
            var partSummary = task.Parts.Any()
            ? string.Join(Environment.NewLine, task.Parts.Select(p => $"    • {p.Name} x{p.Quantity} @ {p.Cost:C}"))
            : "     No Parts";

            var lineDescription =
            $"{taskIndex}: {task.Name} {Environment.NewLine}" +
            $"Labor = {task.LaborCost} {Environment.NewLine}" +
            $"Parts:{Environment.NewLine}{partSummary}";

            var totalPartCost = task.Parts.Sum(p => p.Cost * p.Quantity);
            var totalTaskCost = task.LaborCost + totalPartCost;

            var lineItemResult = InvoiceLineItem.Create(
                invoiceId: invoiceId,
                lineNumber: lineNumber++,
                description: lineDescription,
                quantity: 1,
                unitPrice: totalTaskCost);

            if (lineItemResult.IsError)
            {
                return lineItemResult.Errors;
            }

            lineItems.Add(lineItemResult.Value);
        }

        var subTotal = lineItems.Sum(x => x.LineTotal);

        var taxAmount = subTotal * MechanicShopConstant.TaxRate;

        var discountAmount = workorder.Discount ?? 0m;

        var createInvoiceResult = Invoice.Create(
            id: invoiceId,
            workOrderId: workorder.Id,
            items: lineItems,
            discountAmount: discountAmount,
            taxAmount: taxAmount,
            datetime: _datetime);

        if (createInvoiceResult.IsError)
        {
            _logger.LogWarning(
                "Invoice creation failed for WorkOrderId: {WorkOrderId}. Errors: {@Errors}",
                request.WorkOrderId,
                createInvoiceResult.Errors);

            return createInvoiceResult.Errors;
        }

        var invoice = createInvoiceResult.Value;

        await _context.Invoices.AddAsync(invoice, cancellationToken);

        await _context.SaveChangeAsync(cancellationToken);

        await _cache.RemoveByTagAsync("invoice", cancellationToken);

        _logger.LogInformation("Invoice {InvoiceId} issued for WorkOrder {WorkOrderId}.", invoice.Id, workorder.Id);

        return invoice.ToDto();
    }
}