namespace Reminders.Application.Features.Chats.Commands.SendReminderChatTest;

public sealed class SendReminderChatTestValidator : AbstractValidator<SendReminderChatTestCommand>
{
    public SendReminderChatTestValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
