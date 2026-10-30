using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Drafts;

namespace UnitTests.Crm.CustomSewing;

public sealed class CustomSewingDraftTests
{
    [Test]
    public void Template_origin_is_fixed_once_and_survives_independent_edits()
    {
        var draft = CustomSewingDraft.Create(new CrmUserId(17), null, null, Configuration);
        var origin = new CustomTemplateOrigin(Guid.NewGuid(), Guid.NewGuid(), "Вступ із шаблону");
        draft.SetTemplateOrigin(origin);
        Assert.Throws<CustomSewingValidationException>(() => draft.SetTemplateOrigin(origin));
        draft.Revise(1, Configuration with { Name = "Independent" });
        Assert.That(draft.TemplateOrigin, Is.EqualTo(origin));
    }

    [Test]
    public void Template_origin_cannot_be_attached_after_editing()
    {
        var draft = CustomSewingDraft.Create(new CrmUserId(17), null, null, Configuration);
        draft.Revise(1, Configuration);
        Assert.Throws<CustomSewingValidationException>(() => draft.SetTemplateOrigin(
            new CustomTemplateOrigin(Guid.NewGuid(), Guid.NewGuid(), "Intro")));
    }

    private static CustomConfiguration Configuration => new("Synthetic", "Jacket", "winter", "Description", "Comment", "Private",
        [new(1, 1, "Meters")], [], [new(2, 1, null)], [], 0, 0);

    [Test]
    public void Stale_revision_does_not_overwrite_draft_or_increment_revision()
    {
        var draft = CustomSewingDraft.Create(new CrmUserId(17), null, null, Configuration);
        Assert.Throws<CustomSewingRevisionException>(() => draft.Revise(9, Configuration with { Name = "Wrong" }));
        Assert.That(draft.Name, Is.EqualTo("Synthetic"));
        Assert.That(draft.Revision, Is.EqualTo(1));
    }

    [Test]
    public void Invalid_revision_does_not_replace_previously_saved_configuration()
    {
        var draft = CustomSewingDraft.Create(new CrmUserId(17), null, null, Configuration);
        draft.Revise(1, Configuration with { Name = "Updated" });
        Assert.Throws<CustomSewingRevisionException>(() => draft.Revise(1, Configuration));
        Assert.That(draft.Name, Is.EqualTo("Updated"));
        Assert.That(draft.Revision, Is.EqualTo(2));
    }

    [Test]
    public void Draft_defensively_copies_mutable_composition_lists()
    {
        var fabrics = new List<CustomMaterial> { new(1, 1, "Meters") };
        var draft = CustomSewingDraft.Create(new CrmUserId(17), null, null, Configuration with { Fabrics = fabrics });
        fabrics.Clear();
        Assert.That(draft.Configuration.Fabrics, Has.Count.EqualTo(1));
    }

    [Test]
    public void Incomplete_composition_can_remain_an_editable_draft()
    {
        var draft = CustomSewingDraft.Create(new CrmUserId(17), null, null,
            Configuration with { Fabrics = [], Operations = [] });
        Assert.That(draft.Configuration.Fabrics, Is.Empty);
        draft.Revise(1, Configuration);
        Assert.That(draft.Revision, Is.EqualTo(2));
    }
}
