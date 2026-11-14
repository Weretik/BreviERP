using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Crm.Application.Features.Quotes.Shared.Pdf;

namespace Crm.Infrastructure.Integrations.Pdf;

/// <summary>Pieces both КП documents draw the same way: photos, money, links and the requisites footer.</summary>
internal static class QuotePdfParts
{
    public const string Ink = "#1F2328";
    public const string Muted = "#6B7280";
    public const string Line = "#E5E7EB";
    public static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    public static void Picture(IContainer container, QuotePdfImage image)
    {
        if (image.Svg is not null) container.Svg(image.Svg).FitArea();
        else container.AlignCenter().AlignMiddle().Image(image.Bitmap!).FitArea();
    }

    public static string Money(decimal? value) =>
        value is { } amount ? amount.ToString("#,##0.00", Uk) : "—";

    public static string Display(string url) =>
        url.Replace("https://", "", StringComparison.Ordinal).Replace("http://", "", StringComparison.Ordinal).TrimEnd('/');

    /// <summary>Company requisites on the left, page number on the right.</summary>
    public static void Footer(IContainer container, QuotePdfCompany company) =>
        container.BorderTop(1).BorderColor(Line).PaddingTop(6).Row(row =>
        {
            var requisites = string.Join(" · ", new[]
            {
                company.Name,
                company.Edrpou is { } edrpou ? $"ЄДРПОУ {edrpou}" : null,
                company.TaxId is { } tax ? $"ІПН {tax}" : null,
                company.Iban is { } iban ? $"IBAN {iban}" : null,
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
}
