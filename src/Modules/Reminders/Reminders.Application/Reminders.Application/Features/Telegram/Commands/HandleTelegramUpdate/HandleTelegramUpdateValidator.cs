namespace Reminders.Application.Features.Telegram.Commands.HandleTelegramUpdate;

public sealed class HandleTelegramUpdateValidator : AbstractValidator<HandleTelegramUpdateCommand>
{
    public HandleTelegramUpdateValidator()
    {
        RuleFor(x => x.Update).NotNull();
        When(x => x.Update?.CallbackQuery is not null, () =>
        {
            RuleFor(x => x.Update.CallbackQuery!.Id).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Update.CallbackQuery!.Data).MaximumLength(64);
        });
    }
}
