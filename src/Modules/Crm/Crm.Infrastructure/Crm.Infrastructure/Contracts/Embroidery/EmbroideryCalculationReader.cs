using Crm.Contracts.Embroidery;
using Mediator;
using MockupUsagesQuery = Crm.Application.Features.Embroidery.Queries.GetEmbroideryMockupUsages.GetEmbroideryMockupUsagesQuery;
using Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculation;
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

    public async Task<EmbroideryCalculationInfo?> FindAsync(int userId, long id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEmbroideryCalculationQuery(new EmbroideryActor(userId, false), id),
            cancellationToken);
        if (!result.IsSuccess) return null;
        var summary = result.Value.Summary;
        var calculation = result.Value.Result;
        return new EmbroideryCalculationInfo(summary.Id, summary.Number, summary.Name, summary.CustomerName,
            summary.Mode, summary.Quantity, summary.StitchesPerUnit, summary.HoopLabel, calculation.Hoopings,
            calculation.Stabilizer?.CutMetres, calculation.Fabric?.CutMetres, summary.Mockup?.Url,
            Thumbnail(summary.Mockup));
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
