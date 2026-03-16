using Musicratic.Hub.Domain.Events;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Entities;

public sealed class HubAttachment : BaseEntity, ITenantScoped
{
    public Guid UserId { get; private set; }

    public Guid HubId { get; private set; }

    public Guid TenantId { get; private set; }

    public DateTime AttachedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public bool IsActive => DateTime.UtcNow < ExpiresAt;

    private HubAttachment() { }

    public static HubAttachment Create(Guid hubId, Guid tenantId, Guid userId, TimeSpan duration)
    {
        var attachment = new HubAttachment
        {
            HubId = hubId,
            TenantId = tenantId,
            UserId = userId,
            AttachedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(duration)
        };

        attachment.AddDomainEvent(new UserAttachedEvent(hubId, userId, attachment.Id));

        return attachment;
    }

    public void Expire()
    {
        ExpiresAt = DateTime.UtcNow;
    }
}
