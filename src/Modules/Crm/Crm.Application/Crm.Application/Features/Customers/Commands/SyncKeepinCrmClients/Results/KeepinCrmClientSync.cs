using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.Identifiers;
using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.KeepinCrm;

namespace Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients.Results;

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
                var applied = Apply(existing, target);
                if (applied.IsFailure) Skip(snapshot.KeepinCrmId, applied.Error!);
                if (applied.IsSuccess && applied.Value) updated++;
                else unchanged++;
                continue;
            }

            var customer = NewCustomer(snapshot.KeepinCrmId, target);
            if (customer.IsFailure)
            {
                Skip(snapshot.KeepinCrmId, customer.Error!);
                continue;
            }

            store.Add(customer.Value);
            created++;
        }

        var deactivated = new List<string>();
        foreach (var (id, customer) in linked.Where(pair => !seen.Contains(pair.Key)))
        {
            if (!customer.Active) continue;
            if (!Deactivate(customer, id)) continue;
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
                    if (customer.Active) Deactivate(customer, customer.DisplayName);
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

    /// <summary>A client KeepinCRM data breaks a Brevi rule for is left as it is and logged; the rest still syncs.</summary>
    private void Skip(string keepinCrmId, Error error) =>
        logger.LogWarning("KeepinCRM client {KeepinCrmId} skipped: {Code}", keepinCrmId, error.Code);

    private bool Deactivate(Customer customer, string label)
    {
        var revised = customer.Revise(customer.Revision, customer.Kind, customer.DisplayName, customer.LegalName,
            customer.TaxIdentity, false);
        if (revised.IsFailure) Skip(label, revised.Error!);
        return revised.IsSuccess;
    }

    private DomainResult<Customer> NewCustomer(string keepinCrmId, KeepinCrmClientTarget target)
    {
        var created = Customer.Create(target.Kind, target.Name, null, null, settings.Owner);
        if (created.IsFailure) return created;
        var customer = created.Value;
        var identity = KeepinCrmClientIdentity.Create(keepinCrmId);
        if (identity.IsFailure) return identity.Error!;
        var steps = DomainResult.Combine(
            customer.AddExternalIdentity(identity.Value),
            target.Active
                ? DomainResult.Success()
                : customer.Revise(customer.Revision, target.Kind, target.Name, null, null, false));
        if (steps.IsFailure) return steps.Error!;
        var contact = customer.SaveContact(customer.Revision, null, target.ContactName,
            KeepinCrmClientIdentity.ContactPosition, true, true, target.Channels());
        return contact.IsFailure ? contact.Error! : customer;
    }

    /// <summary>Updates a linked client; true when anything changed. Brevi-owned legal name and tax ID stay.</summary>
    private static DomainResult<bool> Apply(Customer customer, KeepinCrmClientTarget target)
    {
        var changed = false;
        if (customer.Kind != target.Kind || customer.DisplayName != target.Name || customer.Active != target.Active)
        {
            var revised = customer.Revise(customer.Revision, target.Kind, target.Name, customer.LegalName,
                customer.TaxIdentity, target.Active);
            if (revised.IsFailure) return revised.Error!;
            changed = true;
        }

        var contact = customer.Contacts.FirstOrDefault(x => x.Position == KeepinCrmClientIdentity.ContactPosition);
        var channels = target.Channels();
        if (contact is null || !SameContact(contact, target.ContactName, channels))
        {
            var primary = contact?.IsPrimary ?? !customer.Contacts.Any(x => x.Active && x.IsPrimary);
            var saved = customer.SaveContact(customer.Revision, contact?.PublicId, target.ContactName,
                KeepinCrmClientIdentity.ContactPosition, true, primary, channels);
            if (saved.IsFailure) return saved.Error!;
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
