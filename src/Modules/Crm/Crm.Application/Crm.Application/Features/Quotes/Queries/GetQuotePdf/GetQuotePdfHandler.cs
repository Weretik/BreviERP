using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Pdf.Composing;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Queries.GetQuotePdf;

/// <summary>
/// Renders the saved КП with the current settings; the responsible manager's КП contact goes into it,
/// or their account name, phone and email when no contact is set.
/// </summary>
public sealed class GetQuotePdfHandler(IQuoteStore store, QuotePdfComposer composer)
    : IQueryHandler<GetQuotePdfQuery, Result<QuotePdfFile>>
{
    public async ValueTask<Result<QuotePdfFile>> Handle(GetQuotePdfQuery query, CancellationToken cancellationToken)
    {
        var quote = await store.Quotes.AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Options).ThenInclude(x => x.Fabrics)
            .Include(x => x.Items).ThenInclude(x => x.Lines)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (quote is null || !query.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);

        var context = await composer.LoadContextAsync(cancellationToken);
        return Result.Success(await composer.ComposeAsync(quote, context, cancellationToken));
    }
}
