using Mediator;
using Reference.Application.Features.FabricColor.GetList;
using Reference.Contracts.FabricColors;

namespace Reference.Infrastructure.Contracts.FabricColors;

internal sealed class FabricColorDirectory(ISender sender) : IFabricColorDirectory
{
    public async Task<IReadOnlyList<FabricColorInfo>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFabricColorsQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference colours could not be read.");
        return result.Value.Select(x => new FabricColorInfo(x.Id, x.Name, x.IsActive)).ToArray();
    }
}
