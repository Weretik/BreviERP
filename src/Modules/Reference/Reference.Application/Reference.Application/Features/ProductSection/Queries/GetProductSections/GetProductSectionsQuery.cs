
namespace Reference.Application.Features.ProductSection.Queries.GetProductSections;

/// <summary>All product sections: sewing first, then PPE, each in sheet order; an empty list when there are none.</summary>
public sealed record GetProductSectionsQuery : IQuery<Result<List<ProductSectionRowDTO>>>;
