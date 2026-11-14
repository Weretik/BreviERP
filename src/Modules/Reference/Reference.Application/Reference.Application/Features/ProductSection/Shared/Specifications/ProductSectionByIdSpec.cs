using Reference.Domain.ProductSections;
using ProductSectionEntity = Reference.Domain.ProductSections.ProductSection;

namespace Reference.Application.Features.ProductSection.Shared.Specifications;

public sealed class ProductSectionByIdSpec : Specification<ProductSectionEntity>
{
    public ProductSectionByIdSpec(int id) => Query.Where(x => x.Id == ProductSectionId.From(id));
}
