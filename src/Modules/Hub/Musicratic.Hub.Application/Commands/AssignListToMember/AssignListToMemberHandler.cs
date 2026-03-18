using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.AssignListToMember;

public sealed class AssignListToMemberHandler(
    IHubMemberRepository memberRepository,
    IListRepository listRepository,
    IMemberListAssignmentRepository assignmentRepository,
    IHubUnitOfWork unitOfWork) : ICommandHandler<AssignListToMemberCommand>
{
    public async Task Handle(AssignListToMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetMember(request.HubId, request.TargetUserId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"User '{request.TargetUserId}' is not a member of hub '{request.HubId}'.");

        if (member.Role != HubMemberRole.SubListOwner)
            throw new InvalidOperationException(
                "List assignments are only valid for members with the SubListOwner role.");

        var list = await listRepository.GetById(request.ListId, cancellationToken)
            ?? throw new InvalidOperationException($"List '{request.ListId}' not found.");

        if (list.HubId != request.HubId)
            throw new InvalidOperationException("List does not belong to the specified hub.");

        var existing = await assignmentRepository.GetAssignment(
            member.Id, request.ListId, cancellationToken);

        if (existing is not null)
            throw new InvalidOperationException("This list is already assigned to this member.");

        var assignment = MemberListAssignment.Create(
            member.Id, request.ListId, member.TenantId, request.AssignedBy);

        await assignmentRepository.Add(assignment, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
