using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DetachUser;

public sealed class DetachUserHandler(
    IHubAttachmentRepository attachmentRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DetachUserCommand>
{
    public async Task Handle(DetachUserCommand request, CancellationToken cancellationToken)
    {
        var attachment = await attachmentRepository.GetActiveAttachment(request.UserId, cancellationToken);

        if (attachment is null)
            return; // Already detached — idempotent

        attachment.Expire();
        attachmentRepository.Update(attachment);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
