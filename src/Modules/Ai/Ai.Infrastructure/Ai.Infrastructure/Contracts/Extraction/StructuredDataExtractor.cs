using Ai.Application.Abstractions;
using Ai.Contracts.Extraction;
using Ai.Domain.Settings;

using Ardalis.Result;

using Mediator;
using Ai.Application.Features.Extraction.Commands.ExtractStructuredData;

namespace Ai.Infrastructure.Contracts.Extraction;

internal sealed class StructuredDataExtractor(ISender sender) : IStructuredDataExtractor
{
    public async Task<Result<StructuredDataAnswer>> ExtractAsync(StructuredDataRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ExtractStructuredDataCommand(
            request.Model == AiModelChoice.Strong ? AiModelTier.Strong : AiModelTier.Fast,
            request.Instructions,
            request.Text,
            request.Files.Select(x => new AiFile(x.FileName, x.MediaType, x.Content)).ToArray(),
            request.SchemaName,
            request.SchemaJson,
            request.PromptKey), cancellationToken);
        return result.Map(x => new StructuredDataAnswer(x.Json, x.Model, x.CostUsd));
    }
}
