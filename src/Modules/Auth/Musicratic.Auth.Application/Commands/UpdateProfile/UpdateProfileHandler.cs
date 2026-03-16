using Musicratic.Auth.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.UpdateProfile;

public sealed class UpdateProfileHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProfileCommand>
{
    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User '{request.UserId}' not found.");

        user.UpdateProfile(request.DisplayName, request.AvatarUrl);

        userRepository.Update(user);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
