using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public sealed class Invoice : AuditableEntity
{
    public Guid WorkOrderId { get; }
    public WorkOrder workOrder { get; set; } = default!;
    public DateTimeOffset IssuedAtUtc { get; }
    public DateTimeOffset? PaidAt { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public InvoiceStatus InvoiceStatus { get; private set; }
    public decimal SubTotal => LineItems.Sum(S => S.LineTotal);
    public decimal Total => SubTotal - DiscountAmount + TaxAmount;

    private readonly List<InvoiceLineItem> _lineItems = [];
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    private Invoice() { }

    private Invoice(
        Guid id,
        Guid workOrderId,
        DateTimeOffset issuedAt,
        List<InvoiceLineItem> lineItems,
        decimal discountAmount,
        decimal taxAmount)
        : base(id)
    {
        WorkOrderId = workOrderId;
        IssuedAtUtc = issuedAt;
        DiscountAmount = discountAmount;
        InvoiceStatus = InvoiceStatus.UnPaid;
        TaxAmount = taxAmount;
        _lineItems = lineItems;
    }

    public static Result<Invoice> Create(
        Guid id,
        Guid workOrderId,
        List<InvoiceLineItem> items,
        decimal discountAmount,
        decimal taxAmount,
        TimeProvider datetime)
    {
        if (workOrderId == Guid.Empty)
        {
            return InvoiceErrors.WorkOrderIdInvalid;
        }

        if (items is null || items.Count == 0)
        {
            return InvoiceErrors.LineItemsEmpty;
        }

        return new Invoice(id, workOrderId, datetime.GetUtcNow(), items, discountAmount, taxAmount);
    }

    public Result<Updated> ApplyDiscount(decimal discountAmount)
    {
        if (InvoiceStatus != InvoiceStatus.UnPaid)
        {
            return InvoiceErrors.InvoiceLocked;
        }

        if (discountAmount < 0)
        {
            return InvoiceErrors.DiscountNegative;
        }

        if (discountAmount > SubTotal)
        {
            return InvoiceErrors.DiscountExceedsSubtotal;
        }

        DiscountAmount = discountAmount;

        return Result.updated;
    }

    public Result<Updated> MarkAsPaid(TimeProvider timeProvider)
    {
        if (InvoiceStatus != InvoiceStatus.UnPaid)
        {
            return InvoiceErrors.InvoiceLocked;
        }

        InvoiceStatus = InvoiceStatus.Paid;
        PaidAt = timeProvider.GetUtcNow();

        return Result.updated;
    }
}