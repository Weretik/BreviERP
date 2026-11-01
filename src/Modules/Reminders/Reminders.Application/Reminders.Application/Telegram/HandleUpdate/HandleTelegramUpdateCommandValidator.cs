namespace Reminders.Application.Telegram.HandleUpdate;

public sealed class HandleTelegramUpdateCommandValidator : AbstractValidator<HandleTelegramUpdateCommand>
{
    public HandleTelegramUpdateCommandValidator()
    {
        RuleFor(x => x.Update).NotNull();
        When(x => x.Update?.CallbackQuery is not null, () =>
        {
            RuleFor(x => x.Update.CallbackQuery!.Id).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Update.CallbackQuery!.Data).MaximumLength(64);
        });
    }
}
