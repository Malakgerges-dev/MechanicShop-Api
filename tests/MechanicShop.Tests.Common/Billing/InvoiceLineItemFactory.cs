using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Tests.Common.Billing;

public static class InvoiceLineItemFactory
{
    public static Result<InvoiceLineItem> CreateInvoiceLineItem(
        Guid? id = null,
        int? lineNumber = null,
        string? Description = null,
        int? Quantity = null,
        decimal? UnitPrice = null)
    {
        return InvoiceLineItem.Create(
            id ?? Guid.NewGuid(),
            lineNumber ?? 1,
            Description ?? "Some Invoice line",
            Quantity ?? 1,
            UnitPrice ?? 100m);
    }
}