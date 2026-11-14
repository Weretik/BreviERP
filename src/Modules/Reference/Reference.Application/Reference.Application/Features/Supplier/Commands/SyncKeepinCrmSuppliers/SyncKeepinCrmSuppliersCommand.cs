using Reference.Application.Abstractions;

namespace Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;

/// <summary>«Оновити з KeepinCRM» on the admin page.</summary>
public sealed record SyncKeepinCrmSuppliersCommand : ICommand<Result<KeepinCrmSupplierSyncResult>>;
