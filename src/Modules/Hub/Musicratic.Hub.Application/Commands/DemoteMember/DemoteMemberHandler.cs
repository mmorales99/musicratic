using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DemoteMember;

public sealed class DemoteMemberHandler(
    IHubRepository hubRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DemoteMemberCommand>
{
    public async Task Handle(DemoteMemberCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetByIdWithMembers(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        var demoter = hub.Members.FirstOrDefault(m => m.UserId == request.DemotedBy)
            ?? throw new InvalidOperationException($"Demoter '{request.DemotedBy}' is not a member of this hub.");

        if (demoter.Role < HubMemberRole.SuperOwner)
            throw new InvalidOperationException("Only the super owner can demote members.");

        hub.DemoteMember(request.TargetUserId, request.NewRole, request.DemotedBy);

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
