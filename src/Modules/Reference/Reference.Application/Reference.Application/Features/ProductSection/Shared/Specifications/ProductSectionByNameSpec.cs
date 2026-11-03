using Reference.Domain.ProductSections.ValueObjects;
using ProductSectionEntity = Reference.Domain.ProductSections.Entities.ProductSection;

namespace Reference.Application.Features.ProductSection.Shared.Specifications;

/// <summary>A section with this exact name, other than <paramref name="exceptId"/>.</summary>
public sealed class ProductSectionByNameSpec : Specification<ProductSectionEntity>
{
    public ProductSectionByNameSpec(string name, int? exceptId = null)
    {
        Query.Where(x => x.Name == name);
        if (exceptId is { } id)
            Query.Where(x => x.Id != ProductSectionId.From(id));
    }
}
