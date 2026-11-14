using Reference.Application.Contracts.KeepinCrm;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Supplier.KeepinCrmSync.DTOs;
using Reference.Domain.Suppliers;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;

namespace Reference.Application.Features.Supplier.KeepinCrmSync;

/// <summary>
/// Brings Brevi suppliers in line with KeepinCRM, like the old «Оновити постачальників» sheet script:
/// a linked supplier is refreshed by KeepinCRM ID; an unlinked one with the same name is linked; the rest
/// are created. Activity and notes stay as set in Brevi. Nothing is deleted. One save for the whole run.
/// </summary>
public sealed class KeepinCrmSupplierSync(
    IKeepinCrmSupplierSource source,
    IReferenceRepository<SupplierEntity> repository,
    TimeProvider timeProvider,
    ILogger<KeepinCrmSupplierSync> logger)
{
    public async Task<KeepinCrmSupplierSyncResult> RunAsync(CancellationToken cancellationToken)
    {
        var snapshots = await source.GetAllAsync(cancellationToken);
        if (snapshots.Count == 0)
            throw new KeepinCrmUnavailableException("KeepinCRM не повернув постачальників. Дані в Brevi не змінено.");

        var syncedAt = timeProvider.GetUtcNow();
        var suppliers = await repository.ListAsync(cancellationToken);
        var byKeepinId = suppliers.Where(x => x.KeepinCrmId is not null).ToDictionary(x => x.KeepinCrmId!);
        var unlinkedByName = suppliers.Where(x => x.KeepinCrmId is null)
            .GroupBy(x => SupplierNameKey.From(x.Name))
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single());
        var nextId = suppliers.Count == 0 ? 1 : suppliers.Max(x => x.Id.Value) + 1;
        var created = new List<SupplierEntity>();
        var seen = new HashSet<string>();
        int linked = 0, updated = 0;

        foreach (var snapshot in snapshots.Where(x => seen.Add(x.KeepinCrmId)))
        {
            if (string.IsNullOrWhiteSpace(snapshot.KeepinCrmId) || string.IsNullOrWhiteSpace(snapshot.Name))
            {
                // A supplier without an ID or a name cannot be kept in Brevi: it is skipped until fixed in KeepinCRM.
                logger.LogWarning("KeepinCRM supplier {KeepinCrmId} has no ID or name and was skipped", snapshot.KeepinCrmId);
                continue;
            }

            if (byKeepinId.TryGetValue(snapshot.KeepinCrmId, out var existing))
            {
                existing.ApplyKeepinCrmSnapshot(WithFreeName(snapshot, existing, suppliers, created), syncedAt)
                    .ThrowIfFailure();
                updated++;
            }
            else if (unlinkedByName.Remove(SupplierNameKey.From(snapshot.Name), out var manual))
            {
                manual.ApplyKeepinCrmSnapshot(WithFreeName(snapshot, manual, suppliers, created), syncedAt)
                    .ThrowIfFailure();
                linked++;
            }
            else
            {
                var supplier = SupplierEntity.CreateFromKeepinCrm(
                    SupplierId.From(nextId++), WithFreeName(snapshot, null, suppliers, created), syncedAt).Value;
                created.Add(supplier);
            }
        }

        var missing = byKeepinId.Values.Where(x => !seen.Contains(x.KeepinCrmId!)).Select(x => x.Name).ToList();
        if (created.Count > 0)
            await repository.AddRangeAsync(created, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var result = new KeepinCrmSupplierSyncResult(snapshots.Count, created.Count, linked, updated, missing, syncedAt);
        logger.LogInformation(
            "KeepinCRM suppliers sync: received {Received}, created {Created}, linked {Linked}, updated {Updated}, missing in KeepinCRM {Missing}",
            result.Received, result.Created, result.Linked, result.Updated, missing.Count);
        return result;
    }

    /// <summary>Supplier names are unique in Brevi; a clash with another supplier gets the KeepinCRM ID appended.</summary>
    private static KeepinCrmSupplierSnapshot WithFreeName(
        KeepinCrmSupplierSnapshot snapshot,
        SupplierEntity? target,
        IReadOnlyCollection<SupplierEntity> suppliers,
        IReadOnlyCollection<SupplierEntity> created)
    {
        var name = snapshot.Name.Trim();
        var taken = suppliers.Concat(created)
            .Any(x => !ReferenceEquals(x, target) && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        return taken ? snapshot with { Name = $"{name} (KeepinCRM {snapshot.KeepinCrmId})" } : snapshot;
    }
}
