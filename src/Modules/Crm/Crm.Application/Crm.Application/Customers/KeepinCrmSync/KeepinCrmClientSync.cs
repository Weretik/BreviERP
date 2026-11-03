using Crm.Application.Customers.KeepinCrmSync.DTOs;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Application.Customers.KeepinCrmSync;

/// <summary>
/// Brings Brevi clients in line with KeepinCRM, like the Calculator did: a client is matched only by its KeepinCRM ID,
/// new ones are created, a linked client missing from KeepinCRM is marked not used, and nothing is written back.
/// KeepinCRM owns the name, kind, active flag and the «KeepinCRM» contact (person, phones, e-mails).
/// With <c>removeUnlinked</c> (the one-time transfer) Brevi clients without a KeepinCRM link are deleted,
/// except those that quotes or custom sewing use: these are hidden. A preview saves nothing.
/// </summary>
public sealed class KeepinCrmClientSync(
    IKeepinCrmClientSource source,
    ICustomerKeepinSyncStore store,
    KeepinCrmClientSyncSettings settings,
    TimeProvider timeProvider,
    ILogger<KeepinCrmClientSync> logger)
{
    public async Task<KeepinCrmClientSyncResult> RunAsync(bool dryRun, bool removeUnlinked, CancellationToken cancellationToken)
    {
        var snapshots = await source.GetAllAsync(cancellationToken);
        if (snapshots.Count == 0)
            throw new KeepinCrmClientsUnavailableException("KeepinCRM не повернув клієнтів. Дані в Brevi не змінено.");

        var customers = await store.ListWithDetailsAsync(cancellationToken);
        var linked = new Dictionary<string, Customer>();
        var unlinked = new List<Customer>();
        foreach (var customer in customers)
        {
            var id = KeepinCrmClientIdentity.IdOf(customer);
            if (id is null) unlinked.Add(customer);
            else linked.TryAdd(id, customer);
        }

        var seen = new HashSet<string>();
        int created = 0, updated = 0, unchanged = 0;
        foreach (var snapshot in snapshots.Where(x => seen.Add(x.KeepinCrmId)))
        {
            var target = KeepinCrmClientTarget.From(snapshot);
            if (linked.TryGetValue(snapshot.KeepinCrmId, out var existing))
            {
                if (Apply(existing, target)) updated++;
                else unchanged++;
                continue;
            }

            var customer = Customer.Create(target.Kind, target.Name, null, null, settings.Owner);
            customer.AddExternalIdentity(KeepinCrmClientIdentity.Create(snapshot.KeepinCrmId));
            if (!target.Active) customer.Revise(customer.Revision, target.Kind, target.Name, null, null, false);
            customer.SaveContact(customer.Revision, null, target.ContactName, KeepinCrmClientIdentity.ContactPosition,
                true, true, target.Channels());
            store.Add(customer);
            created++;
        }

        var deactivated = new List<string>();
        foreach (var (id, customer) in linked.Where(pair => !seen.Contains(pair.Key)))
        {
            if (!customer.Active) continue;
            Deactivate(customer);
            deactivated.Add($"{customer.DisplayName} (KeepinCRM {id})");
        }

        var deleted = new List<string>();
        var hidden = new List<string>();
        if (removeUnlinked && unlinked.Count > 0)
        {
            var inUse = await store.ListInUseAsync(cancellationToken);
            foreach (var customer in unlinked)
            {
                if (inUse.Contains(customer.Id))
                {
                    if (customer.Active) Deactivate(customer);
                    hidden.Add(customer.DisplayName);
                }
                else
                {
                    store.Remove(customer);
                    deleted.Add(customer.DisplayName);
                }
            }
        }

        var syncedAt = timeProvider.GetUtcNow();
        if (!dryRun)
            await store.SaveAsync(syncedAt, cancellationToken);

        var result = new KeepinCrmClientSyncResult(!dryRun, snapshots.Count, created, updated, unchanged,
            deactivated, deleted, hidden, dryRun ? null : syncedAt);
        logger.LogInformation(
            "KeepinCRM clients sync (applied {Applied}): received {Received}, created {Created}, updated {Updated}, unchanged {Unchanged}, deactivated {Deactivated}, deleted {Deleted}, hidden in use {Hidden}",
            result.Applied, result.Received, created, updated, unchanged, deactivated.Count, deleted.Count, hidden.Count);
        return result;
    }

    private static void Deactivate(Customer customer) =>
        customer.Revise(customer.Revision, customer.Kind, customer.DisplayName, customer.LegalName, customer.TaxIdentity, false);

    /// <summary>Updates a linked client; true when anything changed. Brevi-owned legal name and tax ID stay.</summary>
    private static bool Apply(Customer customer, KeepinCrmClientTarget target)
    {
        var changed = false;
        if (customer.Kind != target.Kind || customer.DisplayName != target.Name || customer.Active != target.Active)
        {
            customer.Revise(customer.Revision, target.Kind, target.Name, customer.LegalName, customer.TaxIdentity, target.Active);
            changed = true;
        }

        var contact = customer.Contacts.FirstOrDefault(x => x.Position == KeepinCrmClientIdentity.ContactPosition);
        var channels = target.Channels();
        if (contact is null || !SameContact(contact, target.ContactName, channels))
        {
            var primary = contact?.IsPrimary ?? !customer.Contacts.Any(x => x.Active && x.IsPrimary);
            customer.SaveContact(customer.Revision, contact?.PublicId, target.ContactName,
                KeepinCrmClientIdentity.ContactPosition, true, primary, channels);
            changed = true;
        }

        return changed;
    }

    private static bool SameContact(CustomerContact contact, string name, IReadOnlyList<CustomerChannel> channels) =>
        contact.Active
        && contact.Name == name
        && contact.Channels.Select(Key).Order().SequenceEqual(channels.Select(Key).Order());

    private static string Key(CustomerChannel channel) => $"{channel.Kind}:{channel.NormalizedValue}";
}
