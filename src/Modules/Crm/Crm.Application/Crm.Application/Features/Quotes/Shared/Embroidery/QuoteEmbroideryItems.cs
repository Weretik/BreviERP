using System.Globalization;
using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Features.Quotes.Shared.Embroidery;

/// <summary>
/// Turns a saved «Ціна вишивки» calculation into a КП item (EMB-001 R-12): the unit price of its one saved category
/// stands in all three КП tiers (the owner's choice of 2026-11-04), and the item is never repriced.
/// </summary>
public static class QuoteEmbroideryItems
{
    public const string GarmentName = "Вишивка";
    public const string ChevronName = "Шеврони";


    public static QuoteItem Create(int position, EmbroiderySavedCalculation saved, EmbroideryPrice price) =>
        QuoteItem.Embroidery(position, saved.Id, Texts(saved), Option(saved, price));

    /// <summary>The saved item again, with the description and the «показати» ticks from the editor.</summary>
    public static QuoteItem Keep(int position, QuoteItem previous, string? description, bool retail, bool wholesale,
        bool large) =>
        QuoteItem.Embroidery(position, previous.EmbroideryCalculationId!.Value,
            new QuoteItemTexts(null, previous.Name, previous.Subtitle, description, null, previous.PhotoUrl, null, null),
            previous.Options[0].CopyShown(retail, wholesale, large));

    private static QuoteItemTexts Texts(EmbroiderySavedCalculation saved)
    {
        var chevron = saved.Mode == EmbroideryMode.Chevron;
        var kind = chevron ? ChevronName : GarmentName;
        var name = string.IsNullOrWhiteSpace(saved.Name) ? kind : saved.Name;
        return new QuoteItemTexts(null, Cut(name, QuoteLimits.NameMaxLength),
            $"{kind} · {EmbroiderySavedCalculation.NumberOf(saved.Id)}", null, null, null,
            null, null);
    }

    private static QuoteItemOption Option(EmbroiderySavedCalculation saved, EmbroideryPrice price)
    {
        var shown = new QuotePrice(price.Unit, null, true);
        var uk = CultureInfo.GetCultureInfo("uk-UA");
        var repeat = $"Повтор з готовою програмою — {price.RepeatUnit.ToString("N2", uk)} грн за 1 шт.";
        return new QuoteItemOption(0, null, null, EmbroideryCategories.Label(price.Name), repeat, false, shown, shown, shown, saved.CreatedAt);
    }

    private static string Cut(string text, int max) => text.Length <= max ? text : text[..max];
}
