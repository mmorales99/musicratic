namespace Musicratic.Shared.Domain;

public interface ITenantScoped
{
    Guid TenantId { get; }
}
