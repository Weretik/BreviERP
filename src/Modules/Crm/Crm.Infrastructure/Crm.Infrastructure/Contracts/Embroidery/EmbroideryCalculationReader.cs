using Crm.Contracts.Embroidery;
using Mediator;
using MockupUsagesQuery = Crm.Application.Features.Embroidery.Queries.GetEmbroideryMockupUsages.GetEmbroideryMockupUsagesQuery;
using Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculationsById;
using Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculations;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;

namespace Crm.Infrastructure.Contracts.Embroidery;

/// <summary>Saved «Ціна вишивки» calculations for other modules, without costs.</summary>
internal sealed class EmbroideryCalculationReader(ISender sender) : IEmbroideryCalculationReader
{
    public async Task<IReadOnlyList<EmbroideryChoiceInfo>> SearchAsync(int userId, string? search, int max,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEmbroideryCalculationsQuery(new EmbroideryActor(userId, false), search,
            1, max), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Saved embroidery calculations could not be read.");
        return result.Value.Items.Select(x => new EmbroideryChoiceInfo(x.Id, x.Number, x.Name, x.CustomerName,
            x.Quantity, x.StitchesPerUnit, x.HoopLabel, Thumbnail(x.Mockup))).ToArray();
    }

    public async Task<IReadOnlyList<EmbroideryCalculationInfo>> FindManyAsync(int userId,
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEmbroideryCalculationsByIdQuery(new EmbroideryActor(userId, false), ids),
            cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Saved embroidery calculations could not be read.");
        return result.Value.Select(x => new EmbroideryCalculationInfo(x.Summary.Id, x.Summary.Number, x.Summary.Name,
            x.Summary.CustomerName, x.Summary.Mode, x.Summary.Quantity, x.Summary.StitchesPerUnit, x.Summary.HoopLabel,
            x.Result.Hoopings, x.Result.Stabilizer?.CutMetres, x.Result.Fabric?.CutMetres, x.Summary.Mockup?.Url,
            Thumbnail(x.Summary.Mockup))).ToArray();
    }

    public async Task<IReadOnlyList<EmbroideryMockupUsage>> MockupUsagesAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MockupUsagesQuery(mediaFileIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Embroidery mockup usages could not be read.");
        return result.Value.Select(x => new EmbroideryMockupUsage(x.MediaFileId, x.CalculationId, x.Title)).ToArray();
    }

    private static string? Thumbnail(EmbroideryMockupResponse? mockup) => mockup?.ThumbnailUrl ?? mockup?.Url;
}
