using FluentValidation;

namespace Musicratic.Hub.Application.Commands.CreateHub;

public sealed class CreateHubValidator : AbstractValidator<CreateHubCommand>
{
    public CreateHubValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("Hub name must be between 3 and 100 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid hub type.");

        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("Owner ID is required.");

        RuleFor(x => x.Settings)
            .NotNull()
            .WithMessage("Hub settings are required.");
    }
}
