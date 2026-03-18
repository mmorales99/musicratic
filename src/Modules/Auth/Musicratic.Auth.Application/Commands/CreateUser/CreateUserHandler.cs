using Musicratic.Auth.Domain.Entities;
using Musicratic.Auth.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.CreateUser;

public sealed class CreateUserHandler(
    IUserRepository userRepository,
    IAuthUnitOfWork unitOfWork) : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(
            request.AuthentikSub,
            request.DisplayName,
            request.Email,
            request.AvatarUrl);

        await userRepository.Add(user, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        return user.Id;
    }
}
