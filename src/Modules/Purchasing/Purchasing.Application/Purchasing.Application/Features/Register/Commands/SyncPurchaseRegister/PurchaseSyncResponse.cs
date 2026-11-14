namespace Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

/// <summary>What one sheet sync changed.</summary>
public sealed record PurchaseSyncResponse(
    int RecordCount,
    int FileCount,
    int MessagesAdded,
    int MessagesUpdated,
    int RowsAdded,
    int RowsUpdated,
    int RowsDeactivated,
    int FilesAdded,
    int FilesDeactivated,
    DateTimeOffset SyncedAt);
