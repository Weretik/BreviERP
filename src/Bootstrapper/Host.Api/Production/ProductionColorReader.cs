using Mediator;
using Production.Application.Abstractions;
using Reference.Application.Features.FabricColor.GetList;

namespace Host.Api.Production;

/// <summary>Active colours of «Кольори» for the workshop forms and their check.</summary>
internal sealed class ProductionColorReader(ISender sender) : IProductionColorReader
{
    public async Task<IReadOnlyList<string>> ListActiveAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFabricColorsQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference colours could not be read.");
        return result.Value.Where(x => x.IsActive).Select(x => x.Name).ToArray();
    }
}
