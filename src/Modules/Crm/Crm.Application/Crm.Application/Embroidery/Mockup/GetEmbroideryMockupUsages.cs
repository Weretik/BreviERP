using Crm.Application.Embroidery.Persistence;
using Crm.Domain.Embroidery.Saved;

namespace Crm.Application.Embroidery.Mockup;

/// <summary>Saved calculations whose «Макет» is one of the photos, so the Media library does not delete it.</summary>
public sealed record GetEmbroideryMockupUsagesQuery(IReadOnlyCollection<int> MediaFileIds)
    : IQuery<Result<IReadOnlyList<EmbroideryMockupUsage>>>;

/// <param name="Title">«ВШ-0042 Логотип».</param>
public sealed record EmbroideryMockupUsage(int MediaFileId, long CalculationId, string Title);

public sealed class GetEmbroideryMockupUsagesQueryHandler(IEmbroideryStore store)
    : IQueryHandler<GetEmbroideryMockupUsagesQuery, Result<IReadOnlyList<EmbroideryMockupUsage>>>
{
    public async ValueTask<Result<IReadOnlyList<EmbroideryMockupUsage>>> Handle(GetEmbroideryMockupUsagesQuery query,
        CancellationToken cancellationToken)
    {
        var ids = query.MediaFileIds.Distinct().ToArray();
        var rows = await store.EmbroideryCalculations.AsNoTracking()
            .Where(x => x.MockupMediaFileId != null && ids.Contains(x.MockupMediaFileId.Value))
            .Select(x => new { x.Id, x.Name, FileId = x.MockupMediaFileId!.Value })
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<EmbroideryMockupUsage>>(rows.Select(x => new EmbroideryMockupUsage(x.FileId,
            x.Id, (EmbroiderySavedCalculation.NumberOf(x.Id) + " " + x.Name).Trim())).ToArray());
    }
}

public sealed class GetEmbroideryMockupUsagesQueryValidator : AbstractValidator<GetEmbroideryMockupUsagesQuery>
{
    public GetEmbroideryMockupUsagesQueryValidator() => RuleFor(x => x.MediaFileIds).NotNull();
}
