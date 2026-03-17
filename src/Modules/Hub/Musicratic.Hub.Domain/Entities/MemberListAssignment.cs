using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Entities;

public sealed class MemberListAssignment : AuditableEntity, ITenantScoped
{
    public Guid MemberId { get; private set; }

    public HubMember? Member { get; private set; }

    public Guid ListId { get; private set; }

    public List? List { get; private set; }

    public Guid TenantId { get; private set; }

    public Guid AssignedBy { get; private set; }

    public DateTime AssignedAt { get; private set; }

    private MemberListAssignment() { }

    public static MemberListAssignment Create(
        Guid memberId,
        Guid listId,
        Guid tenantId,
        Guid assignedBy)
    {
        return new MemberListAssignment
        {
            MemberId = memberId,
            ListId = listId,
            TenantId = tenantId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow
        };
    }
}
