
namespace Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

/// <summary>«Оновити зараз»: the same run as the daily job.</summary>
public sealed record SyncPurchaseRegisterCommand : ICommand<Result<PurchaseSyncResponse>>;
