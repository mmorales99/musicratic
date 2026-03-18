using Musicratic.Auth.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.CreditWallet;

public sealed class CreditWalletHandler(
    IUserRepository userRepository,
    IAuthUnitOfWork unitOfWork) : ICommandHandler<CreditWalletCommand>
{
    public async Task Handle(CreditWalletCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User '{request.UserId}' not found.");

        user.CreditWallet(request.Amount);

        userRepository.Update(user);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
