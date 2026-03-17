using FluentValidation;

namespace Musicratic.Notification.Application.Commands.UpdatePreference;

public sealed class UpdatePreferenceValidator : AbstractValidator<UpdatePreferenceCommand>
{
    public UpdatePreferenceValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Preferences)
            .NotEmpty()
            .WithMessage("At least one preference must be provided.");

        RuleForEach(x => x.Preferences).ChildRules(item =>
        {
            item.RuleFor(x => x.NotificationType)
                .IsInEnum()
                .WithMessage("Invalid notification type.");

            item.RuleFor(x => x.Channel)
                .IsInEnum()
                .WithMessage("Invalid notification channel.");
        });
    }
}
