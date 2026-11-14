using FabricColorEntity = Reference.Domain.FabricColors.FabricColor;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.FabricColor.Queries.GetFabricColors;

public sealed class GetFabricColorsHandler(IReferenceReadRepository<FabricColorEntity> repository)
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
