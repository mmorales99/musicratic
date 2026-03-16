using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.AttachUser;

public sealed class AttachUserHandler(
    IHubRepository hubRepository,
    IHubAttachmentRepository attachmentRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AttachUserCommand, Guid>
{
    /// <summary>
    /// Attaches a user to a hub. Detaches from any previous hub first.
    /// Attachment expires after 1 hour (Phase 1 rule per docs/03-domain-model.md).
    /// </summary>
    public async Task<Guid> Handle(AttachUserCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetByCode(request.HubCode, cancellationToken)
            ?? throw new InvalidOperationException($"Hub with code '{request.HubCode}' not found.");

        if (!hub.IsActive)
            throw new InvalidOperationException($"Hub '{hub.Name}' is not currently active.");

        // Detach from previous hub if attached
        var existingAttachment = await attachmentRepository.GetActiveAttachment(request.UserId, cancellationToken);
        if (existingAttachment is not null)
        {
            existingAttachment.Expire();
            attachmentRepository.Update(existingAttachment);
        }

        // Create new attachment with 1-hour expiry
        var attachment = HubAttachment.Create(hub.Id, hub.TenantId, request.UserId, TimeSpan.FromHours(1));

        await attachmentRepository.Add(attachment, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        return attachment.Id;
    }
}
