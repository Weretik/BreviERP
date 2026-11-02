namespace Crm.Application.Customers.KeepinCrmSync.DTOs;

/// <summary>Outcome of «Оновити з KeepinCRM» for clients. <paramref name="Applied"/> is false for a preview.</summary>
/// <param name="Deactivated">Linked clients missing from KeepinCRM: marked not used, never deleted.</param>
/// <param name="Deleted">Brevi clients without a KeepinCRM link that were deleted (only with removeUnlinked).</param>
/// <param name="HiddenInUse">Brevi clients without a KeepinCRM link that quotes use: hidden instead of deleted.</param>
public sealed record KeepinCrmClientSyncResult(
    bool Applied,
    int Received,
    int Created,
    int Updated,
    int Unchanged,
    IReadOnlyList<string> Deactivated,
    IReadOnlyList<string> Deleted,
    IReadOnlyList<string> HiddenInUse);
