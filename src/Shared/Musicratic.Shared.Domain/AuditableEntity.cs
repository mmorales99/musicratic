namespace Musicratic.Shared.Domain;

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
