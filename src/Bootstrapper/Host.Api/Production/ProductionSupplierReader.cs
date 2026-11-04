using Mediator;
using Production.Application.Abstractions;
using Reference.Application.Features.Supplier.GetList;

namespace Host.Api.Production;

/// <summary>«Постачальники» (Reference) for the supplier of a fabric receipt.</summary>
internal sealed class ProductionSupplierReader(ISender sender) : IProductionSupplierReader
{
    public async Task<ProductionSupplier?> FindAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSuppliersQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference suppliers could not be read.");
        return result.Value.Where(x => x.Id == id).Select(x => new ProductionSupplier(x.Id, x.Name)).FirstOrDefault();
    }
}
