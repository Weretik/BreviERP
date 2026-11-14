using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;

namespace Reference.Domain.PpeMarkupRules;

/// <summary>
/// A PPE markup rule from «Налаштування цін» («Взуття», «Рукавички»…): price = supplier price × (1 + markup).
/// <see cref="Name"/> is the rule code that PPE supplier offers refer to, so it never changes after creation.
/// </summary>
public class PpeMarkupRule : BaseEntity<PpeMarkupRuleId>, IAggregateRoot
{
    public const int NameMaxLength = 200;
    public const int PriceNoteMaxLength = 500;
    public const int AppliesToMaxLength = 500;
    public const decimal MaxMarkup = 10m;

    #region Properties
    public string Name { get; private set; } = null!;
    public decimal RetailMarkup { get; private set; }
    public decimal WholesaleMarkup { get; private set; }
    public decimal LargeWholesaleMarkup { get; private set; }
    public string? PriceNote { get; private set; }
    public string? AppliesTo { get; private set; }
    public int SortOrder { get; private set; }
    #endregion

    #region Constructors
    private PpeMarkupRule() { }
    #endregion

    #region Factories
    public static DomainResult<PpeMarkupRule> Create(PpeMarkupRuleId id, string name, PpeMarkupRuleDetails details)
    {
        if (id.Value == default) throw new ArgumentException("Markup rule id is required.", nameof(id));
        var check = DomainResult.Combine(
            string.IsNullOrWhiteSpace(name)
                ? PpeMarkupRuleErrors.NameIsRequired()
                : Text(name, NameMaxLength, "Назва правила"),
            CheckDetails(details));
        if (check.IsFailure) return check.Error!;

        var rule = new PpeMarkupRule { Id = id, Name = name.Trim() };
        rule.SetDetails(details);
        return rule;
    }

    public DomainResult ApplyDetails(PpeMarkupRuleDetails details)
    {
        var check = CheckDetails(details);
        if (check.IsFailure) return check;
        SetDetails(details);
        return DomainResult.Success();
    }
    #endregion

    #region Rules
    private static DomainResult CheckDetails(PpeMarkupRuleDetails details) => DomainResult.Combine(
        Markup(details.RetailMarkup),
        Markup(details.WholesaleMarkup),
        Markup(details.LargeWholesaleMarkup),
        Text(details.PriceNote, PriceNoteMaxLength, "Примітка для прайсу"),
        Text(details.AppliesTo, AppliesToMaxLength, "До яких товарів належить"));

    private void SetDetails(PpeMarkupRuleDetails details)
    {
        RetailMarkup = details.RetailMarkup;
        WholesaleMarkup = details.WholesaleMarkup;
        LargeWholesaleMarkup = details.LargeWholesaleMarkup;
        PriceNote = Clean(details.PriceNote);
        AppliesTo = Clean(details.AppliesTo);
        SortOrder = details.SortOrder;
    }

    private static DomainResult Markup(decimal value) =>
        value is < 0 or > MaxMarkup ? PpeMarkupRuleErrors.MarkupIsOutOfRange() : DomainResult.Success();

    private static DomainResult Text(string? text, int maxLength, string field) =>
        Clean(text) is { Length: var length } && length > maxLength
            ? PpeMarkupRuleErrors.TextIsTooLong(field, maxLength)
            : DomainResult.Success();

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    #endregion
}
