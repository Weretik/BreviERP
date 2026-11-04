using System.Globalization;
using Crm.Application.Quotes.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Crm.Infrastructure.Quotes.Pdf;

/// <summary>
/// The КП page: company header, title and client, one block per item with its photo and the shown prices of
/// each variant, client comment, VAT note, call-to-action with the manager's contact, requisites in the footer.
/// </summary>
internal sealed class QuotePdfDocument(QuotePdfModel model, IReadOnlyDictionary<string, QuotePdfImage> images)
    : IDocument
{
    private const string Accent = "#E8780F";
    private const string AccentSoft = "#FDF1E6";
    private const string Ink = "#1F2328";
    private const string Muted = "#6B7280";
    private const string Line = "#E5E7EB";
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"{model.Title} № {model.Number}",
        Author = model.Company.Name,
        Creator = "Brevi",
    };

    public void Compose(IDocumentContainer container) =>
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32);
            page.DefaultTextStyle(x => x.FontFamily(QuotePdfFonts.Family).FontSize(9.5f).FontColor(Ink));
            page.Header().Element(Header);
            page.Content().PaddingVertical(12).Element(Content);
            page.Footer().Element(Footer);
        });

    private void Header(IContainer container) =>
        container.BorderBottom(2).BorderColor(Accent).PaddingBottom(8).Row(row =>
        {
            row.RelativeItem().AlignMiddle().Element(Logo);
            row.RelativeItem().AlignRight().Column(column =>
            {
                column.Item().AlignRight().Text(model.Company.Name).Bold();
                foreach (var line in new[] { model.Company.Address, model.Company.Phone, model.Company.Email })
                    if (line is not null) column.Item().AlignRight().Text(line).FontColor(Muted).FontSize(8.5f);
                if (model.Company.Site is { } site)
                    column.Item().AlignRight().Hyperlink(site).Text(Display(site)).FontColor(Accent).FontSize(8.5f);
            });
        });

    private void Logo(IContainer container)
    {
        if (model.Company.LogoUrl is { } url && images.TryGetValue(url, out var logo))
        {
            var box = container.MaxHeight(42).MaxWidth(170);
            if (logo.Svg is not null) box.Svg(logo.Svg).FitArea();
            else box.Image(logo.Bitmap!).FitArea();
            return;
        }

        container.Text(model.Company.Name).FontSize(16).Bold().FontColor(Accent);
    }

    private void Content(IContainer container) =>
        container.Column(column =>
        {
            column.Spacing(12);
            column.Item().Element(Title);
            foreach (var item in model.Items) column.Item().Element(x => Item(x, item));
            if (model.ClientComment is { } comment)
                column.Item().Background(AccentSoft).Padding(10).Column(box =>
                {
                    box.Item().Text("Коментар").Bold();
                    box.Item().Text(comment);
                });
            if (model.VatNotice is { } vat) column.Item().Text(vat).Italic().FontColor(Muted);
            column.Item().Element(CallToAction);
        });

    private void Title(IContainer container) =>
        container.Column(column =>
        {
            column.Item().Text(model.Title).FontSize(18).Bold().FontColor(Accent);
            column.Item().Text($"№ {model.Number} від {model.Date.ToString("dd.MM.yyyy", Uk)}").FontColor(Muted);
            var client = string.Join(" · ", new[] { model.ClientCompanyName, model.RecipientName }.Where(x => x is not null));
            if (client.Length > 0)
                column.Item().PaddingTop(4).Text(text =>
                {
                    text.Span("Для: ").FontColor(Muted);
                    text.Span(client).Bold();
                });
        });

    private void Item(IContainer container, QuotePdfItem item) =>
        container.EnsureSpace(160).Border(1).BorderColor(Line).Padding(10).Column(column =>
        {
            column.Spacing(8);
            column.Item().Row(row =>
            {
                if (item.PhotoUrl is { } url && images.TryGetValue(url, out var photo))
                    row.ConstantItem(96).PaddingRight(12).Height(110).Element(x => Photo(x, photo));
                row.RelativeItem().Column(text =>
                {
                    text.Spacing(2);
                    text.Item().Text(item.Name).FontSize(12).Bold();
                    var line = string.Join(" · ", new[] { item.Subtitle, item.Season, item.Code }.Where(x => x is not null));
                    if (line.Length > 0) text.Item().Text(line).FontColor(Muted).FontSize(8.5f);
                    if (item.Description is { } description) text.Item().PaddingTop(4).Text(description);
                    if (item.SiteUrl is { } site)
                        text.Item().PaddingTop(2).Hyperlink(site).Text("Детальніше на сайті").FontColor(Accent).FontSize(8.5f);
                });
            });
            column.Item().Element(x => Prices(x, item));
        });

    private static void Photo(IContainer container, QuotePdfImage photo)
    {
        if (photo.Svg is not null) container.Svg(photo.Svg).FitArea();
        else container.AlignCenter().AlignMiddle().Image(photo.Bitmap!).FitArea();
    }

    private void Prices(IContainer container, QuotePdfItem item)
    {
        var tiers = new List<(string Label, Func<QuotePdfOption, decimal?> Price)>();
        if (model.Tiers.Retail) tiers.Add(("до 9 шт.", x => x.Retail));
        if (model.Tiers.Wholesale) tiers.Add(("10–40 шт.", x => x.Wholesale));
        if (model.Tiers.Large) tiers.Add(("від 41 шт.", x => x.Large));

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                foreach (var _ in tiers) columns.RelativeColumn(1.2f);
            });
            table.Header(header =>
            {
                header.Cell().Element(HeadCell).Text(item.OptionsLabel);
                foreach (var tier in tiers) header.Cell().Element(HeadCell).AlignRight().Text(tier.Label);
            });
            foreach (var option in item.Options)
            {
                table.Cell().Element(BodyCell).Column(cell =>
                {
                    cell.Item().Text(text =>
                    {
                        text.Span(option.Title).SemiBold();
                        if (option.IsRecommended) text.Span("  · рекомендовано").FontColor(Accent).FontSize(8);
                    });
                    if (option.Details is { } details) cell.Item().Text(details).FontColor(Muted).FontSize(8);
                });
                foreach (var tier in tiers)
                    table.Cell().Element(BodyCell).AlignRight().Text(Money(tier.Price(option))).SemiBold();
            }
        });
    }

    private void CallToAction(IContainer container)
    {
        if (model.CtaTitle is null && model.CtaText is null && model.CtaContact is null) return;
        container.ShowEntire().Border(1).BorderColor(Accent).Background(AccentSoft).Padding(12).Column(column =>
        {
            column.Spacing(4);
            if (model.CtaTitle is { } title) column.Item().Text(title).FontSize(11).Bold();
            if (model.CtaText is { } text) column.Item().Text(text);
            if (model.CtaContact is { } contact) column.Item().PaddingTop(4).Text(contact).SemiBold();
        });
    }

    private void Footer(IContainer container) =>
        container.BorderTop(1).BorderColor(Line).PaddingTop(6).Row(row =>
        {
            var requisites = string.Join(" · ", new[]
            {
                model.Company.Name,
                model.Company.Edrpou is { } edrpou ? $"ЄДРПОУ {edrpou}" : null,
                model.Company.TaxId is { } tax ? $"ІПН {tax}" : null,
                model.Company.Iban is { } iban ? $"IBAN {iban}" : null,
            }.Where(x => x is not null));
            row.RelativeItem().Text(requisites).FontSize(7.5f).FontColor(Muted);
            row.ConstantItem(70).AlignRight().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(7.5f).FontColor(Muted));
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });

    private static IContainer HeadCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Line).PaddingVertical(4).PaddingHorizontal(4)
            .DefaultTextStyle(x => x.FontSize(8).FontColor(Muted));

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Line).PaddingVertical(5).PaddingHorizontal(4);

    internal static string Money(decimal? value) =>
        value is { } amount ? amount.ToString("#,##0.00", Uk) + " ₴" : "—";

    private static string Display(string url) =>
        url.Replace("https://", "", StringComparison.Ordinal).Replace("http://", "", StringComparison.Ordinal).TrimEnd('/');
}
