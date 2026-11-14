using Ai.Contracts.Extraction;
using Ai.Contracts.Prompts;

using Ardalis.Result;

using Purchasing.Application.Abstractions;

namespace Purchasing.Infrastructure.Adapters.Ai;

/// <summary>Purchase invoices read by the AI of «Налаштування ШІ» (module Ai), with the admin's extra rules for invoices.</summary>
internal sealed class PurchaseDocumentRecognizer(IStructuredDataExtractor extractor) : IPurchaseDocumentRecognizer
{
    public async Task<PurchaseRecognitionAnswer> RecognizeAsync(PurchaseRecognitionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await extractor.ExtractAsync(new StructuredDataRequest(
            request.Strong ? AiModelChoice.Strong : AiModelChoice.Fast,
            request.Instructions,
            request.Text,
            [new AiInputFile(request.File.FileName, request.File.MediaType, request.File.Content)],
            request.SchemaName,
            request.SchemaJson,
            AiPromptKeys.PurchaseInvoices), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok => new PurchaseRecognitionAnswer(PurchaseRecognitionStatus.Answered, result.Value.Json,
                result.Value.Model, result.Value.CostUsd, null),
            ResultStatus.Unavailable => new PurchaseRecognitionAnswer(PurchaseRecognitionStatus.Unavailable, null, null, 0m,
                string.Join(" ", result.Errors)),
            _ => new PurchaseRecognitionAnswer(PurchaseRecognitionStatus.Failed, null, null, 0m,
                string.Join(" ", result.Errors.Concat(result.ValidationErrors.Select(x => x.ErrorMessage)))),
        };
    }
}
