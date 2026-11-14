using Reference.Domain.FabricColors;
using FabricColorEntity = Reference.Domain.FabricColors.FabricColor;

namespace Reference.Application.Features.FabricColor.Shared;

public sealed class FabricColorByIdSpec : Specification<FabricColorEntity>
{
    public FabricColorByIdSpec(int id) => Query.Where(x => x.Id == FabricColorId.From(id));
}
