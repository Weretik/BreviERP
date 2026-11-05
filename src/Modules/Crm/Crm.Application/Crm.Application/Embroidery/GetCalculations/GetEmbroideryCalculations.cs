using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Mockup;
using Crm.Application.Embroidery.Persistence;
using Crm.Domain.Embroidery.Saved;

namespace Crm.Application.Embroidery.GetCalculations;

/// <summary>«Збережені»: newest first; the search matches the name, the client or the number «ВШ-0042».</summary>
public sealed record GetEmbroideryCalculationsQuery(EmbroideryActor Actor, string? Search, int Page = 1,
    int PageSize = 25) : IQuery<Result<EmbroiderySavedPageResponse>>;

public sealed class GetEmbroideryCalculationsQueryHandler(IEmbroideryStore store, IEmbroideryMediaReader media)
    : IQueryHandler<GetEmbroideryCalculationsQuery, Result<EmbroiderySavedPageResponse>>
{
    public async ValueTask<Result<EmbroiderySavedPageResponse>> Handle(GetEmbroideryCalculationsQuery query,
        CancellationToken cancellationToken)
    {
        var rows = store.EmbroideryCalculations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            var number = EmbroideryNumberSearch.Parse(search);
            rows = rows.Where(x => x.Id == number || x.Name.ToLower().Contains(search) ||
                (x.CustomerName != null && x.CustomerName.ToLower().Contains(search)));
        }

        var total = await rows.CountAsync(cancellationToken);
        var page = await rows.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        var mockups = await media.ReadForAsync(page.Select(x => x.MockupMediaFileId), cancellationToken);
        return Result.Success(new EmbroiderySavedPageResponse(
            [.. page.Select(x => x.ToSummary(mockups.Of(x.MockupMediaFileId)))], query.Page,
            query.PageSize, total));
    }
}

public sealed class GetEmbroideryCalculationsQueryValidator : AbstractValidator<GetEmbroideryCalculationsQuery>
{
    public const int MaxPageSize = 100;

    public GetEmbroideryCalculationsQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Search).MaximumLength(EmbroiderySavedCalculation.NameMaxLength);
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}

internal static class EmbroideryNumberSearch
{
    /// <summary>«ВШ-0042», «вш42» or «42» → 42; anything else → -1 (matches no record).</summary>
    public static long Parse(string search)
    {
        var digits = search.StartsWith("вш", StringComparison.Ordinal) ? search[2..].TrimStart('-', ' ') : search;
        return digits.Length is > 0 and <= 12 && digits.All(char.IsAsciiDigit) ? long.Parse(digits) : -1;
    }
}
