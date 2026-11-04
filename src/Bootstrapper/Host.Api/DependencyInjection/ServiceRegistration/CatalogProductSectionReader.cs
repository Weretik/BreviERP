using Catalog.Application.Contracts.Reference;
using Mediator;
using Reference.Application.Features.ProductSection.GetList;

namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>Gives Catalog the Reference «Типи і розділи» sections through the Reference query.</summary>
internal sealed class CatalogProductSectionReader(ISender sender) : IProductSectionReader
{
    public async Task<ProductSectionItem?> FindAsync(int id, CancellationToken cancellationToken)
    {
        var sections = await sender.Send(new GetProductSectionsQuery(), cancellationToken);
        var section = sections.Value?.FirstOrDefault(x => x.Id == id);
        return section is null ? null : new ProductSectionItem(section.Id, section.Type, section.Name, section.IsActive);
    }

    public async Task<IReadOnlyList<ProductSectionItem>> ListAsync(CancellationToken cancellationToken)
    {
        var sections = await sender.Send(new GetProductSectionsQuery(), cancellationToken);
        return (sections.Value ?? []).Select(x => new ProductSectionItem(x.Id, x.Type, x.Name, x.IsActive)).ToList();
    }
}
