using Reference.Application.Contracts.Persistence;
using FabricColorEntity = Reference.Domain.FabricColors.Entities.FabricColor;

namespace Reference.Application.Features.FabricColor.GetList;

public sealed class GetFabricColorsQueryHandler(IReferenceReadRepository<FabricColorEntity> repository)
    : IQueryHandler<GetFabricColorsQuery, Result<List<FabricColorRowDTO>>>
{
    public async ValueTask<Result<List<FabricColorRowDTO>>> Handle(
        GetFabricColorsQuery query, CancellationToken cancellationToken)
    {
        var colors = await repository.ListAsync(cancellationToken);

        return Result.Success(colors
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)
            .Select(x => new FabricColorRowDTO(x.Id.Value, x.Name, x.IsActive, x.SortOrder))
            .ToList());
    }
}
