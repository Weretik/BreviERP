using Crm.Domain.Customers;
using Crm.Domain.Customers.Identifiers;
using Crm.Application.Features.Customers.Queries.GetCustomerLookup;
using Crm.Application.Features.Customers.Queries.GetCustomersByTaxIds;
using Crm.Infrastructure.Persistence.Repositories.Customers;

namespace IntegrationTests.Crm;

/// <summary>Active clients by ЄДРПОУ/ІПН for the bank statement import of «Бухгалтерія» (ACC-001 stage 6).</summary>
[NonParallelizable]
public sealed class CustomerTaxIdLookupTests : CrmDatabaseFixture
{
    [Test]
    public async Task Only_active_clients_with_one_of_the_codes_are_found()
    {
        Guid wanted;
        await using (var db = Context())
        {
            var first = Customer.Create("organization", "ТОВ «Агро»", null, "12345678", new CrmUserId(1)).Value;
            var archived = Customer.Create("organization", "ФОП Старий", null, "87654321", new CrmUserId(1)).Value;
            archived.Revise(archived.Revision, archived.Kind, archived.DisplayName, null, "87654321", active: false);
            var other = Customer.Create("organization", "Інший код", null, "11112222", new CrmUserId(1)).Value;
            db.Customers.AddRange(first, archived, other);
            await db.SaveChangesAsync();
            wanted = first.PublicId;
        }

        await using var read = Context();
        var rows = await new GetCustomersByTaxIdsHandler(new CustomerReadRepository(read))
            .Handle(new GetCustomersByTaxIdsQuery(["12345678", "87654321", "99999999"]), default);

        Assert.That(rows, Is.EqualTo(new[] { new CustomerLookup(wanted, "ТОВ «Агро»", "12345678", true) }));
    }
}
