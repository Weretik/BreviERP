using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Settings;

namespace Production.Application.Settings;

internal static class SettingsMessages
{
    public const string CannotManage = "Налаштування цеху змінює начальник цеху або адміністратор.";
    public const string ChatRequired = "Оберіть групу, щоб увімкнути надсилання.";
    public const string NoChat = "Спершу оберіть групу цеху.";
    public const string BotNotConfigured = "На сервері немає токена бота.";
}

/// <summary>«Налаштування цеху» with the groups the bot is in.</summary>
public sealed record GetWorkshopSettingsQuery(ProductionActor Actor) : IQuery<Result<WorkshopSettingsResponse>>;

public sealed class GetWorkshopSettingsQueryValidator : AbstractValidator<GetWorkshopSettingsQuery>
{
    public GetWorkshopSettingsQueryValidator() => RuleFor(x => x.Actor).ValidActor();
}

public sealed record UpdateWorkshopSettingsCommand(ProductionActor Actor, WorkshopSettingsInput Input)
    : ICommand<Result<WorkshopSettingsResponse>>;

public sealed class UpdateWorkshopSettingsCommandValidator : AbstractValidator<UpdateWorkshopSettingsCommand>
{
    public UpdateWorkshopSettingsCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.TelegramChatId).NotNull().WithMessage(SettingsMessages.ChatRequired)
            .When(x => x.Input is { TelegramEnabled: true });
        RuleFor(x => x.Input.TelegramThreadId).GreaterThan(0).When(x => x.Input?.TelegramThreadId is not null)
            .WithMessage("Номер теми — ціле число від 1.");
    }
}

/// <summary>Sends a test line to the chosen group and topic.</summary>
public sealed record SendWorkshopTelegramTestCommand(ProductionActor Actor) : ICommand<Result>;

public sealed class SendWorkshopTelegramTestCommandValidator : AbstractValidator<SendWorkshopTelegramTestCommand>
{
    public SendWorkshopTelegramTestCommandValidator() => RuleFor(x => x.Actor).ValidActor();
}

public sealed class WorkshopSettingsHandler(
    IProductionDbContext db, IProductionStaffReader staff, IProductionTelegram telegram, TimeProvider clock) :
    IQueryHandler<GetWorkshopSettingsQuery, Result<WorkshopSettingsResponse>>,
    ICommandHandler<UpdateWorkshopSettingsCommand, Result<WorkshopSettingsResponse>>,
    ICommandHandler<SendWorkshopTelegramTestCommand, Result>
{
    public async ValueTask<Result<WorkshopSettingsResponse>> Handle(GetWorkshopSettingsQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.CanManageSettings)
            return Result<WorkshopSettingsResponse>.Forbidden(SettingsMessages.CannotManage);
        return Result.Success(await ResponseAsync(await ReadAsync(cancellationToken), cancellationToken));
    }

    public async ValueTask<Result<WorkshopSettingsResponse>> Handle(UpdateWorkshopSettingsCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanManageSettings)
            return Result<WorkshopSettingsResponse>.Forbidden(SettingsMessages.CannotManage);
        var actorName = (await staff.FindAsync(command.Actor.UserId, cancellationToken))?.Name;
        if (actorName is null) return Result<WorkshopSettingsResponse>.Forbidden("Не вдалося визначити користувача.");
        var settings = await db.Settings.SingleOrDefaultAsync(x => x.Id == WorkshopSettings.SingletonId,
            cancellationToken);
        if (settings is null)
        {
            settings = new WorkshopSettings(null, null);
            db.Settings.Add(settings);
        }

        var input = command.Input;
        settings.Update(input.TelegramEnabled, input.TelegramChatId, input.TelegramThreadId, actorName,
            clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await ResponseAsync(settings, cancellationToken));
    }

    public async ValueTask<Result> Handle(SendWorkshopTelegramTestCommand command, CancellationToken cancellationToken)
    {
        if (!command.Actor.CanManageSettings) return Result.Forbidden(SettingsMessages.CannotManage);
        if (!telegram.IsConfigured) return Result.Conflict(SettingsMessages.BotNotConfigured);
        var settings = await ReadAsync(cancellationToken);
        if (settings?.TelegramChatId is not { } chatId) return Result.Conflict(SettingsMessages.NoChat);
        try
        {
            await telegram.SendAsync(chatId, settings.TelegramThreadId,
                "Перевірка: сюди «Швейний цех» Brevi надсилатиме нові завдання.", cancellationToken);
            return Result.Success();
        }
        catch (ProductionTelegramException error)
        {
            return Result.Error(error.Message);
        }
    }

    private Task<WorkshopSettings?> ReadAsync(CancellationToken cancellationToken) =>
        db.Settings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == WorkshopSettings.SingletonId, cancellationToken);

    private async Task<WorkshopSettingsResponse> ResponseAsync(WorkshopSettings? settings,
        CancellationToken cancellationToken)
    {
        var chats = telegram.IsConfigured ? await telegram.ListChatsAsync(cancellationToken) : [];
        return new WorkshopSettingsResponse(settings?.TelegramEnabled ?? false, settings?.TelegramChatId,
            settings?.TelegramThreadId, telegram.IsConfigured,
            chats.Select(x => new WorkshopTelegramChatResponse(x.ChatId, x.Title)).ToArray(),
            settings?.UpdatedByName, settings?.UpdatedAt);
    }
}
