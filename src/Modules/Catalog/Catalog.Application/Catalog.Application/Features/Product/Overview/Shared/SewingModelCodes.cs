using System.Globalization;
using System.Text.RegularExpressions;

namespace Catalog.Application.Features.Product.Overview.Shared;

/// <summary>Sewing model codes from the sheet: «MOD-» and a number of at least three digits.</summary>
public static partial class SewingModelCodes
{
    public const string Prefix = "MOD-";

    /// <summary>One more than the largest MOD number in use, padded to three digits (MOD-001 when none).</summary>
    public static string Next(IEnumerable<string> codes)
    {
        var max = 0;
        foreach (var code in codes)
        {
            var match = CodePattern().Match(code.Trim());
            if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
                max = Math.Max(max, number);
        }

        return Prefix + (max + 1).ToString("000", CultureInfo.InvariantCulture);
    }

    [GeneratedRegex(@"^MOD-(\d{1,9})$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CodePattern();
}
