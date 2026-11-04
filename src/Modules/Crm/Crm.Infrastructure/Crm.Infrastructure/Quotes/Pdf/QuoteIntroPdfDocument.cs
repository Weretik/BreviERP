using Crm.Application.Quotes.Pdf.Introduction;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Crm.Infrastructure.Quotes.Pdf;

/// <summary>
/// «КП для знайомства» (R-16): one A4 page per season with its banner, title and 4 model cards (the main model first,
/// framed), then the advantages, the next step and the manager's contact. Requisites are in the footer.
/// </summary>
internal sealed class QuoteIntroPdfDocument(QuoteIntroPdfModel model, IReadOnlyDictionary<string, QuotePdfImage> images)
    : IDocument
{
    private const string Ink = QuotePdfParts.Ink;
    private const string Muted = QuotePdfParts.Muted;
    private const string Line = QuotePdfParts.Line;
    private const string Soft = "#F7F7F8";

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Добірка спецодягу № {model.Number}",
        Author = model.Company.Name,
        Creator = "Brevi",
    };

    public void Compose(IDocumentContainer container) =>
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(28);
            page.DefaultTextStyle(x => x.FontFamily(QuotePdfFonts.Family).FontSize(9.5f).FontColor(Ink));
            page.Header().Element(Header);
            page.Content().PaddingVertical(10).Element(Content);
            page.Footer().Element(x => QuotePdfParts.Footer(x, model.Company));
        });

    private void Header(IContainer container) =>
        container.BorderBottom(2).BorderColor(model.Accent).PaddingBottom(6).Row(row =>
        {
            row.RelativeItem().AlignMiddle().Element(Logo);
            row.RelativeItem().AlignRight().AlignMiddle().Column(column =>
            {
                if (model.LogoText is { } logoText)
                    foreach (var line in logoText.Split('\n'))
                        column.Item().AlignRight().Text(line).FontSize(8.5f).FontColor(Muted);
                var client = string.Join(" · ", new[] { model.ClientCompanyName, model.RecipientName }.Where(x => x is not null));
                if (client.Length > 0) column.Item().AlignRight().Text($"Для: {client}").FontSize(8.5f).SemiBold();
            });
        });

    private void Logo(IContainer container)
    {
        if (model.Company.LogoUrl is { } url && images.TryGetValue(url, out var logo))
        {
            container.MaxHeight(38).MaxWidth(160).Element(x => QuotePdfParts.Picture(x, logo));
            return;
        }

        container.Text(model.Company.Name).FontSize(16).Bold().FontColor(model.Accent);
    }

    private void Content(IContainer container) =>
        container.Column(column =>
        {
            for (var index = 0; index < model.Pages.Count; index++)
            {
                var page = model.Pages[index];
                if (index > 0) column.Item().PageBreak();
                column.Item().Element(x => Page(x, page));
            }

            column.Item().PageBreak();
            column.Item().Element(Closing);
        });

    private void Page(IContainer container, QuoteIntroPdfPage page) =>
        container.Column(column =>
        {
            column.Spacing(10);
            if (page.BannerUrl is { } url && images.TryGetValue(url, out var banner))
                column.Item().Height(130).Element(x => QuotePdfParts.Picture(x, banner));
            column.Item().Text(page.Title).FontSize(20).Bold().FontColor(model.Accent);
            if (page.Subtitle is { } subtitle) column.Item().Text(subtitle).FontColor(Muted);
            if (page.ModelsTitle is { } modelsTitle)
                column.Item().PaddingTop(4).Text(modelsTitle).FontSize(11).Bold().LetterSpacing(0.05f);
            foreach (var pair in page.Models.Chunk(2))
                column.Item().Row(row =>
                {
                    row.Spacing(10);
                    foreach (var card in pair) row.RelativeItem().Element(x => Card(x, card));
                    if (pair.Length == 1) row.RelativeItem();
                });
            if (model.AssortmentUrl is { } assortment)
                column.Item().AlignCenter().Hyperlink(assortment)
                    .Text(model.AssortmentLinkText ?? QuotePdfParts.Display(assortment))
                    .Bold().FontColor(model.Accent);
        });

    private void Card(IContainer container, QuoteIntroPdfCard card) =>
        container.Border(card.IsMain ? 1.5f : 1).BorderColor(card.IsMain ? model.Accent : Line).Padding(8).Column(column =>
        {
            column.Spacing(3);
            column.Item().Height(150).Background(Soft).Element(box =>
            {
                if (card.PhotoUrl is { } url && images.TryGetValue(url, out var photo))
                    QuotePdfParts.Picture(box.Padding(4), photo);
            });
            column.Item().PaddingTop(4).Text(card.Name).FontSize(11).Bold();
            if (card.Fabric is { } fabric) column.Item().Text($"Тканина: {fabric}").FontSize(8.5f).FontColor(Muted);
            if (card.Price is { } price)
                column.Item().Text(text =>
                {
                    text.Span("від 41 шт.  ").FontColor(Muted).FontSize(8.5f);
                    text.Span(QuotePdfParts.Money(price)).Bold().FontColor(model.Accent);
                });
            if (card.SiteUrl is { } site)
                column.Item().PaddingTop(3).AlignLeft().Hyperlink(site).Background(model.Accent)
                    .PaddingVertical(3).PaddingHorizontal(10).Text(model.ProductButtonText)
                    .FontColor(Colors.White).FontSize(8.5f).SemiBold();
        });

    private void Closing(IContainer container) =>
        container.Column(column =>
        {
            column.Spacing(12);
            if (model.Advantages.Count > 0)
            {
                column.Item().Text("Чому Brevi").FontSize(14).Bold().FontColor(model.Accent);
                column.Item().Row(row =>
                {
                    row.Spacing(6);
                    foreach (var advantage in model.Advantages)
                        row.RelativeItem().Background(Soft).Padding(8).Column(box =>
                        {
                            box.Item().Text(advantage.Title).SemiBold();
                            if (advantage.Text is { } text) box.Item().Text(text).FontSize(8).FontColor(Muted);
                        });
                });
            }

            if (model.VatNotice is { } vat) column.Item().Text(vat).Italic().FontColor(Muted);
            column.Item().ShowEntire().Border(1).BorderColor(model.Accent).Padding(12).Column(box =>
            {
                box.Spacing(4);
                if (model.NextStepText is { } next) box.Item().Text(next).FontSize(11).Bold();
                box.Item().PaddingTop(2).Text(model.Manager.Name).SemiBold();
                foreach (var line in new[] { model.Manager.Phone, model.Manager.Email, model.Company.Site })
                    if (line is not null) box.Item().Text(line);
            });
        });
}
