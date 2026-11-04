using Crm.Domain.Tasks;

namespace Crm.Application.Tasks.Validation;

internal static class TaskStatusParser
{
    /// <summary>Only the names `New`, `InProgress`, `Done`; numbers are rejected.</summary>
    public static bool IsKnown(string? value) =>
        !string.IsNullOrEmpty(value) && !value.Any(char.IsDigit) &&
        Enum.TryParse<ManagerTaskStatus>(value, false, out var status) && Enum.IsDefined(status);
}
