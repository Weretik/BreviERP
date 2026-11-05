using Accounting.Domain.Movements;

namespace Accounting.Application.Journal;

internal static class MovementBy
{
    /// <summary>The e-mail of the staff account written to the record and the log.</summary>
    public static IRuleBuilderOptions<T, string> ValidBy<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(MovementRules.ByMaxLength);
}
