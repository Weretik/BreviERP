namespace Crm.Domain.Quotes.Introductions;

/// <summary>Reusable selection identities only; current prices and public metadata are resolved on each use.</summary>
public sealed class IntroductionSelectionSnapshot
{
    public IntroductionLayout Layout { get; }
    public Guid MappingVersionId { get; }
    public Guid EmailTemplateVersionId { get; }
    public IReadOnlyList<IntroductionSelection> Selections { get; }

    private IntroductionSelectionSnapshot(IntroductionLayout layout, Guid mappingVersionId,
        Guid emailTemplateVersionId, IReadOnlyList<IntroductionSelection> selections)
    {
        Layout = layout;
        MappingVersionId = mappingVersionId;
        EmailTemplateVersionId = emailTemplateVersionId;
        Selections = selections;
    }

    public static IntroductionSelectionSnapshot Create(IntroductionLayout layout,
        FeaturedModelMapping mapping, Guid emailTemplateVersionId,
        IEnumerable<IntroductionSelection> selections)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(selections);
        if (mapping.VersionId == Guid.Empty || mapping.SummerProductId <= 0 ||
            mapping.WinterProductId <= 0 || emailTemplateVersionId == Guid.Empty)
            throw new IntroductionValidationException("configuration_missing");

        var selected = selections.ToArray();
        var required = IntroductionSlot.Required(layout);
        if (selected.Length != required.Count ||
            selected.Select(x => x.Slot).Distinct().Count() != selected.Length ||
            selected.Any(x => x.ProductId <= 0 || x.ArticleId <= 0 ||
                !Enum.IsDefined(x.Slot.Season) || x.Slot.Position is < 0 or > 3) ||
            !selected.Select(x => x.Slot).OrderBy(x => x.Uid)
                .SequenceEqual(required.OrderBy(x => x.Uid)))
            throw new IntroductionValidationException("slots_invalid");

        foreach (var season in required.Select(x => x.Season).Distinct())
        {
            var page = selected.Where(x => x.Slot.Season == season).ToArray();
            if (page.Select(x => x.ProductId).Distinct().Count() != 4)
                throw new IntroductionValidationException("models_duplicate");
            if (page.Single(x => x.Slot.Position == 0).ProductId != mapping.For(season))
                throw new IntroductionValidationException("featured_mapping_mismatch");
        }

        return new IntroductionSelectionSnapshot(layout, mapping.VersionId,
            emailTemplateVersionId, Array.AsReadOnly(selected.OrderBy(x => x.Slot.Uid).ToArray()));
    }
}
