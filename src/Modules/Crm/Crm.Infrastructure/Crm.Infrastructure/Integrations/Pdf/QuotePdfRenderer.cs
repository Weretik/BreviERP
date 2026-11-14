using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Crm.Application.Features.Quotes.Shared.Pdf;
using Crm.Application.Features.Quotes.Shared.Pdf.Introduction;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Infrastructure.Integrations.Pdf;

/// <summary>Draws <see cref="QuotePdfDocument"/> and <see cref="QuoteIntroPdfDocument"/> with QuestPDF (Community license).</summary>
internal sealed class QuotePdfRenderer(QuotePdfImageLoader images) : IQuotePdfRenderer
{
    static QuotePdfRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseEnvironmentFonts = false;
    }

    public async Task<byte[]> RenderAsync(QuotePdfModel model, CancellationToken cancellationToken)
    {
        QuotePdfFonts.EnsureRegistered();
        var loaded = await images.LoadAsync(
            model.Items.Select(x => x.PhotoUrl).Append(model.Company.LogoUrl), cancellationToken);
        return new QuotePdfDocument(model, loaded).GeneratePdf();
    }

    public async Task<byte[]> RenderIntroductionAsync(QuoteIntroPdfModel model, CancellationToken cancellationToken)
    {
        QuotePdfFonts.EnsureRegistered();
        var urls = model.Pages.SelectMany(page => page.Models.Select(x => x.PhotoUrl).Append(page.BannerUrl))
            .Append(model.Company.LogoUrl);
        var loaded = await images.LoadAsync(urls, cancellationToken);
        return new QuoteIntroPdfDocument(model, loaded).GeneratePdf();
    }
}
