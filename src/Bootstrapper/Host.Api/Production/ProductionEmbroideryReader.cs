using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.GetCalculation;
using Crm.Application.Embroidery.GetCalculations;
using Mediator;
using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Host.Api.Production;

/// <summary>Saved «Ціна вишивки» calculations of Crm for the workshop, read without costs.</summary>
internal sealed class ProductionEmbroideryReader(ISender sender) : IProductionEmbroideryReader
{
    private const int MaxChoices = 50;

    public async Task<IReadOnlyList<ProductionEmbroideryChoiceResponse>> SearchAsync(int userId, string? search,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEmbroideryCalculationsQuery(new EmbroideryActor(userId, false), search,
            1, MaxChoices), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Saved embroidery calculations could not be read.");
        return result.Value.Items.Select(x => new ProductionEmbroideryChoiceResponse(x.Id, x.Number, x.Name,
            x.CustomerName, x.Quantity, x.StitchesPerUnit, x.HoopLabel, Thumbnail(x.Mockup))).ToArray();
    }

    public async Task<IReadOnlyDictionary<long, ProductionEmbroideryResponse>> ReadAsync(int userId,
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken)
    {
        var found = new Dictionary<long, ProductionEmbroideryResponse>();
        foreach (var id in ids.Distinct())
        {
            var result = await sender.Send(new GetEmbroideryCalculationQuery(new EmbroideryActor(userId, false), id),
                cancellationToken);
            if (!result.IsSuccess) continue;
            var summary = result.Value.Summary;
            var calculation = result.Value.Result;
            found[id] = new ProductionEmbroideryResponse(summary.Id, summary.Number, summary.Name,
                summary.CustomerName, summary.Mode, summary.Quantity, summary.StitchesPerUnit, summary.HoopLabel,
                calculation.Hoopings, calculation.Stabilizer?.CutMetres, calculation.Fabric?.CutMetres,
                summary.Mockup?.Url, Thumbnail(summary.Mockup));
        }

        return found;
    }

    private static string? Thumbnail(EmbroideryMockupResponse? mockup) => mockup?.ThumbnailUrl ?? mockup?.Url;
}
