using Ardalis.Specification;
using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using Crm.Application.Abstractions.Customers;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients.Results;
namespace Crm.Application.Features.Customers.Queries.GetCustomers;

public sealed class CustomerPageSpec : Specification<Customer, CustomerSummary>
{
    public CustomerPageSpec(CrmActor actor, string? search, bool? active, int page, int pageSize)
    {
        var term = string.IsNullOrWhiteSpace(search) ? null : CustomerText.Normalize(search);
        var digits = search == null ? null : new string(search.Where(char.IsDigit).ToArray());
        Query.AsNoTracking().Where(x => x.MergedIntoId == null && (actor.IsAdmin || x.OwnerId == new Crm.Domain.Customers.Identifiers.CrmUserId(actor.UserId)));
        if (active.HasValue) Query.Where(x => x.Active == active.Value);
        if (term != null) Query.Where(x => x.NormalizedName.Contains(term) || (x.TaxIdentity != null && x.TaxIdentity.ToUpper().Contains(term))
            || x.Contacts.Any(c => c.Channels.Any(v => (v.NormalizedValue.Contains(term) || (v.Kind == "phone" && digits != null && digits.Length >= 5 && v.NormalizedValue.Contains(digits))))));
        Query.OrderByDescending(x => x.UpdatedAt).ThenBy(x => x.PublicId).Skip((page - 1) * pageSize).Take(pageSize);
        Query.Select(x => new CustomerSummary(x.PublicId, x.Kind, x.DisplayName, x.LegalName, x.TaxIdentity,
            x.OwnerId.Value, x.Active, x.Revision, x.UpdatedAt,
            x.ExternalIdentities.Where(i => i.Source == KeepinCrmClientIdentity.Source && i.Type == KeepinCrmClientIdentity.Type)
                .Select(i => i.ExternalId).FirstOrDefault(),
            x.Contacts.Where(c => c.Active).OrderByDescending(c => c.IsPrimary).Select(c => c.Name).FirstOrDefault(),
            x.Contacts.Where(c => c.Active).SelectMany(c => c.Channels).Where(v => v.Kind == "phone").Select(v => v.Value).ToList(),
            x.KeepinSyncedAt));
    }
}
