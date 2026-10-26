using Ardalis.Specification;
using Crm.Application.Customers.Models;
using Crm.Domain.Customers;
namespace Crm.Application.Customers.GetCustomers.Specifications;

public sealed class CustomerCountSpec : Specification<Customer>
{
    public CustomerCountSpec(CrmActor actor, string? search, bool? active, int page, int pageSize)
    {
        var term = string.IsNullOrWhiteSpace(search) ? null : CustomerText.Normalize(search);
        var digits = search == null ? null : new string(search.Where(char.IsDigit).ToArray());
        Query.AsNoTracking().Where(x => x.MergedIntoId == null && (actor.IsAdmin || x.OwnerId == new Crm.Domain.Customers.Identifiers.CrmUserId(actor.UserId)));
        if (active.HasValue) Query.Where(x => x.Active == active.Value);
        if (term != null) Query.Where(x => x.NormalizedName.Contains(term) || (x.TaxIdentity != null && x.TaxIdentity.ToUpper().Contains(term))
            || x.Contacts.Any(c => c.Channels.Any(v => (v.NormalizedValue.Contains(term) || (v.Kind == "phone" && digits != null && digits.Length >= 5 && v.NormalizedValue.Contains(digits))))));
    }
}
