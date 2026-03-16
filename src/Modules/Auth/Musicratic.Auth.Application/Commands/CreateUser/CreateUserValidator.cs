using FluentValidation;

namespace Musicratic.Auth.Application.Commands.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.AuthentikSub)
            .NotEmpty()
            .WithMessage("Authentik subject identifier is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
            .WithMessage("Display name must be between 2 and 50 characters.");
    }
}
