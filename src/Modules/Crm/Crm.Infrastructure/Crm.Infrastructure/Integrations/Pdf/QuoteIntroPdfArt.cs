using Crm.Domain.Quotes;

namespace Crm.Infrastructure.Integrations.Pdf;

/// <summary>
/// The line icons, banner fade and built-in season banners of the «КП для знайомства» (R-16). Icons are 24×24
/// outlines in the accent colour, drawn after the Lucide set (ISC license).
/// </summary>
internal static class QuoteIntroPdfArt
{
    private const string Factory =
        "<path d=\"M2 20a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V8l-7 5V8l-7 5V4a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2Z\"/>" +
        "<path d=\"M17 18h1M12 18h1M7 18h1\"/>";

    private const string Calendar =
        "<rect width=\"18\" height=\"18\" x=\"3\" y=\"4\" rx=\"2\"/><path d=\"M16 2v4M8 2v4M3 10h18\"/>" +
        "<path d=\"M8 14h.01M12 14h.01M16 14h.01M8 18h.01M12 18h.01M16 18h.01\"/>";

    private const string Document =
        "<path d=\"M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z\"/>" +
        "<path d=\"M14 2v4a2 2 0 0 0 2 2h4\"/><path d=\"M10 9H8M16 13H8M16 17H8\"/>";

    private const string Truck =
        "<path d=\"M14 18V6a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2v11a1 1 0 0 0 1 1h2\"/><path d=\"M15 18H9\"/>" +
        "<path d=\"M19 18h2a1 1 0 0 0 1-1v-3.65a1 1 0 0 0-.22-.62l-3.48-4.35A1 1 0 0 0 17.52 8H14\"/>" +
        "<circle cx=\"17\" cy=\"18\" r=\"2\"/><circle cx=\"7\" cy=\"18\" r=\"2\"/>";

    private const string Shirt =
        "<path d=\"M20.38 3.46 16 2a4 4 0 0 1-8 0L3.62 3.46a2 2 0 0 0-1.34 2.23l.58 3.47a1 1 0 0 0 .99.84H6v10" +
        "c0 1.1.9 2 2 2h8a2 2 0 0 0 2-2V10h2.15a1 1 0 0 0 .99-.84l.58-3.47a2 2 0 0 0-1.34-2.23z\"/>";

    public const string Sun =
        "<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41" +
        "M2 12h2M20 12h2M6.34 17.66l-1.41 1.41M19.07 4.93l-1.41 1.41\"/>";

    public const string Snowflake =
        "<path d=\"M2 12h20M12 2v20M4.93 4.93l14.14 14.14M19.07 4.93 4.93 19.07\"/>" +
        "<path d=\"m9 4 3 3 3-3M9 20l3-3 3 3M4 9l3 3-3 3M20 9l-3 3 3 3\"/>";

    public const string Layers =
        "<path d=\"M12.83 2.18a2 2 0 0 0-1.66 0L2.6 6.08a1 1 0 0 0 0 1.83l8.58 3.91a2 2 0 0 0 1.66 0l8.58-3.9" +
        "a1 1 0 0 0 0-1.83Z\"/><path d=\"m22 17.65-9.17 4.16a2 2 0 0 1-1.66 0L2 17.65\"/>" +
        "<path d=\"m22 12.65-9.17 4.16a2 2 0 0 1-1.66 0L2 12.65\"/>";

    public const string Arrow = "<path d=\"M3 12h18M14 5l7 7-7 7\"/>";

    public const string Chat = "<path d=\"M7.9 20A9 9 0 1 0 4 16.1L2 22Z\"/>";

    public const string Phone =
        "<path d=\"M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1" +
        "-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91" +
        "a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z\"/>";

    public const string Mail =
        "<rect width=\"20\" height=\"16\" x=\"2\" y=\"4\" rx=\"2\"/><path d=\"m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7\"/>";

    /// <summary>The five advantages of the settings, in order: production, years, VAT, delivery, branding.</summary>
    public static readonly string[] Advantages = [Factory, Calendar, Document, Truck, Shirt];

    public static string Icon(string paths, string color, float stroke = 1.6f) =>
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" " +
        $"stroke=\"{color}\" stroke-width=\"{stroke.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" " +
        $"stroke-linecap=\"round\" stroke-linejoin=\"round\">{paths}</svg>";

    public static string SeasonIcon(QuoteIntroSeason season) => season == QuoteIntroSeason.Summer ? Sun : Snowflake;

    public static string SeasonLabel(QuoteIntroSeason season) => season == QuoteIntroSeason.Summer ? "ЛІТО" : "ЗИМА";

    /// <summary>White on the left, where the title is, fading into the photo.</summary>
    public const string BannerFade =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"600\" height=\"240\" viewBox=\"0 0 600 240\" " +
        "preserveAspectRatio=\"none\"><defs><linearGradient id=\"f\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"0\">" +
        "<stop offset=\"0\" stop-color=\"#FFFFFF\" stop-opacity=\"0.96\"/>" +
        "<stop offset=\"0.3\" stop-color=\"#FFFFFF\" stop-opacity=\"0.85\"/>" +
        "<stop offset=\"0.5\" stop-color=\"#FFFFFF\" stop-opacity=\"0.35\"/>" +
        "<stop offset=\"0.62\" stop-color=\"#FFFFFF\" stop-opacity=\"0\"/></linearGradient></defs>" +
        "<rect width=\"600\" height=\"240\" fill=\"url(#f)\"/></svg>";

    private static readonly Lazy<byte[]> SummerBanner = new(() => Resource("intro-summer.jpg"));
    private static readonly Lazy<byte[]> WinterBanner = new(() => Resource("intro-winter.jpg"));

    /// <summary>The banner printed when «Налаштування КП → Знайомство» has none for the season.</summary>
    public static byte[] DefaultBanner(QuoteIntroSeason season) =>
        season == QuoteIntroSeason.Summer ? SummerBanner.Value : WinterBanner.Value;

    private static byte[] Resource(string name)
    {
        using var stream = typeof(QuoteIntroPdfArt).Assembly.GetManifestResourceStream($"Brevi.Quotes.Pdf.Banners.{name}")
            ?? throw new InvalidOperationException($"The built-in banner {name} is missing.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
