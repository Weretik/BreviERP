using Ardalis.Specification;
using Crm.Domain.Customers;
using Crm.Application.Abstractions.Customers;
namespace Crm.Application.Features.Customers.Queries.GetCustomers;

public sealed class CustomerCountSpec : Specification<Customer>
{
    public CustomerCountSpec(CrmActor actor, string? search, bool? active, int page, int pageSize)
    {
        var digits = search == null ? null : new string(search.Where(char.IsDigit).ToArray());
        Query.AsNoTracking().Where(x => x.MergedIntoId == null && (actor.IsAdmin || x.OwnerId == new BuildingBlocks.Domain.Identifiers.People.UserId(actor.UserId)));
        if (active.HasValue) Query.Where(x => x.Active == active.Value);
        foreach (var pattern in SearchWords.Patterns(search))
            Query.Where(x => EF.Functions.Like(EF.Property<string>(x, SearchWords.Column), pattern, SearchWords.Escape) || x.Contacts.Any(c => c.Channels.Any(v =>
                EF.Functions.Like(EF.Property<string>(v, SearchWords.Column), pattern, SearchWords.Escape) ||
                (v.Kind == "phone" && digits != null && digits.Length >= 5 && v.NormalizedValue.Contains(digits)))));
    }
}
