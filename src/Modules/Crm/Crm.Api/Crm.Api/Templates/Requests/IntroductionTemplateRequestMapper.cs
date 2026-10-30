using Crm.Domain.Quotes.Introductions;

namespace Crm.Api.Templates.Requests;

public static class IntroductionTemplateRequestMapper
{
    public static bool TryToDomain(this IntroductionTemplateWriteRequest request, out IntroductionLayout layout, out IReadOnlyList<IntroductionSelection> selections)
    {
        layout = request.Layout switch
        {
            "general" => IntroductionLayout.General,
            "summer" => IntroductionLayout.Summer,
            "winter" => IntroductionLayout.Winter,
            _ => (IntroductionLayout)(-1)
        };
        selections = [];
        if (!Enum.IsDefined(layout) || request.Selections is null || request.Selections.Count is < 4 or > 8 ||
            request.Selections.Any(x => x is null)) return false;
        var mapped = new List<IntroductionSelection>(request.Selections.Count);
        foreach (var item in request.Selections)
        {
            var season = (item.SlotUid / 100) switch
            {
                1 => IntroductionSeason.Summer,
                2 => IntroductionSeason.Winter,
                _ => (IntroductionSeason)(-1)
            };
            if (!Enum.IsDefined(season) || item.SlotUid % 100 is < 0 or > 3 ||
                item.ProductId <= 0 || item.ArticleId <= 0) return false;
            mapped.Add(new IntroductionSelection(
                new IntroductionSlot(season, item.SlotUid % 100), item.ProductId, item.ArticleId));
        }
        selections = mapped;
        return true;
    }
}
