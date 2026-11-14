using Reminders.Domain.Chats;

namespace Reminders.Application.Features.Chats.Commands.UpdateReminderChat;

public sealed class UpdateReminderChatValidator : AbstractValidator<UpdateReminderChatCommand>
{
    public UpdateReminderChatValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.DisplayName).MaximumLength(ReminderChat.NameMaxLength).When(x => x.Input is not null);
    }
}
