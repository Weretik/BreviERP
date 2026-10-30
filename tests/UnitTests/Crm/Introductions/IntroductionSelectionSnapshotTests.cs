using Crm.Domain.Quotes.Introductions;

namespace UnitTests.Crm.Introductions;

public sealed class IntroductionSelectionSnapshotTests
{
    private static readonly FeaturedModelMapping Mapping = new(Guid.NewGuid(), 101, 201);

    [TestCase(IntroductionLayout.General, 8)]
    [TestCase(IntroductionLayout.Summer, 4)]
    [TestCase(IntroductionLayout.Winter, 4)]
    public void Layout_requires_exact_featured_and_companion_slots(
        IntroductionLayout layout, int expectedCount)
    {
        var selections = IntroductionSlot.Required(layout).Select(slot =>
            new IntroductionSelection(slot,
                slot.Position == 0 ? Mapping.For(slot.Season) : slot.Uid + 10,
                slot.Uid + 1000));
        var snapshot = IntroductionSelectionSnapshot.Create(layout, Mapping,
            Guid.NewGuid(), selections);

        Assert.That(snapshot.Selections, Has.Count.EqualTo(expectedCount));
        Assert.That(snapshot.MappingVersionId, Is.EqualTo(Mapping.VersionId));
        Assert.That(snapshot.Selections.Select(x => x.Slot.Uid), Is.Ordered);
    }

    [Test]
    public void Missing_slot_duplicate_model_and_unmapped_featured_model_are_blocked()
    {
        var slots = IntroductionSlot.Required(IntroductionLayout.Summer);
        var valid = slots.Select(slot => new IntroductionSelection(slot,
            slot.Position == 0 ? Mapping.SummerProductId : slot.Uid + 10,
            slot.Uid + 1000)).ToArray();

        Assert.That(Assert.Throws<IntroductionValidationException>(() =>
            IntroductionSelectionSnapshot.Create(IntroductionLayout.Summer, Mapping,
                Guid.NewGuid(), valid.Skip(1)))!.Message, Is.EqualTo("slots_invalid"));
        Assert.That(Assert.Throws<IntroductionValidationException>(() =>
            IntroductionSelectionSnapshot.Create(IntroductionLayout.Summer, Mapping,
                Guid.NewGuid(), valid.Select((item, index) => index == 1
                    ? item with { ProductId = Mapping.SummerProductId } : item)))!.Message,
            Is.EqualTo("models_duplicate"));
        Assert.That(Assert.Throws<IntroductionValidationException>(() =>
            IntroductionSelectionSnapshot.Create(IntroductionLayout.Summer, Mapping,
                Guid.NewGuid(), valid.Select((item, index) => index == 0
                    ? item with { ProductId = 999 } : item)))!.Message,
            Is.EqualTo("featured_mapping_mismatch"));
    }

    [Test]
    public void Snapshot_contains_only_selection_and_version_ids()
    {
        var selections = IntroductionSlot.Required(IntroductionLayout.Summer)
            .Select(slot => new IntroductionSelection(slot,
                slot.Position == 0 ? Mapping.SummerProductId : slot.Uid + 10,
                slot.Uid + 1000));
        var snapshot = IntroductionSelectionSnapshot.Create(IntroductionLayout.Summer,
            Mapping, Guid.NewGuid(), selections);
        Assert.That(typeof(IntroductionSelectionSnapshot).GetProperties().Select(x => x.Name),
            Is.EquivalentTo(new[] { "Layout", "MappingVersionId", "EmailTemplateVersionId", "Selections" }));
        Assert.That(typeof(IntroductionSelection).GetProperties().Select(x => x.Name),
            Is.EquivalentTo(new[] { "Slot", "ProductId", "ArticleId" }));
        Assert.That(snapshot.Selections, Has.Count.EqualTo(4));
    }
}
