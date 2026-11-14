using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductSection.GetList.DTOs;
using Reference.Application.Features.ProductSection.Shared;
using ProductSectionEntity = Reference.Domain.ProductSections.ProductSection;

namespace Reference.Application.Features.ProductSection.GetList;

public sealed class GetProductSectionsQueryHandler(IReferenceReadRepository<ProductSectionEntity> repository)
    : IQueryHandler<GetProductSectionsQuery, Result<List<ProductSectionRowDTO>>>
{
    public async ValueTask<Result<List<ProductSectionRowDTO>>> Handle(
        GetProductSectionsQuery query, CancellationToken cancellationToken)
    {
        var sections = await repository.ListAsync(cancellationToken);

        return Result.Success(sections
            .OrderBy(x => x.Type).ThenBy(x => x.SortOrder).ThenBy(x => x.Id.Value)
            .Select(x => new ProductSectionRowDTO(x.Id.Value, ProductSectionTypes.Name(x.Type), x.Name, x.IsActive,
                x.Comment, x.SortOrder))
            .ToList());
    }
}
