using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.PpeMarkupRules.Errors;
using Reference.Domain.PpeMarkupRules.ValueObjects;

namespace Reference.Domain.PpeMarkupRules.Entities;

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
    public static PpeMarkupRule Create(PpeMarkupRuleId id, string name, PpeMarkupRuleDetails details)
    {
        if (id.Value == default)
            throw new DomainException(PpeMarkupRuleErrors.IdIsRequired());
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(PpeMarkupRuleErrors.NameIsRequired());

        var rule = new PpeMarkupRule { Id = id, Name = Text(name, NameMaxLength, "Назва правила")! };
        rule.ApplyDetails(details);
        return rule;
    }

    public void ApplyDetails(PpeMarkupRuleDetails details)
    {
        RetailMarkup = Markup(details.RetailMarkup);
        WholesaleMarkup = Markup(details.WholesaleMarkup);
        LargeWholesaleMarkup = Markup(details.LargeWholesaleMarkup);
        PriceNote = Text(details.PriceNote, PriceNoteMaxLength, "Примітка для прайсу");
        AppliesTo = Text(details.AppliesTo, AppliesToMaxLength, "До яких товарів належить");
        SortOrder = details.SortOrder;
    }
    #endregion

    #region Validation
    private static decimal Markup(decimal value)
    {
        if (value is < 0 or > MaxMarkup)
            throw new DomainException(PpeMarkupRuleErrors.MarkupIsOutOfRange());
        return value;
    }

    private static string? Text(string? text, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        var trimmed = text.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException(PpeMarkupRuleErrors.TextIsTooLong(field, maxLength));
        return trimmed;
    }
    #endregion
}
