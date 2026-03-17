using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Domain.Events;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Entities;

public sealed class HubMember : AuditableEntity, ITenantScoped
{
    public Guid UserId { get; private set; }

    public Guid HubId { get; private set; }

    public Hub? Hub { get; private set; }

    public Guid TenantId { get; private set; }

    public HubMemberRole Role { get; private set; }

    public Guid? AssignedBy { get; private set; }

    public DateTime AssignedAt { get; private set; }

    private HubMember() { }

    internal static HubMember Create(
        Guid hubId,
        Guid tenantId,
        Guid userId,
        HubMemberRole role,
        Guid? assignedBy)
    {
        return new HubMember
        {
            HubId = hubId,
            TenantId = tenantId,
            UserId = userId,
            Role = role,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow
        };
    }

    internal void PromoteTo(HubMemberRole newRole, Guid promotedBy)
    {
        if (newRole <= Role)
            throw new InvalidOperationException($"Cannot demote or assign same role. Current: {Role}, Requested: {newRole}.");

        var oldRole = Role;
        Role = newRole;
        AssignedBy = promotedBy;
        AssignedAt = DateTime.UtcNow;

        AddDomainEvent(new MemberPromotedEvent(HubId, UserId, oldRole, newRole, promotedBy));
    }

    internal void DemoteTo(HubMemberRole newRole, Guid demotedBy)
    {
        if (newRole >= Role)
            throw new InvalidOperationException($"Cannot demote to same or higher role. Current: {Role}, Requested: {newRole}.");

        if (Role == HubMemberRole.SuperOwner)
            throw new InvalidOperationException("Cannot demote the super owner of a hub.");

        var oldRole = Role;
        Role = newRole;
        AssignedBy = demotedBy;
        AssignedAt = DateTime.UtcNow;

        AddDomainEvent(new MemberDemotedEvent(HubId, UserId, oldRole, newRole, demotedBy));
    }
}
