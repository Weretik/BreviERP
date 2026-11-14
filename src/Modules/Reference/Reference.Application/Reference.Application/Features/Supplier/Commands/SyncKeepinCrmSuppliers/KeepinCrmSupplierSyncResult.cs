namespace Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;

/// <param name="Received">Suppliers returned by KeepinCRM.</param>
/// <param name="Created">New Brevi suppliers created from KeepinCRM.</param>
/// <param name="Linked">Existing manual Brevi suppliers linked to KeepinCRM by name.</param>
/// <param name="Updated">Already linked suppliers refreshed from KeepinCRM.</param>
/// <param name="MissingInKeepinCrm">Linked Brevi suppliers that KeepinCRM no longer returns; they are kept.</param>
public sealed record KeepinCrmSupplierSyncResult(
    int Received,
    int Created,
    int Linked,
    int Updated,
    IReadOnlyList<string> MissingInKeepinCrm,
    DateTimeOffset SyncedAt);
