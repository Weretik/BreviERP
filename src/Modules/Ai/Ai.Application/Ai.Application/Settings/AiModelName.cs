using System.Text.RegularExpressions;
using Ai.Domain.Settings;

namespace Ai.Application.Settings;

internal static partial class AiModelName
{
    public const string Message = "Назва моделі — латиниця, цифри, крапка, дефіс або двокрапка, до 80 символів.";

    public static bool IsValid(string? model) =>
        model is not null && model.Trim().Length <= AiSettings.ModelMaxLength && Pattern().IsMatch(model.Trim());

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._:-]*$")]
    private static partial Regex Pattern();
}
