using Reminders.Domain.Chats;

namespace Reminders.Application.Chats.UpdateChat;

public sealed class UpdateReminderChatCommandValidator : AbstractValidator<UpdateReminderChatCommand>
{
    public UpdateReminderChatCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.DisplayName).MaximumLength(ReminderChat.NameMaxLength).When(x => x.Input is not null);
    }
}
