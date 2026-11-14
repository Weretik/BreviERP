using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Domain.Prompts;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.UpdateAiPrompt;

/// <summary>Saves the extra rules of one request kind; empty text removes them.</summary>
public sealed record UpdateAiPromptCommand(string Key, string? Text, string By) : ICommand<Result<AiSettingsResponse>>;

public sealed class UpdateAiPromptValidator : AbstractValidator<UpdateAiPromptCommand>
{
    public UpdateAiPromptValidator()
    {
        RuleFor(x => x.Text).Must(text => text is null || text.Trim().Length <= AiPrompt.TextMaxLength)
            .WithMessage($"Умови — не більше {AiPrompt.TextMaxLength} символів.");
        RuleFor(x => x.By).NotEmpty().MaximumLength(AiSettings.UpdatedByMaxLength);
    }
}

public sealed class UpdateAiPromptHandler(IAiDbContext db, AiSettingsStore store, TimeProvider clock)
    : ICommandHandler<UpdateAiPromptCommand, Result<AiSettingsResponse>>
{
    public async ValueTask<Result<AiSettingsResponse>> Handle(UpdateAiPromptCommand command, CancellationToken cancellationToken)
    {
        if (!AiPromptKeys.IsKnown(command.Key)) return Result<AiSettingsResponse>.NotFound("Таких умов немає.");
        var prompt = await db.Prompts.FirstOrDefaultAsync(x => x.Id == command.Key, cancellationToken);
        if (prompt is null) db.Prompts.Add(prompt = new AiPrompt(command.Key));
        if (prompt.Update(command.Text ?? string.Empty, command.By, clock.GetUtcNow())) await db.SaveChangesAsync(cancellationToken);

        var settings = await db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == AiSettings.OpenAi, cancellationToken)
            ?? AiSettings.CreateDefault();
        return Result.Success(await store.ResponseAsync(settings, cancellationToken));
    }
}
