using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Calls.Shared;

namespace Ai.Application.Features.Extraction.Commands.ExtractStructuredData;

public sealed class ExtractStructuredDataHandler(AiCaller caller)
    : ICommandHandler<ExtractStructuredDataCommand, Result<AiExtraction>>
{
    public async ValueTask<Result<AiExtraction>> Handle(ExtractStructuredDataCommand command, CancellationToken cancellationToken) =>
        await caller.CallAsync(command.Tier, command.Instructions, command.Text, command.Files, command.SchemaName,
            command.SchemaJson, requireEnabled: true, cancellationToken, command.PromptKey);
}
