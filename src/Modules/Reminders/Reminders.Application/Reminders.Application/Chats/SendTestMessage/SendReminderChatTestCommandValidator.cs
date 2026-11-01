namespace Reminders.Application.Chats.SendTestMessage;

public sealed class SendReminderChatTestCommandValidator : AbstractValidator<SendReminderChatTestCommand>
{
    public SendReminderChatTestCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
