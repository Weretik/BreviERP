using BuildingBlocks.Domain.Results;

namespace Reference.Domain.GarmentAccessories;

/// <summary>
/// Optional text of a reference card: <see cref="Check"/> reports a value longer than the column as a rule;
/// <see cref="Optional"/> then trims it and turns blank into null.
/// </summary>
internal static class ReferenceText
{
    public static DomainResult Check(string? value, int maxLength, Func<int, Error> tooLong) =>
        Optional(value) is { Length: var length } && length > maxLength
            ? tooLong(maxLength)
            : DomainResult.Success();

    public static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
