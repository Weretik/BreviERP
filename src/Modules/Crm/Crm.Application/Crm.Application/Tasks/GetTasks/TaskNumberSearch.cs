namespace Crm.Application.Tasks.GetTasks;

internal static class TaskNumberSearch
{
    /// <summary>«ЗД-0142», «зд142» or «142» → 142; anything else → -1 (matches no task).</summary>
    public static long Parse(string search)
    {
        var digits = search.StartsWith("зд", StringComparison.Ordinal) ? search[2..].TrimStart('-', ' ') : search;
        return digits.Length is > 0 and <= 12 && digits.All(char.IsAsciiDigit) ? long.Parse(digits) : -1;
    }
}
