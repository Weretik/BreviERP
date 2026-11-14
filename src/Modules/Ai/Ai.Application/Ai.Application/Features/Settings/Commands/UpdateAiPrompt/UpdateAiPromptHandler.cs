using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Domain.Prompts;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.UpdateAiPrompt;

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
