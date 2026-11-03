using Reference.Application.Features.ProductSection.Shared.DTOs;
using Reference.Domain.ProductSections.ValueObjects;

namespace Reference.Application.Features.ProductSection.Shared;

internal static class ProductSectionDetailsFactory
{
    public static ProductSectionDetails From(IProductSectionFields fields) => new(
        ProductSectionTypes.Parse(fields.Type), fields.Name, fields.IsActive, fields.Comment, fields.SortOrder);
}
