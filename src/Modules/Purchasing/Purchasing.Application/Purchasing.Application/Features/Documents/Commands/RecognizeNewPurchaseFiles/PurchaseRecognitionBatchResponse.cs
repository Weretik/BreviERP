namespace Purchasing.Application.Features.Documents.Commands.RecognizeNewPurchaseFiles;

/// <summary>
/// «Розпізнати нові»: files never tried before, newest messages first, at most the batch size. <see cref="StopReason"/>
/// is set when the AI became unavailable (off, no key, monthly limit).
/// </summary>
public sealed record PurchaseRecognitionBatchResponse(
    int Recognized,
    int NeedsReview,
    int Failed,
    int Skipped,
    int Remaining,
    decimal CostUsd,
    string? StopReason);
