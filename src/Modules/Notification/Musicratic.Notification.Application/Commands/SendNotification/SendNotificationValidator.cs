using FluentValidation;

namespace Musicratic.Notification.Application.Commands.SendNotification;

public sealed class SendNotificationValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Title is required and must be at most 200 characters.");

        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("Body is required and must be at most 2000 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid notification type.");
    }
}
