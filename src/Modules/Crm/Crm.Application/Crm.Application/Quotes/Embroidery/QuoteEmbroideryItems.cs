using System.Globalization;
using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Embroidery;

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
            $"{kind} · {EmbroiderySavedCalculation.NumberOf(saved.Id)}", Description(saved), null, null,
            null, null);
    }

    /// <summary>«120 шт · 8 000 стібків на 1 шт · п’яльця 18 см.»; the manager can change it in the КП.</summary>
    public static string Description(EmbroiderySavedCalculation saved)
    {
        var uk = CultureInfo.GetCultureInfo("uk-UA");
        var hoop = saved.Snapshot.Settings.Hoops.FirstOrDefault(x => x.Id == saved.Input.HoopId)?.Label
                   ?? saved.Input.HoopId;
        var stitches = saved.Input.StitchesPerUnit.ToString("N0", uk);
        return $"{saved.Quantity.ToString("N0", uk)} шт · {stitches} стібків на 1 шт · п’яльця {hoop}.";
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
