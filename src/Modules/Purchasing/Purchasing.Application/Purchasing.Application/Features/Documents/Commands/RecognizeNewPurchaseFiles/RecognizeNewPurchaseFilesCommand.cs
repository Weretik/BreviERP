using Purchasing.Application.Abstractions;
using Purchasing.Domain.Ingestion;
using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Application.Features.Documents.Commands.RecognizeNewPurchaseFiles;

/// <summary>
/// One batch of «Розпізнати всі нові»: active safe files never tried before, newest messages first. The admin repeats it
/// until <c>Remaining</c> is 0; it stops early when the AI becomes unavailable.
/// </summary>
public sealed record RecognizeNewPurchaseFilesCommand(string By, int BatchSize = RecognizeNewPurchaseFilesCommand.DefaultBatchSize)
    : ICommand<Result<PurchaseRecognitionBatchResponse>>
{
    public const int DefaultBatchSize = 5;
}
