using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.PromoteMember;

public sealed class PromoteMemberHandler(
    IHubRepository hubRepository,
    IHubMemberRepository memberRepository,
    IHubUnitOfWork unitOfWork) : ICommandHandler<PromoteMemberCommand>
{
    // Tier limits from docs/07-user-roles.md
    private static readonly Dictionary<SubscriptionTier, (int SubHubManagers, int SubListOwners)> TierLimits = new()
    {
        [SubscriptionTier.FreeTrial] = (1, 3),
        [SubscriptionTier.Monthly] = (5, 15),
        [SubscriptionTier.Annual] = (5, 15),
        [SubscriptionTier.Event] = (5, 15)
    };

    public async Task Handle(PromoteMemberCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetByIdWithMembers(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        var promoter = hub.Members.FirstOrDefault(m => m.UserId == request.PromotedBy)
            ?? throw new InvalidOperationException($"Promoter '{request.PromotedBy}' is not a member of this hub.");

        if (promoter.Role < HubMemberRole.SuperOwner)
            throw new InvalidOperationException("Only the super owner can promote members.");

        await ValidateTierLimits(hub, request.NewRole, cancellationToken);

        hub.PromoteMember(request.TargetUserId, request.NewRole, request.PromotedBy);

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);
    }

    private async Task ValidateTierLimits(
        Domain.Entities.Hub hub,
        HubMemberRole newRole,
        CancellationToken cancellationToken)
    {
        if (!TierLimits.TryGetValue(hub.SubscriptionTier, out var limits))
            return;

        if (newRole == HubMemberRole.SubHubManager)
        {
            var currentCount = await memberRepository.CountByRole(
                hub.Id, HubMemberRole.SubHubManager, cancellationToken);

            if (currentCount >= limits.SubHubManagers)
                throw new InvalidOperationException(
                    $"Hub has reached the maximum number of Sub Hub Managers ({limits.SubHubManagers}) for the {hub.SubscriptionTier} tier.");
        }
        else if (newRole == HubMemberRole.SubListOwner)
        {
            var currentCount = await memberRepository.CountByRole(
                hub.Id, HubMemberRole.SubListOwner, cancellationToken);

            if (currentCount >= limits.SubListOwners)
                throw new InvalidOperationException(
                    $"Hub has reached the maximum number of Sub List Owners ({limits.SubListOwners}) for the {hub.SubscriptionTier} tier.");
        }
    }
}
