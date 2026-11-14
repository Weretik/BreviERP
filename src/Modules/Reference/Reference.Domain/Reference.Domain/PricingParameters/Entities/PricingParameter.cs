using BuildingBlocks.Domain.Errors;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
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
    public static PricingParameter Create(PricingParameterId id, string code, PricingParameterDetails details,
        DateOnly? updatedOn)
    {
        if (id.Value == default)
            throw new DomainException(PricingParameterErrors.IdIsRequired());

        var parameter = new PricingParameter { Id = id, Code = ValidCode(code) };
        parameter.ApplyDetails(details, updatedOn);
        return parameter;
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
    public void ApplyDetails(PricingParameterDetails details, DateOnly? updatedOn)
    {
        Group = Required(details.Group, GroupMaxLength, "Група", PricingParameterErrors.GroupIsRequired());
        Name = Required(details.Name, NameMaxLength, "Назва", PricingParameterErrors.NameIsRequired());
        Unit = Required(details.Unit, UnitMaxLength, "Одиниця виміру", PricingParameterErrors.UnitIsRequired());
        Comment = Optional(details.Comment, CommentMaxLength, "Коментар");
        SetValue(details.IsCalculated, details.Value);
        SortOrder = details.SortOrder;
        UpdatedOn = updatedOn;
    }
    #endregion

    #region Validation
    private static string ValidCode(string code)
    {
        var normalized = NormalizeCode(code);
        if (!IsValidCode(normalized))
            throw new DomainException(PricingParameterErrors.CodeIsInvalid());
        return normalized!;
    }

    private void SetValue(bool isCalculated, decimal? value)
    {
        if (isCalculated)
        {
            if (value is not null)
                throw new DomainException(PricingParameterErrors.CalculatedValueIsNotAllowed());
            if (!PricingParameterFormulas.IsKnown(Code))
                throw new DomainException(PricingParameterErrors.FormulaIsUnknown());
        }
        else if (value is null)
            throw new DomainException(PricingParameterErrors.ValueIsRequired());
        else if (value < 0)
            throw new DomainException(PricingParameterErrors.ValueMustBeNonNegative());

        IsCalculated = isCalculated;
        Value = value;
    }

    private static string Required(string? text, int maxLength, string field, Error missing)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException(missing);
        return Optional(text, maxLength, field)!;
    }

    private static string? Optional(string? text, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        var trimmed = text.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException(PricingParameterErrors.TextIsTooLong(field, maxLength));
        return trimmed;
    }
    #endregion
}
