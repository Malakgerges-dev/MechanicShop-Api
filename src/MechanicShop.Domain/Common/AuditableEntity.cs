
namespace MechanicShop.Domain.Common;


public abstract class AuditableEntity : Entity
{
    protected AuditableEntity()
    {}
    protected AuditableEntity(Guid Id) : base(Id)
    {}

    public DateTimeOffset CreatedAtUtc {get;set;}
    public string? CreatedBy{get; set;}
    public DateTimeOffset LastModifiedUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}