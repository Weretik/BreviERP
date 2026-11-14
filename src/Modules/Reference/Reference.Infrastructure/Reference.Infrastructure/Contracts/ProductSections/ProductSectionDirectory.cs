using Mediator;
using Reference.Contracts.ProductSections;
using Reference.Application.Features.ProductSection.Queries.GetProductSections;

namespace Reference.Infrastructure.Contracts.ProductSections;

internal sealed class ProductSectionDirectory(ISender sender) : IProductSectionDirectory
{
    public async Task<IReadOnlyList<ProductSectionInfo>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductSectionsQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference product sections could not be read.");
        return result.Value.Select(x => new ProductSectionInfo(x.Id, x.Type, x.Name, x.IsActive)).ToArray();
    }
}
