using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.UnassignListFromMember;

public sealed class UnassignListFromMemberHandler(
    IHubMemberRepository memberRepository,
    IMemberListAssignmentRepository assignmentRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UnassignListFromMemberCommand>
{
    public async Task Handle(UnassignListFromMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetMember(request.HubId, request.TargetUserId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"User '{request.TargetUserId}' is not a member of hub '{request.HubId}'.");

        var assignment = await assignmentRepository.GetAssignment(
            member.Id, request.ListId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"List '{request.ListId}' is not assigned to this member.");

        assignmentRepository.Remove(assignment);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
