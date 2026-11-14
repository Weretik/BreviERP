using BuildingBlocks.Domain.Errors;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;
using Reference.Domain.PricingParameters.Errors;
using Reference.Domain.PricingParameters.ValueObjects;

namespace Reference.Domain.PricingParameters.Entities;

/// <summary>
/// A pricing parameter from the Calculator sheet «Налаштування цін»: salary, work time, production rates,
/// overhead and price multipliers. <see cref="Code"/> (SALARY_SEAMSTRESS…) is the stable key that pricing and
/// the sheet import use; it never changes after creation. Percentages are stored as fractions (0.25 = 25 %).
/// </summary>
public class PricingParameter : BaseEntity<PricingParameterId>, IAggregateRoot
{
    public const int CodeMaxLength = 64;
    public const int GroupMaxLength = 100;
    public const int NameMaxLength = 200;
    public const int UnitMaxLength = 30;
    public const int CommentMaxLength = 1000;

    private static readonly Regex CodePattern = new("^[A-Z][A-Z0-9_]*$", RegexOptions.Compiled);

    #region Properties
    public string Code { get; private set; } = null!;
    public string Group { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public decimal? Value { get; private set; }
    public string Unit { get; private set; } = null!;
    public bool IsCalculated { get; private set; }
    public DateOnly? UpdatedOn { get; private set; }
    public string? Comment { get; private set; }
    public int SortOrder { get; private set; }
    #endregion

    #region Constructors
    private PricingParameter() { }
    #endregion

    #region Factories
    public static DomainResult<PricingParameter> Create(PricingParameterId id, string code,
        PricingParameterDetails details, DateOnly? updatedOn)
    {
        if (id.Value == default) throw new ArgumentException("Pricing parameter id is required.", nameof(id));
        var normalized = NormalizeCode(code);
        if (!IsValidCode(normalized)) return PricingParameterErrors.CodeIsInvalid();

        var parameter = new PricingParameter { Id = id, Code = normalized! };
        var applied = parameter.ApplyDetails(details, updatedOn);
        return applied.IsFailure ? applied.Error! : parameter;
    }

    /// <summary>Upper-cases and trims a code; returns null for an empty one.</summary>
    public static string? NormalizeCode(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

    public static bool IsValidCode(string? code) =>
        code is { Length: > 0 and <= CodeMaxLength } && CodePattern.IsMatch(code);

    /// <summary>
    /// Applies the card fields. <paramref name="updatedOn"/> is the «Дата оновлення»: the sheet's date on import,
    /// today's date when someone edits the value in Brevi.
    /// </summary>
    public DomainResult ApplyDetails(PricingParameterDetails details, DateOnly? updatedOn)
    {
        var check = DomainResult.Combine(
            Required(details.Group, GroupMaxLength, "Група", PricingParameterErrors.GroupIsRequired()),
            Required(details.Name, NameMaxLength, "Назва", PricingParameterErrors.NameIsRequired()),
            Required(details.Unit, UnitMaxLength, "Одиниця виміру", PricingParameterErrors.UnitIsRequired()),
            Optional(details.Comment, CommentMaxLength, "Коментар"),
            CheckValue(details.IsCalculated, details.Value));
        if (check.IsFailure) return check;

        Group = details.Group.Trim();
        Name = details.Name.Trim();
        Unit = details.Unit.Trim();
        Comment = string.IsNullOrWhiteSpace(details.Comment) ? null : details.Comment.Trim();
        IsCalculated = details.IsCalculated;
        Value = details.Value;
        SortOrder = details.SortOrder;
        UpdatedOn = updatedOn;
        return DomainResult.Success();
    }
    #endregion

    #region Rules
    private DomainResult CheckValue(bool isCalculated, decimal? value)
    {
        if (isCalculated)
        {
            if (value is not null) return PricingParameterErrors.CalculatedValueIsNotAllowed();
            return PricingParameterFormulas.IsKnown(Code)
                ? DomainResult.Success()
                : PricingParameterErrors.FormulaIsUnknown();
        }

        return value switch
        {
            null => PricingParameterErrors.ValueIsRequired(),
            < 0 => PricingParameterErrors.ValueMustBeNonNegative(),
            _ => DomainResult.Success()
        };
    }

    private static DomainResult Required(string? text, int maxLength, string field, Error missing) =>
        string.IsNullOrWhiteSpace(text) ? missing : Optional(text, maxLength, field);

    private static DomainResult Optional(string? text, int maxLength, string field) =>
        text?.Trim() is { Length: var length } && length > maxLength
            ? PricingParameterErrors.TextIsTooLong(field, maxLength)
            : DomainResult.Success();
    #endregion
}
