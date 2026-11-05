using Purchasing.Application.Abstractions;
using Purchasing.Application.Contracts;

namespace Purchasing.Application.Register.Review;

/// <summary>
/// Active suppliers for «Постачальник» in the invoice panel. The accountant cannot read the Reference supplier list
/// (product management policy), so the register offers its own short list.
/// </summary>
public sealed record GetPurchaseSuppliersQuery : IQuery<Result<IReadOnlyList<PurchaseSupplierOption>>>;

public sealed class GetPurchaseSuppliersQueryHandler(IPurchasingSupplierReader suppliers)
    : IQueryHandler<GetPurchaseSuppliersQuery, Result<IReadOnlyList<PurchaseSupplierOption>>>
{
    public async ValueTask<Result<IReadOnlyList<PurchaseSupplierOption>>> Handle(GetPurchaseSuppliersQuery query,
        CancellationToken cancellationToken)
    {
        var options = (await suppliers.ListAsync(cancellationToken))
            .Where(x => x.Active)
            .OrderBy(x => x.Name, StringComparer.Create(CultureInfo.GetCultureInfo("uk-UA"), true))
            .Select(x => new PurchaseSupplierOption(x.Id, x.Name))
            .ToList();
        return Result.Success<IReadOnlyList<PurchaseSupplierOption>>(options);
    }
}
