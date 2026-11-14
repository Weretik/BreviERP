namespace Reminders.Application.Features.Telegram.Commands.HandleTelegramUpdate;

public sealed class HandleTelegramUpdateHandler(ReminderButtonHandler buttons, ReminderChatTracker chats)
    : ICommandHandler<HandleTelegramUpdateCommand, Result>
{
    public async ValueTask<Result> Handle(HandleTelegramUpdateCommand command, CancellationToken cancellationToken)
    {
        var update = command.Update;
        if (update.CallbackQuery is not null) await buttons.HandleAsync(update.CallbackQuery, cancellationToken);
        else if (update.MyChatMember is not null) await chats.MembershipChangedAsync(update.MyChatMember, cancellationToken);
        else if (update.Message is not null) await chats.MessageReceivedAsync(update.Message, cancellationToken);
        return Result.Success();
    }
}
