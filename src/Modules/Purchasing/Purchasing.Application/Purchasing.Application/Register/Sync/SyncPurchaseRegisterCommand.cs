using Purchasing.Application.Contracts;

namespace Purchasing.Application.Register.Sync;

/// <summary>«Оновити зараз»: the same run as the daily job.</summary>
public sealed record SyncPurchaseRegisterCommand : ICommand<Result<PurchaseSyncResponse>>;
