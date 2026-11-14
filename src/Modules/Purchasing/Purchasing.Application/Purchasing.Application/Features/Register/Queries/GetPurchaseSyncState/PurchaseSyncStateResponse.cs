namespace Purchasing.Application.Features.Register.Queries.GetPurchaseSyncState;

/// <summary>The last sheet sync; all null/zero before the first one.</summary>
public sealed record PurchaseSyncStateResponse(
    DateTimeOffset? LastSuccessAt,
    DateTimeOffset? LastAttemptAt,
    string? Error,
    int RecordCount,
    int FileCount);
