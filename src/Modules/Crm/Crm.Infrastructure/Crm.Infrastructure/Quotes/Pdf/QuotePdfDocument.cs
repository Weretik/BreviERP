using Crm.Application.Quotes.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Crm.Infrastructure.Quotes.Pdf;

/// <summary>
/// The КП page «Класика+» (R-30): the logo, number and company requisites in the header of the first page and a
/// short header after it; the client and the opening text; numbered items one after another with the photo, the
/// description and only the prices the client may see; the client comment, the VAT note and the dark call to
/// action; the responsible manager's contacts in the dark footer of every page.
/// </summary>
internal sealed class QuotePdfDocument(QuotePdfModel model, IReadOnlyDictionary<string, QuotePdfImage> images)
    : IDocument
{
    private const float Side = 36;
    private const string Accent = "#FD9600";
    private const string AccentText = "#9A5A00";
    private const string AccentOnDark = "#FFB547";
    private const string Graphite = "#262626";
    private const string Paper = "#F6F4F1";
    private const string PhotoBack = "#F7F5F2";
    private const string Soft = "#FFF6E8";
    private const string Ink = QuotePdfParts.Ink;
    private const string Muted = "#555555";
    private const string Line = "#E2DED8";

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
            page.Margin(0);
            page.DefaultTextStyle(x => x.FontFamily(QuotePdfFonts.Family).FontSize(9.5f).FontColor(Ink).LineHeight(1.35f));
            page.Header().Column(column =>
            {
                column.Item().ShowOnce().Element(FirstHeader);
                column.Item().SkipOnce().Element(NextHeader);
            });
            page.Content().PaddingHorizontal(Side).PaddingTop(14).PaddingBottom(18).Element(Content);
            page.Footer().Element(Footer);
        });

    private void FirstHeader(IContainer container) =>
        container.PaddingHorizontal(Side).PaddingTop(28).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().AlignMiddle().Element(x => Logo(x, 56, 170));
                row.RelativeItem().AlignRight().AlignMiddle().Column(title =>
                {
                    title.Item().AlignRight().Text(model.Title).FontSize(13).Bold().LetterSpacing(0.04f);
                    title.Item().AlignRight().Text(text =>
                    {
                        text.Span("№ ");
                        text.Span(model.Number).Bold();
                        text.Span($" · від {model.Date.ToString("dd.MM.yyyy", QuotePdfParts.Uk)}");
                    });
                });
            });
            column.Item().PaddingTop(10).LineHorizontal(2.5f).LineColor(Accent);
            column.Item().PaddingTop(6).Element(Requisites);
        });

    private void Requisites(IContainer container)
    {
        var company = model.Company;
        var first = Join(company.Name, Prefixed("ЄДРПОУ", company.Edrpou), Prefixed("ІПН", company.TaxId),
            Prefixed("IBAN", company.Iban));
        var second = Join(company.Address, company.Site is { } site ? QuotePdfParts.Display(site) : null,
            company.Email);
        container.DefaultTextStyle(x => x.FontSize(8).FontColor("#444444")).Column(column =>
        {
            if (first.Length > 0) column.Item().Text(first);
            if (second.Length > 0) column.Item().Text(second);
        });
    }

    private void NextHeader(IContainer container) =>
        container.PaddingHorizontal(Side).PaddingTop(20).Column(column =>
        {
            column.Item().PaddingBottom(8).Row(row =>
            {
                row.RelativeItem().AlignMiddle().Element(x => Logo(x, 28, 90));
                var client = model.ClientCompanyName ?? model.RecipientName;
                row.RelativeItem().AlignRight().AlignMiddle()
                    .Text(Join($"КП № {model.Number}", client)).FontSize(8.5f).FontColor(Muted);
            });
            column.Item().LineHorizontal(1).LineColor(Line);
        });

    private void Logo(IContainer container, float height, float width)
    {
        if (model.Company.LogoUrl is { } url && images.TryGetValue(url, out var logo))
        {
            var box = container.MaxHeight(height).MaxWidth(width);
            if (logo.Svg is not null) box.Svg(logo.Svg).FitArea();
            else box.Image(logo.Bitmap!).FitArea();
            return;
        }

        container.Text(model.Company.Name).FontSize(height > 40 ? 16 : 11).Bold().FontColor(AccentText);
    }

    private void Content(IContainer container) =>
        container.Column(column =>
        {
            column.Spacing(14);
            column.Item().Element(Client);
            for (var index = 0; index < model.Items.Count; index++)
            {
                var number = (index + 1).ToString("00", QuotePdfParts.Uk);
                var item = model.Items[index];
                column.Item().Element(x => Item(x, number, item));
            }
            if (model.ClientComment is { } comment)
                column.Item().Background(Paper).Padding(12).Column(box =>
                {
                    box.Item().Text("Коментар").Bold();
                    box.Item().Text(comment);
                });
            column.Item().Element(CallToAction);
        });

    private void Client(IContainer container)
    {
        var client = model.ClientCompanyName ?? model.RecipientName;
        var person = model.ClientCompanyName is not null ? model.RecipientName : null;
        container.Row(row =>
        {
            if (client is not null)
                row.ConstantItem(160).PaddingRight(18).Column(column =>
                {
                    column.Item().Text("ДЛЯ").FontSize(7.5f).FontColor(Muted).LetterSpacing(0.08f);
                    column.Item().Text(client).FontSize(12).Bold();
                    if (person is not null) column.Item().Text(person).FontSize(8.5f).FontColor(Muted);
                });
            row.RelativeItem().Column(column =>
            {
                if (model.Intro is { } intro) column.Item().Text(intro);
                if (model.VatNotice is { } vat) column.Item().PaddingTop(2).Text(vat).SemiBold();
            });
        });
    }

    private void Item(IContainer container, string number, QuotePdfItem item) =>
        container.EnsureSpace(190).BorderTop(1).BorderColor(Line).PaddingTop(14).Row(row =>
        {
            if (item.PhotoUrl is { } url && images.TryGetValue(url, out var photo))
                row.ConstantItem(140).PaddingRight(16).Height(182).Background(PhotoBack).Padding(4)
                    .Element(x => QuotePdfParts.Picture(x, photo));
            row.RelativeItem().Column(text =>
            {
                text.Spacing(3);
                text.Item().Text(title =>
                {
                    title.Span(number + "  ").FontColor(AccentText).SemiBold();
                    title.Span(item.Name).FontSize(12.5f).Bold();
                });
                var line = Join(item.Code is { } code ? $"Код {code}" : null, item.Subtitle, item.Season);
                if (line.Length > 0) text.Item().Text(line).FontSize(8).FontColor(Muted);
                if (item.Description is { } description) text.Item().PaddingTop(3).Text(description).FontSize(9);
                if (item.SiteUrl is { } site)
                    text.Item().Hyperlink(site).Text("Детальніше на сайті").FontColor(AccentText).FontSize(8.5f);
                text.Item().PaddingTop(6).Element(x => Prices(x, item));
            });
        });

    private void Prices(IContainer container, QuotePdfItem item)
    {
        if (item.Options.Count == 1)
        {
            // One price set: a strip of the quantities the client sees.
            container.Border(1).BorderColor(Line).Row(row =>
            {
                foreach (var column in item.Columns)
                    row.RelativeItem().PaddingVertical(6).PaddingHorizontal(10).Column(cell =>
                    {
                        cell.Item().Text(column.Label).FontSize(8).FontColor(Muted);
                        cell.Item().Text(Price(column.Prices[0])).FontSize(12).Bold();
                    });
            });
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.6f);
                foreach (var _ in item.Columns) columns.RelativeColumn(1.3f);
            });
            table.Header(header =>
            {
                header.Cell().Element(HeadCell).Text(item.OptionsLabel);
                foreach (var column in item.Columns) header.Cell().Element(HeadCell).AlignRight().Text(column.Label);
            });
            for (var index = 0; index < item.Options.Count; index++)
            {
                var option = item.Options[index];
                // The recommended row is tinted and marked, so the client sees it first.
                IContainer Cell(IContainer cell) => option.IsRecommended ? BodyCell(cell).Background(Soft) : BodyCell(cell);
                table.Cell().Element(Cell).Column(cell =>
                {
                    if (option.IsRecommended)
                        cell.Item().Text("РЕКОМЕНДУЄМО").FontColor(AccentText).FontSize(7).Bold().LetterSpacing(0.05f);
                    cell.Item().Text(option.Title).SemiBold();
                    if (option.Details is { } details) cell.Item().Text(details).FontColor(Muted).FontSize(8);
                });
                foreach (var column in item.Columns)
                    table.Cell().Element(Cell).AlignRight().Text(Price(column.Prices[index])).SemiBold();
            }
        });
    }

    /// <summary>The note before the footer (R-30): dark, as in the «Каталог» concept the owner chose.</summary>
    private void CallToAction(IContainer container)
    {
        if (model.CtaTitle is null && model.CtaText is null && model.CtaContact is null) return;
        container.ShowEntire().Background(Graphite).CornerRadius(8).PaddingVertical(16).PaddingHorizontal(20).Column(column =>
        {
            column.Spacing(5);
            if (model.CtaTitle is { } title) column.Item().Text(title).FontSize(12).Bold().FontColor(AccentOnDark);
            if (model.CtaText is { } text) column.Item().Text(text).FontColor("#E8E6E3");
            if (model.CtaContact is { } contact) column.Item().PaddingTop(2).Text(contact).FontColor(Colors.White).SemiBold();
        });
    }

    /// <summary>The responsible manager on the left, the site and the page number on the right.</summary>
    private void Footer(IContainer container) =>
        container.Background(Graphite).PaddingVertical(12).PaddingHorizontal(Side)
            .DefaultTextStyle(x => x.FontSize(8.5f).FontColor(Colors.White)).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(model.Manager.Name).Bold().FontColor(AccentOnDark);
                    if (model.Manager.Phone is { } phone) column.Item().Text(phone);
                    if (model.Manager.Email is { } email) column.Item().Text(email);
                });
                row.RelativeItem().AlignRight().AlignBottom().Column(column =>
                {
                    if (model.Company.Site is { } site)
                        column.Item().AlignRight().Hyperlink(site).Text(QuotePdfParts.Display(site));
                    column.Item().AlignRight().Text(text =>
                    {
                        text.Span("Сторінка ");
                        text.CurrentPageNumber();
                        text.Span(" з ");
                        text.TotalPages();
                    });
                });
            });

    private static string Price(decimal? value) => value is null ? "—" : $"{QuotePdfParts.Money(value)} грн";

    private static string? Prefixed(string label, string? value) => value is null ? null : $"{label} {value}";

    private static string Join(params string?[] parts) =>
        string.Join(" · ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static IContainer HeadCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Line).PaddingVertical(4).PaddingHorizontal(4)
            .DefaultTextStyle(x => x.FontSize(8).FontColor(Muted));

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Line).PaddingVertical(5).PaddingHorizontal(4);
}
