using Crm.Domain.Customers;
using Crm.Domain.Customers.Identifiers;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using Crm.Application.Features.Customers.Queries.GetCustomersByKeepinIds;
using Crm.Infrastructure.Persistence.Customers;

namespace IntegrationTests.Crm;

/// <summary>Clients by KeepinCRM id for the finance archive move of «Бухгалтерія» (ACC-001 stage 5).</summary>
[NonParallelizable]
public sealed class CustomerKeepinLookupTests : CrmDatabaseFixture
{
    [Test]
    public async Task Clients_are_found_by_their_KeepinCRM_ids_archived_too()
    {
        Guid active, archived;
        await using (var db = Context())
        {
            var first = Customer.Create("organization", "ТОВ «Агро»", null, "12345678", new CrmUserId(1)).Value;
            first.AddExternalIdentity(KeepinCrmClientIdentity.Create("100").Value);
            var second = Customer.Create("organization", "ФОП Старий", null, null, new CrmUserId(1)).Value;
            second.AddExternalIdentity(KeepinCrmClientIdentity.Create("200").Value);
            second.Revise(second.Revision, second.Kind, second.DisplayName, null, null, active: false);
            var other = Customer.Create("organization", "Без KeepinCRM", null, null, new CrmUserId(1)).Value;
            db.Customers.AddRange(first, second, other);
            await db.SaveChangesAsync();
            (active, archived) = (first.PublicId, second.PublicId);
        }

        await using var read = Context();
        var rows = await new GetCustomersByKeepinIdsHandler(new CustomerReadRepository(read))
            .Handle(new GetCustomersByKeepinIdsQuery(["100", "200", "300"]), default);

        Assert.That(rows.OrderBy(x => x.KeepinId), Is.EqualTo(new[]
        {
            new KeepinCustomerLookup("100", active, "ТОВ «Агро»", "12345678", true),
            new KeepinCustomerLookup("200", archived, "ФОП Старий", null, false),
        }));
    }
}
