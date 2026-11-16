using ActivityLog.Contracts.Activity;
using Crm.Application.Features.Activity.EventHandlers;
using Crm.Domain.Embroidery;

namespace UnitTests.Crm.Activity;

/// <summary>«Ціна вишивки» actions read as «Журнал дій» entries (ACT-001).</summary>
public sealed class EmbroideryActivityTests
{
    private readonly EmbroideryActivity _describer = new();

    [Test]
    public void A_deleted_calculation_is_listed_with_its_number()
    {
        var entry = _describer.Describe(new EmbroideryCalculationDeleted(42, ""));

        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Deleted));
        Assert.That(entry.SubjectTitle, Is.EqualTo("ВШ-0042 · Без назви"));
    }

    [Test]
    public void Saved_rates_name_the_new_version()
    {
        var entry = _describer.Describe(new EmbroiderySettingsChanged(3));

        Assert.That(entry!.Summary, Is.EqualTo("Збережено версію 3"));
        Assert.That(entry.SubjectType, Is.EqualTo(EmbroideryActivity.SettingsSubject));
    }
}
