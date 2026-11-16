using ActivityLog.Contracts.Activity;
using Production.Domain.FabricNorms;

namespace Production.Application.Features.Activity.EventHandlers;

/// <summary>How fabric norms of a model read in «Журнал дій» (ACT-001): meters per colour variant.</summary>
public sealed class FabricNormActivity : IActivityDescriber
{
    public const string Subject = "fabric-norm";

    public ActivityDescription? Describe(object domainEvent) => domainEvent switch
    {
        FabricNormSaved { Before: null } e => Of(ActivityActions.Created, e.Norm.Id, e.Norm.ModelCode,
            Changes(new FabricNormValues(null, null, null, null, null, null), e.Norm.Values).Items),
        FabricNormSaved e => Of(ActivityActions.Changed, e.Norm.Id, e.Norm.ModelCode, Changes(e.Before!, e.Norm.Values).Items),
        FabricNormDeleted e => Of(ActivityActions.Deleted, e.NormId, e.ModelCode, []),
        _ => null,
    };

    private static ActivityChanges Changes(FabricNormValues before, FabricNormValues after) => new ActivityChanges()
        .Add("Один колір", before.OneColor, after.OneColor, "м")
        .Add("Два кольори: основний", before.TwoColorsMain, after.TwoColorsMain, "м")
        .Add("Два кольори: додатковий", before.TwoColorsAdditional, after.TwoColorsAdditional, "м")
        .Add("Три кольори: основний", before.ThreeColorsMain, after.ThreeColorsMain, "м")
        .Add("Три кольори: додатковий 1", before.ThreeColorsAdditional1, after.ThreeColorsAdditional1, "м")
        .Add("Три кольори: додатковий 2", before.ThreeColorsAdditional2, after.ThreeColorsAdditional2, "м");

    private static ActivityDescription Of(string action, int id, string code, IReadOnlyList<ActivityChangeItem> changes) =>
        new(action, Subject, "Норма тканини", id.ToString(CultureInfo.InvariantCulture), "Модель " + code, null, changes,
            "Production");
}
