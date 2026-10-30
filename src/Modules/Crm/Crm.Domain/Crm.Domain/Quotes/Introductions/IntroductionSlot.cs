namespace Crm.Domain.Quotes.Introductions;

public readonly record struct IntroductionSlot(IntroductionSeason Season, int Position)
{
    public int Uid => (Season == IntroductionSeason.Summer ? 100 : 200) + Position;

    public static IReadOnlyList<IntroductionSlot> Required(IntroductionLayout layout)
    {
        if (!Enum.IsDefined(layout))
            throw new IntroductionValidationException("layout_invalid");
        var seasons = layout switch
        {
            IntroductionLayout.General => new[] { IntroductionSeason.Summer, IntroductionSeason.Winter },
            IntroductionLayout.Summer => new[] { IntroductionSeason.Summer },
            _ => new[] { IntroductionSeason.Winter }
        };
        return seasons.SelectMany(season => Enumerable.Range(0, 4)
            .Select(position => new IntroductionSlot(season, position))).ToArray();
    }
}
