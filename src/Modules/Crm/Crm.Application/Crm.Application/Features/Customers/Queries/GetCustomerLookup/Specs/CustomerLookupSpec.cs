using Crm.Domain.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomerLookup.Specs;

internal class CustomerLookupSpec : Specification<Customer>
{
    public CustomerLookupSpec(GetCustomerLookupQuery query, bool paged) => Filter(Query, query, paged);

    internal static void Filter(ISpecificationBuilder<Customer> builder, GetCustomerLookupQuery query, bool paged)
    {
        builder.AsNoTracking().Where(x => x.MergedIntoId == null);
        if (query.Ids is { } ids)
        {
            var wanted = ids.ToList();
            builder.Where(x => wanted.Contains(x.PublicId));
        }
        else
        {
            if (query.ActiveOnly) builder.Where(x => x.Active);
            var term = string.IsNullOrWhiteSpace(query.Search) ? null : CustomerText.Normalize(query.Search);
            if (term != null)
                builder.Where(x => x.NormalizedName.Contains(term)
                    || (x.TaxIdentity != null && x.TaxIdentity.ToUpper().Contains(term)));
        }

        if (!paged) return;
        builder.OrderBy(x => x.DisplayName).ThenBy(x => x.PublicId)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);
    }
}
