namespace Crm.Api.Customers.Requests;

/// <param name="DryRun">Preview only; true by default so a bare call never writes.</param>
/// <param name="RemoveUnlinked">One-time transfer: delete Brevi clients without a KeepinCRM link, hide those quotes use.</param>
public sealed record SyncKeepinCrmClientsRequest(bool DryRun = true, bool RemoveUnlinked = false);
