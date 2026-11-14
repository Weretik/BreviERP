using Crm.Application.Features.Quotes.Shared.Introductions;
using Crm.Application.Features.Quotes.Shared.Introductions.Templates;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteIntroTemplates;

public sealed class GetQuoteIntroTemplatesHandler(IQuoteStore store)
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
