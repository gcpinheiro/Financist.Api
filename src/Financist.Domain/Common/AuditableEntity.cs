namespace Financist.Domain.Common;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity()
    {
        CreatedAtUtc = DateTime.UtcNow;
    }

    protected AuditableEntity(Guid id)
        : base(id)
    {
        CreatedAtUtc = DateTime.UtcNow;
    }

    public DateTime CreatedAtUtc { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    protected void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
