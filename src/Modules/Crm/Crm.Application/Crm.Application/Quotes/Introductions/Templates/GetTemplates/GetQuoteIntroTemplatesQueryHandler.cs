using Crm.Application.Quotes.Introductions.Contracts;
using Crm.Application.Quotes.Persistence;

namespace Crm.Application.Quotes.Introductions.Templates.GetTemplates;

public sealed class GetQuoteIntroTemplatesQueryHandler(IQuoteStore store)
    : IQueryHandler<GetQuoteIntroTemplatesQuery, Result<IReadOnlyList<QuoteIntroTemplateResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<QuoteIntroTemplateResponse>>> Handle(GetQuoteIntroTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        var templates = await store.QuoteIntroTemplates.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<QuoteIntroTemplateResponse>>(
            templates.Select(x => x.ToResponse(query.Actor)).ToArray());
    }
}
