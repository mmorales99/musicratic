using FluentAssertions;
using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Domain.Events;
using HubEntity = Musicratic.Hub.Domain.Entities.Hub;
using Musicratic.Hub.Domain.Entities;

namespace Musicratic.Hub.Tests.Domain;

public class HubTests
{
    private static HubSettings DefaultSettings => new()
    {
        AllowProposals = true,
        AutoSkipThreshold = 0.65,
        VotingWindowSeconds = 60,
        MaxQueueSize = 50
    };

    [Fact]
    public void Create_ShouldReturnHub_WhenValidParameters()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        var hub = HubEntity.Create("My Bar", HubType.Venue, ownerId, DefaultSettings, "MYBAR42");

        // Assert
        hub.Should().NotBeNull();
        hub.Id.Should().NotBeEmpty();
        hub.Name.Should().Be("My Bar");
        hub.Type.Should().Be(HubType.Venue);
        hub.OwnerId.Should().Be(ownerId);
        hub.IsActive.Should().BeFalse();
        hub.SubscriptionTier.Should().Be(SubscriptionTier.FreeTrial);
        hub.Visibility.Should().Be(HubVisibility.Public);
        hub.Settings.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldSetTenantIdToHubId_WhenCreated()
    {
        // Act
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");

        // Assert
        hub.TenantId.Should().Be(hub.Id);
    }

    [Fact]
    public void Create_ShouldGenerateCode_WhenCreated()
    {
        // Act
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");

        // Assert
        hub.Code.Should().NotBeNullOrWhiteSpace();
        hub.QrUrl.Should().Contain(hub.Code);
        hub.DirectLink.Should().Contain(hub.Code);
    }

    [Fact]
    public void Create_ShouldRaiseHubCreatedEvent_WhenCreated()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        var hub = HubEntity.Create("My Bar", HubType.Venue, ownerId, DefaultSettings, "MYBAR42");

        // Assert
        hub.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HubCreatedEvent>()
            .Which.Should().Match<HubCreatedEvent>(e =>
                e.HubId == hub.Id && e.OwnerId == ownerId && e.Name == "My Bar");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrow_WhenNameIsInvalid(string? name)
    {
        // Act
        var act = () => HubEntity.Create(name!, HubType.Venue, Guid.NewGuid(), DefaultSettings, "TEST42");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldAcceptPortableType_WhenProvided()
    {
        // Act
        var hub = HubEntity.Create("Party", HubType.Portable, Guid.NewGuid(), DefaultSettings, "PARTY42");

        // Assert
        hub.Type.Should().Be(HubType.Portable);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue_WhenInactive()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        hub.ClearDomainEvents();

        // Act
        hub.Activate();

        // Assert
        hub.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_ShouldRaiseHubActivatedEvent_WhenActivated()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        hub.ClearDomainEvents();

        // Act
        hub.Activate();

        // Assert
        hub.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HubActivatedEvent>()
            .Which.HubId.Should().Be(hub.Id);
    }

    [Fact]
    public void Activate_ShouldNotRaiseEvent_WhenAlreadyActive()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        hub.Activate();
        hub.ClearDomainEvents();

        // Act
        hub.Activate();

        // Assert
        hub.DomainEvents.Should().BeEmpty();
        hub.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse_WhenActive()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        hub.Activate();
        hub.ClearDomainEvents();

        // Act
        hub.Deactivate();

        // Assert
        hub.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldRaiseHubDeactivatedEvent_WhenDeactivated()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        hub.Activate();
        hub.ClearDomainEvents();

        // Act
        hub.Deactivate();

        // Assert
        hub.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<HubDeactivatedEvent>()
            .Which.HubId.Should().Be(hub.Id);
    }

    [Fact]
    public void Deactivate_ShouldNotRaiseEvent_WhenAlreadyInactive()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        hub.ClearDomainEvents();

        // Act
        hub.Deactivate();

        // Assert
        hub.DomainEvents.Should().BeEmpty();
        hub.IsActive.Should().BeFalse();
    }

    [Fact]
    public void AddMember_ShouldAddMember_WhenUserNotAlreadyMember()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();
        hub.ClearDomainEvents();

        // Act
        var member = hub.AddMember(userId, HubMemberRole.Visitor, assignedBy);

        // Assert
        member.Should().NotBeNull();
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(HubMemberRole.Visitor);
        hub.Members.Should().ContainSingle();
    }

    [Fact]
    public void AddMember_ShouldRaiseMemberJoinedEvent_WhenMemberAdded()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();
        hub.ClearDomainEvents();

        // Act
        hub.AddMember(userId, HubMemberRole.Visitor, Guid.NewGuid());

        // Assert
        hub.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MemberJoinedEvent>()
            .Which.Should().Match<MemberJoinedEvent>(e =>
                e.HubId == hub.Id && e.UserId == userId && e.Role == HubMemberRole.Visitor);
    }

    [Fact]
    public void AddMember_ShouldThrow_WhenUserAlreadyMember()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();
        hub.AddMember(userId, HubMemberRole.Visitor, Guid.NewGuid());

        // Act
        var act = () => hub.AddMember(userId, HubMemberRole.Visitor, Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{userId}'*already a member*");
    }

    [Fact]
    public void AddMember_ShouldAllowNullAssignedBy_WhenSelfJoin()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();

        // Act
        var member = hub.AddMember(userId, HubMemberRole.Visitor, assignedBy: null);

        // Assert
        member.Should().NotBeNull();
    }

    [Fact]
    public void RemoveMember_ShouldRemoveMember_WhenMemberExists()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();
        hub.AddMember(userId, HubMemberRole.Visitor, Guid.NewGuid());

        // Act
        hub.RemoveMember(userId);

        // Assert
        hub.Members.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMember_ShouldThrow_WhenUserNotMember()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();

        // Act
        var act = () => hub.RemoveMember(userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*'{userId}'*not a member*");
    }

    [Fact]
    public void RemoveMember_ShouldThrow_WhenRemovingSuperOwner()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();
        hub.AddMember(userId, HubMemberRole.SuperOwner, Guid.NewGuid());

        // Act
        var act = () => hub.RemoveMember(userId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*super owner*");
    }

    [Fact]
    public void PromoteMember_ShouldPromote_WhenNewRoleIsHigher()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();
        var promotedBy = Guid.NewGuid();
        hub.AddMember(userId, HubMemberRole.Visitor, Guid.NewGuid());

        // Act
        hub.PromoteMember(userId, HubMemberRole.SubListOwner, promotedBy);

        // Assert
        hub.Members.First(m => m.UserId == userId).Role.Should().Be(HubMemberRole.SubListOwner);
    }

    [Fact]
    public void PromoteMember_ShouldThrow_WhenUserNotMember()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");

        // Act
        var act = () => hub.PromoteMember(Guid.NewGuid(), HubMemberRole.SubListOwner, Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not a member*");
    }

    [Fact]
    public void PromoteMember_ShouldThrow_WhenDemoting()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();
        hub.AddMember(userId, HubMemberRole.SubHubManager, Guid.NewGuid());

        // Act
        var act = () => hub.PromoteMember(userId, HubMemberRole.Visitor, Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot demote*");
    }

    [Fact]
    public void PromoteMember_ShouldThrow_WhenAssigningSameRole()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var userId = Guid.NewGuid();
        hub.AddMember(userId, HubMemberRole.Visitor, Guid.NewGuid());

        // Act
        var act = () => hub.PromoteMember(userId, HubMemberRole.Visitor, Guid.NewGuid());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot demote or assign same role*");
    }

    [Fact]
    public void UpdateSettings_ShouldUpdateSettings_WhenValid()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var newSettings = new HubSettings
        {
            AllowProposals = false,
            AutoSkipThreshold = 0.50,
            VotingWindowSeconds = 30,
            MaxQueueSize = 100
        };

        // Act
        hub.UpdateSettings(newSettings);

        // Assert
        hub.Settings.Should().Be(newSettings);
    }

    [Fact]
    public void UpdateSettings_ShouldThrow_WhenSettingsIsNull()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");

        // Act
        var act = () => hub.UpdateSettings(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetVisibility_ShouldUpdateVisibility_WhenCalled()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");

        // Act
        hub.SetVisibility(HubVisibility.Private);

        // Assert
        hub.Visibility.Should().Be(HubVisibility.Private);
    }

    [Fact]
    public void AddMultipleMembers_ShouldTrackAll_WhenDifferentUsers()
    {
        // Arrange
        var hub = HubEntity.Create("My Bar", HubType.Venue, Guid.NewGuid(), DefaultSettings, "MYBAR42");
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();

        // Act
        hub.AddMember(user1, HubMemberRole.Visitor, null);
        hub.AddMember(user2, HubMemberRole.SubListOwner, null);
        hub.AddMember(user3, HubMemberRole.SubHubManager, null);

        // Assert
        hub.Members.Should().HaveCount(3);
    }
}
