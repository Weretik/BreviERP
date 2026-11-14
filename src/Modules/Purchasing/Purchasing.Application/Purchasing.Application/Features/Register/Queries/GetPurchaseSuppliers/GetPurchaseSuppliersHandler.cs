using Purchasing.Application.Abstractions;

namespace Purchasing.Application.Features.Register.Queries.GetPurchaseSuppliers;

public sealed class GetPurchaseSuppliersHandler(IPurchasingSupplierReader suppliers)
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
