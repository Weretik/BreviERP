using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Settings.Contracts;
using Crm.Application.Quotes.Settings.Reading;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Quotes.Pdf.GetPdf;

/// <summary>
/// Renders the saved КП with the current settings; the responsible manager's КП contact goes into it,
/// or their account name, phone and email when no contact is set.
/// </summary>
public sealed class GetQuotePdfQueryHandler(IQuoteStore store, IQuoteStaffReader staff, IQuotePdfRenderer renderer)
    : IQueryHandler<GetQuotePdfQuery, Result<QuotePdfFile>>
{
    public async ValueTask<Result<QuotePdfFile>> Handle(GetQuotePdfQuery query, CancellationToken cancellationToken)
    {
        var quote = await store.Quotes.AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Options)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (quote is null || !query.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);

        var saved = await store.FindSettingsAsync(tracking: false, cancellationToken);
        var member = await staff.FindAsync(quote.ResponsibleUserId, cancellationToken)
            ?? new QuoteStaffMember(quote.ResponsibleUserId, quote.ResponsibleName, null);
        var manager = QuoteSettingsMapper.ContactOrAccount(member, saved?.ContactOf(member.UserId));

        var model = QuotePdfModelBuilder.Build(quote, saved?.Content ?? QuoteSettingsDefaults.Content, manager);
        var content = await renderer.RenderAsync(model, cancellationToken);
        return Result.Success(new QuotePdfFile(content, $"КП {quote.Number}.pdf"));
    }
}
