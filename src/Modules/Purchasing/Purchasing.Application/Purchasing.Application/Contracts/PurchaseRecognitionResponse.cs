namespace Purchasing.Application.Contracts;

/// <summary>Result of «Розпізнати» for one file.</summary>
public sealed record PurchaseRecognitionResponse(
    string FileId,
    PurchaseRecognitionResult Result,
    IReadOnlyList<string> Problems,
    string? Model,
    decimal CostUsd);

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

/// <summary>Saving a checked document by hand: <see cref="Unchanged"/> when the same facts were already saved.</summary>
public sealed record PurchaseDocumentSaveResponse(string FileId, bool Unchanged, bool Replaced);
