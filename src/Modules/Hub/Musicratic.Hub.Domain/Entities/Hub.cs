using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Domain.Events;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Entities;

public sealed class Hub : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public HubType Type { get; private set; }

    public Guid OwnerId { get; private set; }

    public SubscriptionTier SubscriptionTier { get; private set; }

    public DateTime? SubscriptionExpiresAt { get; private set; }

    public bool IsActive { get; private set; }

    public string QrUrl { get; private set; } = string.Empty;

    public string DirectLink { get; private set; } = string.Empty;

    public HubVisibility Visibility { get; private set; }

    public HubSettings Settings { get; private set; } = null!;

    private readonly List<HubMember> _members = [];

    public IReadOnlyCollection<HubMember> Members => _members.AsReadOnly();

    private Hub() { }

    public static Hub Create(string name, HubType type, Guid ownerId, HubSettings settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var hub = new Hub
        {
            TenantId = Guid.NewGuid(),
            Name = name,
            Code = GenerateCode(name),
            Type = type,
            OwnerId = ownerId,
            SubscriptionTier = SubscriptionTier.FreeTrial,
            IsActive = false,
            Visibility = HubVisibility.Public,
            Settings = settings
        };

        hub.TenantId = hub.Id;
        hub.QrUrl = $"/qr/{hub.Code}";
        hub.DirectLink = $"/h/{hub.Code}";

        hub.AddDomainEvent(new HubCreatedEvent(hub.Id, ownerId, name));

        return hub;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        AddDomainEvent(new HubActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        AddDomainEvent(new HubDeactivatedEvent(Id));
    }

    public void UpdateSettings(HubSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void SetVisibility(HubVisibility visibility)
    {
        Visibility = visibility;
    }

    public HubMember AddMember(Guid userId, HubMemberRole role, Guid? assignedBy)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException($"User '{userId}' is already a member of this hub.");

        var member = HubMember.Create(Id, TenantId, userId, role, assignedBy);
        _members.Add(member);

        AddDomainEvent(new MemberJoinedEvent(Id, userId, role));

        return member;
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new InvalidOperationException($"User '{userId}' is not a member of this hub.");

        if (member.Role == HubMemberRole.SuperOwner)
            throw new InvalidOperationException("Cannot remove the super owner of a hub.");

        _members.Remove(member);
    }

    public void PromoteMember(Guid userId, HubMemberRole newRole, Guid promotedBy)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new InvalidOperationException($"User '{userId}' is not a member of this hub.");

        member.PromoteTo(newRole, promotedBy);
    }

    private static string GenerateCode(string name)
    {
        var sanitized = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        var prefix = sanitized.Length >= 8 ? sanitized[..8] : sanitized;
        var suffix = DateTime.UtcNow.Ticks % 10000;
        return $"{prefix}{suffix}";
    }
}
