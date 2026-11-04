using Catalog.Application.Features.Product.Overview.GetList;
using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Domain.Products.Enums;
using Mediator;
using Production.Application.Abstractions;

namespace Host.Api.Production;

/// <summary>Gives the workshop the Catalog sewing models (about 130) by paging through the product list.</summary>
internal sealed class ProductionModelReader(ISender sender) : IProductionModelReader
{
    public async Task<IReadOnlyList<ProductionModel>> ListSewingModelsAsync(CancellationToken cancellationToken)
    {
        var rows = new List<ProductOverviewItem>();
        for (var page = 1; ; page++)
        {
            var result = await sender.Send(new GetProductOverviewPageQuery(ProductType.Sewing, null, null, false, false,
                ProductOverviewSort.Code, false, page, GetProductOverviewPageQuery.MaxPageSize), cancellationToken);
            if (!result.IsSuccess) throw new InvalidOperationException("Catalog sewing models could not be read.");
            rows.AddRange(result.Value.Items);
            if (rows.Count >= result.Value.Total || result.Value.Items.Count == 0) break;
        }

        return rows.Select(x => new ProductionModel(x.Code, x.Name, x.IsActive)).ToArray();
    }
}
