using Reference.Application.Features.SheetImport.Shared;

using Reference.Application.Features.PricingSettingsImport.Shared;

namespace Reference.Application.Features.PricingSettingsImport.Commands.ImportPricingSettings;

/// <summary>Collects what an import would do to one table, then does it on <see cref="Apply"/>.</summary>
internal sealed class ImportPlan<TEntity> where TEntity : class
{
    private readonly List<ReferenceImportRowResult> _rows = [];
    private readonly List<TEntity> _matched = [];
    private readonly List<Action> _updates = [];

    public List<TEntity> Created { get; } = [];
    public List<TEntity> Deleted { get; private set; } = [];
    public PricingSettingsImportSection Section { get; private set; } = null!;

    public void Error(int rowNumber, string code, string name, string message) =>
        _rows.Add(new(rowNumber, code, name, ReferenceImportActions.Error, [], message));

    public void Create(int rowNumber, string code, string name, TEntity entity)
    {
        Created.Add(entity);
        _rows.Add(new(rowNumber, code, name, ReferenceImportActions.Create, [], null));
    }

    public void Match(int rowNumber, string code, string name, TEntity target, List<string> changes, Action update)
    {
        _matched.Add(target);
        if (changes.Count > 0)
            _updates.Add(update);
        _rows.Add(new(rowNumber, code, name,
            changes.Count == 0 ? ReferenceImportActions.Unchanged : ReferenceImportActions.Update, changes, null));
    }

    public void Finish(IEnumerable<TEntity> existing, bool deleteMissing, Func<TEntity, string> describe)
    {
        var matched = _matched.ToHashSet();
        Deleted = deleteMissing ? existing.Where(x => !matched.Contains(x)).ToList() : [];
        Section = new PricingSettingsImportSection(
            Count(ReferenceImportActions.Create),
            Count(ReferenceImportActions.Update),
            Count(ReferenceImportActions.Unchanged),
            Count(ReferenceImportActions.Error),
            Deleted.Select(describe).ToList(),
            _rows);
    }

    public void Apply()
    {
        foreach (var update in _updates)
            update();
    }

    private int Count(string action) => _rows.Count(x => x.Action == action);
}
