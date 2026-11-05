using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Pdf.GetPdf;
using Crm.Application.Quotes.Pdf.Introduction;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Settings.Contracts;
using Crm.Application.Quotes.Settings.Reading;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Quotes.Pdf.Composing;

/// <summary>Builds the PDF of a saved КП (R-14) or the catalogue of an introduction (R-16); used by «Сформувати КП» and by sending (R-15).</summary>
public sealed class QuotePdfComposer(IQuoteStore store, IQuoteStaffReader staff, IQuotePdfRenderer renderer,
    IQuoteCatalogReader catalog)
{
    public async Task<QuoteDocumentContext> LoadContextAsync(CancellationToken cancellationToken)
    {
        var saved = await store.FindSettingsAsync(tracking: false, cancellationToken);
        return new QuoteDocumentContext(saved?.Content ?? QuoteSettingsDefaults.Content, saved);
    }

    /// <summary>The staff member, or the name saved in the КП when the account is gone.</summary>
    public async Task<QuoteStaffMember> MemberAsync(int userId, string fallbackName, CancellationToken cancellationToken) =>
        await staff.FindAsync(userId, cancellationToken) ?? new QuoteStaffMember(userId, fallbackName, null);

    public async Task<QuotePdfFile> ComposeAsync(Quote quote, QuoteDocumentContext context,
        CancellationToken cancellationToken)
    {
        var responsible = await MemberAsync(quote.ResponsibleUserId, quote.ResponsibleName, cancellationToken);
        var saved = context.Saved?.ContactOf(responsible.UserId);
        var manager = QuoteSettingsMapper.ContactOrAccount(responsible, saved);
        var photos = await QuoteItemPhotos.CatalogPhotosAsync(quote, catalog, cancellationToken);
        var content = quote.Kind == QuoteKind.Introduction
            ? await renderer.RenderIntroductionAsync(
                QuoteIntroPdfModelBuilder.Build(quote, context.Settings, manager, photos), cancellationToken)
            : await renderer.RenderAsync(QuotePdfModelBuilder.Build(quote, context.Settings, manager, photos),
                cancellationToken);
        return new QuotePdfFile(content, FileName(quote));
    }

    public static string FileName(Quote quote) => $"КП {quote.Number}.pdf";
}
