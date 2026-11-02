using System.Text;
using System.Text.RegularExpressions;

namespace Reference.Application.Features.Shared.SheetImport;

/// <summary>Name comparison key for linking an older Brevi row without a code: case and punctuation are ignored.</summary>
internal static partial class ReferenceNameKey
{
    public static string From(string name)
    {
        var words = NonWord().Split(name.ToLower(CultureInfo.GetCultureInfo("uk-UA"))).Where(word => word.Length > 0);
        return new StringBuilder().AppendJoin(' ', words).ToString();
    }

    [GeneratedRegex(@"[^\p{L}\p{Nd}]+")]
    private static partial Regex NonWord();
}
