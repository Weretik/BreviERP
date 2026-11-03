using Reference.Domain.ProductSections.ValueObjects;
using ProductSectionEntity = Reference.Domain.ProductSections.Entities.ProductSection;

namespace Reference.Application.Features.ProductSection.Shared.Specifications;

public sealed class ProductSectionByIdSpec : Specification<ProductSectionEntity>
{
    public ProductSectionByIdSpec(int id) => Query.Where(x => x.Id == ProductSectionId.From(id));
}
