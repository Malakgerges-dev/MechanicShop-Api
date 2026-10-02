
using System.Security.Cryptography;
namespace MechanicShop.Domain.WorkOrders.Billing;


public enum InvoiceStatus
{
    UnPaid = 0,
    Paid = 1,
    Refunded = 2
}