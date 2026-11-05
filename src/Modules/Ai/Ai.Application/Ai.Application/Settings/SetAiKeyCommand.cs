using Ai.Application.Abstractions;
using Ai.Application.Contracts;
using Ai.Domain.Settings;

namespace Ai.Application.Settings;

/// <summary>Stores a new OpenAI key encrypted; the key itself is never returned or logged.</summary>
public sealed record SetAiKeyCommand(string ApiKey, string By) : ICommand<Result<AiSettingsResponse>>
{
    public override string ToString() => $"{nameof(SetAiKeyCommand)} {{ By = {By} }}";
}

public sealed class SetAiKeyCommandValidator : AbstractValidator<SetAiKeyCommand>
{
    public const string Message = "Ключ OpenAI — від 20 до 300 символів без пробілів.";

    public SetAiKeyCommandValidator()
    {
        RuleFor(x => x.ApiKey).Must(key => key is { Length: >= 20 and <= 300 } && !key.Any(char.IsWhiteSpace))
            .WithMessage(Message);
        RuleFor(x => x.By).NotEmpty().MaximumLength(AiSettings.UpdatedByMaxLength);
    }
}

public sealed class SetAiKeyCommandHandler(IAiDbContext db, AiSettingsStore store, IAiKeyProtector protector, TimeProvider clock)
    : ICommandHandler<SetAiKeyCommand, Result<AiSettingsResponse>>
{
    public async ValueTask<Result<AiSettingsResponse>> Handle(SetAiKeyCommand command, CancellationToken cancellationToken)
    {
        var settings = await store.LoadForUpdateAsync(cancellationToken);
        settings.SetKey(protector.Protect(command.ApiKey), AiSettings.HintOf(command.ApiKey), command.By, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await store.ResponseAsync(settings, cancellationToken));
    }
}

public sealed record RemoveAiKeyCommand(string By) : ICommand<Result<AiSettingsResponse>>;

public sealed class RemoveAiKeyCommandHandler(IAiDbContext db, AiSettingsStore store, TimeProvider clock)
    : ICommandHandler<RemoveAiKeyCommand, Result<AiSettingsResponse>>
{
    public async ValueTask<Result<AiSettingsResponse>> Handle(RemoveAiKeyCommand command, CancellationToken cancellationToken)
    {
        var settings = await store.LoadForUpdateAsync(cancellationToken);
        if (settings.RemoveKey(command.By, clock.GetUtcNow())) await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await store.ResponseAsync(settings, cancellationToken));
    }
}
