using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing;
using Crm.Domain.CustomSewing.Templates;
namespace UnitTests.Crm.CustomSewing;

public sealed class CustomTemplateRulesTests
{
    private static TemplateSelections Selections => new("Куртка", "Куртка", "winter", "Public description", "Comment",
        [new(1, 2, "Meters")], [], [new(2, 1, null)], [new("print", "Front")]);
    [Test]
    public void Template_selection_freezes_norms_and_builds_only_unpriced_editable_draft_inputs()
    {
        var fabrics = Selections.Fabrics.ToList();
        var selection = (Selections with { Fabrics = fabrics }).Freeze();
        fabrics.Clear();
        var draft = selection.ToDraft().Freeze();
        Assert.Multiple(() => {
            Assert.That(draft.Fabrics.Single().Quantity, Is.EqualTo(2));
            Assert.That(draft.InternalNote, Is.Empty);
            Assert.That(draft.Delivery, Is.Zero);
            Assert.That(draft.Packaging, Is.Zero);
            Assert.That(draft.Applications.Single().Cost, Is.Zero);
            Assert.That(draft.Applications.Single().SaleApproved, Is.False);
            Assert.That(draft.Applications.Single().ArtworkFileId, Is.Null);
        });
    }
    [Test]
    public void Malformed_norms_cannot_be_frozen_as_reusable_templates()
    {
        Assert.Throws<CustomSewingValidationException>(() => (Selections with { Fabrics = [new(0, 2, "Meters")] }).Freeze());
        Assert.Throws<CustomSewingValidationException>(() => (Selections with { Applications = [null!] }).Freeze());
        Assert.Throws<CustomSewingValidationException>(() => (Selections with { Season = "ambiguous" }).Freeze());
    }
    [Test]
    public void Archive_checks_revision_before_mutation_and_allows_explicit_restore()
    {
        var template = CustomTemplate.Create(new CrmUserId(17));
        Assert.Throws<CustomSewingRevisionException>(() => template.SetArchived(99, true));
        Assert.That(template.Archived, Is.False);
        template.SetArchived(1, true);
        template.SetArchived(2, false);
        Assert.That(template.Revision, Is.EqualTo(3));
        Assert.That(template.Archived, Is.False);
    }
}
