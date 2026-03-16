using Musicratic.Hub.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Repositories;

public interface IHubAttachmentRepository : IRepository<HubAttachment>
{
    Task<HubAttachment?> GetActiveAttachment(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HubAttachment>> GetAttachmentsByHub(Guid hubId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HubAttachment>> GetExpiredActive(CancellationToken cancellationToken = default);
}
