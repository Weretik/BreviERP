using System.Text;
using System.Text.RegularExpressions;

namespace Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;

/// <summary>
/// Name comparison key for linking a manual supplier to KeepinCRM: case, quotes, punctuation and legal
/// form words (ТОВ, ФОП, ПП, ООО, ТМ) are ignored, so «Скай Текс, ТОВ» matches «Скай Текс».
/// </summary>
internal static partial class SupplierNameKey
{
    private static readonly HashSet<string> LegalForms = ["тов", "фоп", "пп", "ооо", "тм", "фл", "чп"];

    public static string From(string name)
    {
        var lowered = name.ToLower(CultureInfo.GetCultureInfo("uk-UA"));
        var words = NonWord().Split(lowered)
            .Where(word => word.Length > 0 && !LegalForms.Contains(word));
        return new StringBuilder().AppendJoin(' ', words).ToString();
    }

    [GeneratedRegex(@"[^\p{L}\p{Nd}]+")]
    private static partial Regex NonWord();
}
