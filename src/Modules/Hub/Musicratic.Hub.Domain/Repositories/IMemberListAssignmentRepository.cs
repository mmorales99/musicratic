using Musicratic.Hub.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Repositories;

public interface IMemberListAssignmentRepository : IRepository<MemberListAssignment>
{
    Task<MemberListAssignment?> GetAssignment(
        Guid memberId, Guid listId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemberListAssignment>> GetByMember(
        Guid memberId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetAssignedListIds(
        Guid memberId, CancellationToken cancellationToken = default);
}
