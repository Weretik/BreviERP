using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.UpdateAiSettings;

/// <summary>Models, prices, on/off and the monthly limit. <paramref name="Version"/> is the version the admin saw.</summary>
public sealed record UpdateAiSettingsCommand(
    bool Enabled,
    string FastModel,
    decimal FastInputPrice,
    decimal FastOutputPrice,
    string StrongModel,
    decimal StrongInputPrice,
    decimal StrongOutputPrice,
    decimal MonthlyLimitUsd,
    int Version,
    string By) : ICommand<Result<AiSettingsResponse>>;

public sealed class UpdateAiSettingsValidator : AbstractValidator<UpdateAiSettingsCommand>
{
    public UpdateAiSettingsValidator()
    {
        RuleFor(x => x.FastModel).Must(AiModelName.IsValid).WithMessage(AiModelName.Message);
        RuleFor(x => x.StrongModel).Must(AiModelName.IsValid).WithMessage(AiModelName.Message);
        RuleFor(x => x.FastInputPrice).InclusiveBetween(0, AiModelPrice.Max).PrecisionScale(10, 4, true);
        RuleFor(x => x.FastOutputPrice).InclusiveBetween(0, AiModelPrice.Max).PrecisionScale(10, 4, true);
        RuleFor(x => x.StrongInputPrice).InclusiveBetween(0, AiModelPrice.Max).PrecisionScale(10, 4, true);
        RuleFor(x => x.StrongOutputPrice).InclusiveBetween(0, AiModelPrice.Max).PrecisionScale(10, 4, true);
        RuleFor(x => x.MonthlyLimitUsd).InclusiveBetween(0, AiSettings.MonthlyLimitMax).PrecisionScale(10, 2, true)
            .WithMessage($"Ліміт на місяць — від 0 до {AiSettings.MonthlyLimitMax} $.");
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(AiSettings.UpdatedByMaxLength);
    }
}

public sealed class UpdateAiSettingsHandler(IAiDbContext db, AiSettingsStore store, TimeProvider clock)
    : ICommandHandler<UpdateAiSettingsCommand, Result<AiSettingsResponse>>
{
    public const string Stale = "Налаштування вже змінили. Відкрийте сторінку знову.";
    public const string KeyRequired = "Спочатку додайте ключ OpenAI, потім вмикайте ШІ.";

    public async ValueTask<Result<AiSettingsResponse>> Handle(UpdateAiSettingsCommand command, CancellationToken cancellationToken)
    {
        var settings = await store.LoadForUpdateAsync(cancellationToken);
        if (settings.Version != command.Version) return Result<AiSettingsResponse>.Conflict(Stale);
        if (command.Enabled && !settings.HasKey)
            return Result<AiSettingsResponse>.Invalid(new ValidationError(nameof(command.Enabled), KeyRequired));

        if (settings.Update(command.Enabled, command.FastModel, new AiModelPrice(command.FastInputPrice, command.FastOutputPrice),
                command.StrongModel, new AiModelPrice(command.StrongInputPrice, command.StrongOutputPrice),
                command.MonthlyLimitUsd, command.By, clock.GetUtcNow()))
            await db.SaveChangesAsync(cancellationToken);

        return Result.Success(await store.ResponseAsync(settings, cancellationToken));
    }
}
