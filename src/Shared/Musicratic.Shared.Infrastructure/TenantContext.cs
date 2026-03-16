namespace Musicratic.Shared.Infrastructure;

public sealed class TenantContext
{
    public Guid TenantId { get; private set; }

    public bool IsSupraTenant { get; private set; }

    public static readonly Guid SupraTenantId = Guid.Empty;

    public void SetTenant(Guid tenantId)
    {
        TenantId = tenantId;
        IsSupraTenant = tenantId == SupraTenantId;
    }

    public void SetSupraTenant()
    {
        TenantId = SupraTenantId;
        IsSupraTenant = true;
    }
}
