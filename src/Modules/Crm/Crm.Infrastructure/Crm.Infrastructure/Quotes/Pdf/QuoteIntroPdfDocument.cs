using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Crm.Application.Features.Quotes.Shared.Pdf.Introduction;

namespace Crm.Infrastructure.Quotes.Pdf;

/// <summary>
/// «КП для знайомства» (R-16) as the client's sample: one A4 page per season. A header with the logo and the
/// manager's phone, the season banner with the title and the main model, the five advantages, three popular models,
/// the prices note, the next step and the manager's contacts. Long texts are cut to their lines, so a page never
/// spills over.
/// </summary>
internal sealed class QuoteIntroPdfDocument(QuoteIntroPdfModel model, IReadOnlyDictionary<string, QuotePdfImage> images)
    : IDocument
{
    private const string Ink = "#10131B";
    private const string Muted = "#5F6672";
    private const string Line = "#E3E6EA";
    private const string Panel = "#EFF3F5";
    private const float Side = 20;

    private string Accent => model.Accent;
    private string Tint => $"#24{model.Accent.TrimStart('#')}";

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
            page.Margin(0);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontFamily(QuotePdfFonts.IntroFamily).FontSize(8).FontColor(Ink).LineHeight(1.15f));
            page.Content().Column(column =>
            {
                for (var index = 0; index < model.Pages.Count; index++)
                {
                    if (index > 0) column.Item().PageBreak();
                    var season = model.Pages[index];
                    // A page that would not fit (very long texts) is scaled down rather than spilling over.
                    column.Item().Height(PageSizes.A4.Height - 1).ScaleToFit().Element(x => Page(x, season));
                }
            });
        });

    private void Page(IContainer container, QuoteIntroPdfPage page) =>
        container.Column(column =>
        {
            column.Item().Height(64).Element(Header);
            column.Item().Height(238).Element(x => Banner(x, page));
            column.Item().PaddingHorizontal(Side).PaddingTop(12).Height(56).Element(Advantages);
            column.Item().PaddingHorizontal(Side).PaddingTop(16).Element(x => ModelsHeader(x, page));
            column.Item().PaddingHorizontal(Side).PaddingTop(12).Row(row =>
            {
                row.Spacing(8);
                var cards = page.Models.Skip(1).Take(3).ToArray();
                foreach (var card in cards) row.RelativeItem().Element(x => Card(x, card, page));
                for (var empty = cards.Length; empty < 3; empty++) row.RelativeItem();
            });
            if (model.PricesNote is { } note)
                column.Item().PaddingHorizontal(Side).PaddingTop(7).Text(note).FontSize(7.5f).FontColor(Muted).ClampLines(1);
            if (model.NextStepText is { } next)
                column.Item().PaddingHorizontal(Side).PaddingTop(10).Element(x => NextStep(x, next));
            column.Item().PaddingHorizontal(36).PaddingTop(8).Element(Footer);
        });

    private void Header(IContainer container) =>
        container.PaddingHorizontal(40).Row(row =>
        {
            row.ConstantItem(100).AlignMiddle().Element(x => Logo(x, 44));
            row.ConstantItem(18);
            row.ConstantItem(0.8f).AlignMiddle().Height(30).Background(Line);
            row.ConstantItem(12);
            row.RelativeItem().AlignMiddle().Column(column =>
            {
                if (model.LogoText is { } logoText)
                    foreach (var line in logoText.Split('\n').Take(3))
                        column.Item().Text(line.Trim()).FontSize(8).ClampLines(1);
            });
            if (model.Manager.Phone is { } phone)
                row.ConstantItem(170).AlignMiddle().Row(contact =>
                {
                    contact.ConstantItem(20).AlignMiddle().Height(18).Svg(QuoteIntroPdfArt.Icon(QuoteIntroPdfArt.Phone, Accent)).FitArea();
                    contact.ConstantItem(8);
                    contact.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Контакт вашого менеджера:").FontSize(7.5f).FontColor(Muted);
                        column.Item().Text(phone).FontSize(12).Medium();
                    });
                });
        });

    private void Logo(IContainer container, float height)
    {
        if (model.Company.LogoUrl is { } url && images.TryGetValue(url, out var logo))
        {
            container.Height(height).AlignLeft().Element(x => QuotePdfParts.Picture(x, logo));
            return;
        }

        container.Text(model.Company.Name).FontSize(height * 0.45f).Medium().FontColor(Accent);
    }

    private void Banner(IContainer container, QuoteIntroPdfPage page) =>
        container.Layers(layers =>
        {
            layers.PrimaryLayer().AlignRight().AlignTop().Element(box =>
            {
                if (page.BannerUrl is { } url && images.TryGetValue(url, out var banner) && banner.Bitmap is not null)
                    box.Image(banner.Bitmap).FitArea();
                else box.Image(QuoteIntroPdfArt.DefaultBanner(page.Season)).FitArea();
            });
            layers.Layer().Svg(QuoteIntroPdfArt.BannerFade).FitArea();
            layers.Layer().Row(row =>
            {
                row.ConstantItem(270).PaddingLeft(26).PaddingTop(12).PaddingBottom(14).Column(column =>
                {
                    // The title breaks as in the sample, the subtitle under it is wider.
                    column.Item().Height(162).PaddingRight(44).Text(text =>
                    {
                        text.ClampLines(5);
                        text.DefaultTextStyle(x => x.FontSize(30).Medium().LineHeight(1.06f));
                        foreach (var part in QuoteIntroTitlePart.Split(page.Title))
                            text.Span(part.Text).FontColor(part.IsAccent ? Accent : Ink);
                    });
                    if (page.Subtitle is { } subtitle)
                        column.Item().PaddingTop(6).Text(subtitle).FontSize(9.5f).LineHeight(1.25f).ClampLines(3);
                });
                row.RelativeItem();
                if (page.Models.FirstOrDefault() is { } main)
                    row.ConstantItem(140).AlignBottom().Element(x => MainCard(x, main, page));
                row.ConstantItem(12);
            });
        });

    private void MainCard(IContainer container, QuoteIntroPdfCard card, QuoteIntroPdfPage page) =>
        container.Background(Colors.White).PaddingHorizontal(8).PaddingTop(10).PaddingBottom(9).Column(column =>
        {
            column.Item().Text(card.Name).FontSize(12).Medium().ClampLines(3);
            column.Item().PaddingTop(6).Element(x => SeasonBadge(x, page));
            if (Fabric(card) is { } fabric)
                column.Item().PaddingTop(6).Text(fabric).FontSize(7.5f).FontColor(Muted).ClampLines(4);
            if (card.Price is { } price)
            {
                column.Item().PaddingTop(8).Text($"від {Money(price)}").FontSize(15).Medium().FontColor(Accent);
                column.Item().PaddingTop(2).Text("з ПДВ · за замовлення від 41 шт.").FontSize(6.3f).FontColor(Muted);
            }
        });

    private void SeasonBadge(IContainer container, QuoteIntroPdfPage page) =>
        container.AlignLeft().Background(Tint).CornerRadius(2).PaddingVertical(4).PaddingHorizontal(6).Row(row =>
        {
            row.AutoItem().AlignMiddle().Width(12).Height(12)
                .Svg(QuoteIntroPdfArt.Icon(QuoteIntroPdfArt.SeasonIcon(page.Season), Accent, 1.8f)).FitArea();
            row.AutoItem().PaddingLeft(6).AlignMiddle().Text(QuoteIntroPdfArt.SeasonLabel(page.Season)).FontSize(7.5f).Medium();
        });

    private void Advantages(IContainer container) =>
        container.Row(row =>
        {
            for (var index = 0; index < model.Advantages.Count; index++)
            {
                var advantage = model.Advantages[index];
                var icon = QuoteIntroPdfArt.Advantages[index % QuoteIntroPdfArt.Advantages.Length];
                var item = row.RelativeItem();
                if (index > 0) item = item.BorderLeft(0.7f).BorderColor(Line);
                item.PaddingLeft(index > 0 ? 8 : 6).PaddingRight(12).Row(cell =>
                {
                    cell.ConstantItem(18).Height(18).Svg(QuoteIntroPdfArt.Icon(icon, Accent)).FitArea();
                    cell.ConstantItem(7);
                    cell.RelativeItem().Column(column =>
                    {
                        column.Item().Text(advantage.Title).FontSize(8.3f).Medium().ClampLines(2);
                        if (advantage.Text is { } text)
                            column.Item().PaddingTop(4).Text(text).FontSize(7).FontColor(Muted).ClampLines(3);
                    });
                });
            }
        });

    private void ModelsHeader(IContainer container, QuoteIntroPdfPage page) =>
        container.Row(row =>
        {
            row.RelativeItem().AlignBottom().Text(page.ModelsTitle ?? "").FontSize(11).Medium().FontColor(Accent)
                .LetterSpacing(0.16f).ClampLines(1);
            if (model.AssortmentUrl is { } url)
                row.AutoItem().AlignBottom().Hyperlink(url).Row(link =>
                {
                    link.AutoItem().AlignMiddle()
                        .Text(model.AssortmentLinkText ?? QuotePdfParts.Display(url)).FontSize(8).FontColor(Muted);
                    link.AutoItem().PaddingLeft(8).AlignMiddle().Width(16).Height(14)
                        .Svg(QuoteIntroPdfArt.Icon(QuoteIntroPdfArt.Arrow, Accent, 1.8f)).FitArea();
                });
        });

    private void Card(IContainer container, QuoteIntroPdfCard card, QuoteIntroPdfPage page) =>
        container.Border(0.7f).BorderColor(Line).CornerRadius(3).Padding(9).Column(column =>
        {
            column.Item().Height(156).Layers(layers =>
            {
                layers.PrimaryLayer().Element(box =>
                {
                    if (card.PhotoUrl is { } url && images.TryGetValue(url, out var photo))
                        QuotePdfParts.Picture(box, photo);
                });
                layers.Layer().AlignRight().AlignTop().Element(x => SeasonBadge(x, page));
            });
            column.Item().PaddingTop(10).Height(28).Text(card.Name).FontSize(11).Medium().LineHeight(1.1f).ClampLines(2);
            column.Item().PaddingTop(5).Height(40).Row(row =>
            {
                row.ConstantItem(15).PaddingTop(1).Height(13)
                    .Svg(QuoteIntroPdfArt.Icon(QuoteIntroPdfArt.Layers, Muted, 1.5f)).FitArea();
                row.ConstantItem(6);
                if (Fabric(card) is { } fabric)
                    row.RelativeItem().Text(fabric).FontSize(7.8f).FontColor(Muted).ClampLines(4);
            });
            column.Item().PaddingTop(6).Height(20).Text(text =>
            {
                if (card.Price is not { } price) return;
                text.Span($"від {Money(price)}").FontSize(15).Medium().FontColor(Accent);
                text.Span("  з ПДВ").FontSize(7.5f).FontColor(Muted);
            });
            column.Item().PaddingTop(4).Element(x => Button(x, card));
        });

    private void Button(IContainer container, QuoteIntroPdfCard card)
    {
        var button = card.SiteUrl is { } url ? container.Hyperlink(url) : container;
        button.Height(20).Background(Accent).Row(row =>
        {
            row.RelativeItem().AlignCenter().AlignMiddle().Text(model.ProductButtonText).FontSize(8.5f).Medium()
                .FontColor(Colors.White).ClampLines(1);
            row.ConstantItem(28).AlignMiddle().PaddingRight(12).Height(12)
                .Svg(QuoteIntroPdfArt.Icon(QuoteIntroPdfArt.Arrow, "#FFFFFF", 1.8f)).FitArea();
        });
    }

    private void NextStep(IContainer container, string text)
    {
        var box = model.Manager.Email is { } email ? container.Hyperlink($"mailto:{email}") : container;
        box.Height(42).Background(Panel).Row(row =>
        {
            row.ConstantItem(64).Background(Accent).AlignCenter().AlignMiddle().Width(24).Height(24)
                .Svg(QuoteIntroPdfArt.Icon(QuoteIntroPdfArt.Chat, "#FFFFFF", 1.6f)).FitArea();
            row.RelativeItem().PaddingHorizontal(16).AlignMiddle().Text(text).FontSize(9.8f).ClampLines(2);
            row.ConstantItem(44).AlignMiddle().PaddingRight(16).Height(20)
                .Svg(QuoteIntroPdfArt.Icon(QuoteIntroPdfArt.Arrow, Accent, 1.6f)).FitArea();
        });
    }

    private void Footer(IContainer container) =>
        container.Row(row =>
        {
            row.ConstantItem(96).AlignMiddle().Element(x => Logo(x, 34));
            row.ConstantItem(24);
            row.ConstantItem(20).AlignMiddle().Height(18).Svg(QuoteIntroPdfArt.Icon(QuoteIntroPdfArt.Phone, Accent)).FitArea();
            row.ConstantItem(10);
            row.RelativeItem().AlignMiddle().Column(column =>
            {
                column.Item().Text($"Ваш менеджер: {model.Manager.Name}").FontSize(7.2f).FontColor(Muted).ClampLines(1);
                if (model.Manager.Phone is { } phone) column.Item().Text(phone).FontSize(12).Medium();
            });
            row.ConstantItem(20).AlignMiddle().Height(16).Svg(QuoteIntroPdfArt.Icon(QuoteIntroPdfArt.Mail, Accent)).FitArea();
            row.ConstantItem(10);
            row.ConstantItem(140).AlignMiddle().Column(column =>
            {
                if (model.Manager.Email is { } email) column.Item().Text(email).FontSize(8).FontColor(Muted).ClampLines(1);
                if (model.Company.Site is { } site)
                    column.Item().PaddingTop(4).Hyperlink(site).Text(QuotePdfParts.Display(site)).FontSize(8)
                        .FontColor(Muted).ClampLines(1);
            });
        });

    /// <summary>«Тканина: …» with the composition and density on their own lines.</summary>
    private static string? Fabric(QuoteIntroPdfCard card)
    {
        var lines = new List<string>();
        if (card.Fabric is { } fabric) lines.Add($"Тканина: {fabric}");
        lines.AddRange(card.FabricLines);
        return lines.Count == 0 ? null : string.Join("\n", lines);
    }

    /// <summary>Whole hryvnias: the card holds a price rounded up (R-35).</summary>
    private static string Money(decimal value) => $"{value.ToString("#,##0", QuotePdfParts.Uk)} грн";
}
