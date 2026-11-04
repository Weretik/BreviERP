using Crm.Application.Quotes.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Crm.Infrastructure.Quotes.Pdf;

/// <summary>Draws <see cref="QuotePdfDocument"/> with QuestPDF (Community license).</summary>
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
}
