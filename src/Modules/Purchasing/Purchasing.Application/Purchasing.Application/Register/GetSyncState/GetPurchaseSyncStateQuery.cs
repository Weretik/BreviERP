using Purchasing.Application.Contracts;

namespace Purchasing.Application.Register.GetSyncState;

public sealed record GetPurchaseSyncStateQuery : IQuery<Result<PurchaseSyncStateResponse>>;
