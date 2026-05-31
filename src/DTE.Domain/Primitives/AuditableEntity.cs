namespace DTE.Domain.Primitives;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    protected AuditableEntity(Guid id)
        : base(id)
    {
    }
}
