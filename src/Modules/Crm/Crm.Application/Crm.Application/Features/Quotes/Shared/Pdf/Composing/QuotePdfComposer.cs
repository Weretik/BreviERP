using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;
using Crm.Application.Features.Quotes.Queries.GetQuotePdf;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Pdf;
using Crm.Application.Features.Quotes.Shared.Pdf.Introduction;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Features.Quotes.Shared.Settings;
using Crm.Application.Features.Quotes.Shared.Settings.Reading;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Application.Abstractions.Quotes.Products;
using Crm.Application.Abstractions.Quotes.Sending;

namespace Crm.Application.Features.Quotes.Shared.Pdf.Composing;

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
