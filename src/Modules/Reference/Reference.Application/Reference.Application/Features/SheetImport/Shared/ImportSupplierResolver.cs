using Reference.Domain.Suppliers;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;

namespace Reference.Application.Features.SheetImport.Shared;

/// <summary>
/// Finds the supplier named in a sheet row the same way the KeepinCRM sync links names (case, punctuation and
/// ТОВ/ФОП ignored). An unknown name becomes a new manual supplier, which the next KeepinCRM sync can link.
/// An empty name means «Не вказано».
/// </summary>
internal sealed class ImportSupplierResolver
{
    public const string UnspecifiedName = "Не вказано";
    private static readonly string[] UnspecifiedKeys = ["не вказано", "не указан", "не указано"];

    private readonly Dictionary<string, SupplierEntity> _byKey;
    private readonly List<SupplierEntity> _created = [];
    private int _nextId;

    public ImportSupplierResolver(IReadOnlyCollection<SupplierEntity> suppliers)
    {
        _byKey = suppliers
            .GroupBy(x => SupplierNameKey.From(x.Name))
            .ToDictionary(group => group.Key, group => group.OrderBy(x => x.Id.Value).First());
        _nextId = suppliers.Count == 0 ? 1 : suppliers.Max(x => x.Id.Value) + 1;
    }

    /// <summary>Suppliers that do not exist yet; add them before saving the rows that use them.</summary>
    public IReadOnlyList<SupplierEntity> Created => _created;

    public SupplierEntity Resolve(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            var unspecified = UnspecifiedKeys.Select(key => _byKey.GetValueOrDefault(key)).FirstOrDefault(x => x is not null);
            return unspecified ?? Create(UnspecifiedName);
        }

        return _byKey.GetValueOrDefault(SupplierNameKey.From(name)) ?? Create(name.Trim());
    }

    private SupplierEntity Create(string name)
    {
        // Resolve never passes a blank name, the only rule a new supplier can break here.
        var supplier = SupplierEntity.Create(SupplierId.From(_nextId++), name).Value;
        _created.Add(supplier);
        _byKey[SupplierNameKey.From(name)] = supplier;
        return supplier;
    }
}
