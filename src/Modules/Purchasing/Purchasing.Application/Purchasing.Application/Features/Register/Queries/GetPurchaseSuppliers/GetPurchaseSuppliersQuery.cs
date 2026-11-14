using Purchasing.Application.Abstractions;

namespace Purchasing.Application.Features.Register.Queries.GetPurchaseSuppliers;

/// <summary>
/// Active suppliers for «Постачальник» in the invoice panel. The accountant cannot read the Reference supplier list
/// (product management policy), so the register offers its own short list.
/// </summary>
public sealed record GetPurchaseSuppliersQuery : IQuery<Result<IReadOnlyList<PurchaseSupplierOption>>>;
