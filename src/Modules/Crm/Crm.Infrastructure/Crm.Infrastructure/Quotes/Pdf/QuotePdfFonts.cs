using QuestPDF.Drawing;

namespace Crm.Infrastructure.Quotes.Pdf;

/// <summary>
/// Noto Sans (the КП) and Roboto (the «КП для знайомства», as in the client's sample) are embedded so Ukrainian text
/// prints the same on every server.
/// </summary>
internal static class QuotePdfFonts
{
    public const string Family = "Noto Sans";
    public const string IntroFamily = "Roboto";
    private static readonly Lazy<bool> Registered = new(Register);

    public static void EnsureRegistered() => _ = Registered.Value;

    private static bool Register()
    {
        var assembly = typeof(QuotePdfFonts).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(x => x.EndsWith(".ttf", StringComparison.Ordinal)))
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            FontManager.RegisterFont(stream);
        }

        return true;
    }
}
