using System.Reflection;
using System.Text.Json.Nodes;
using Crm.Application.Customers.KeepinCrmSync;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Identifiers;
using Crm.Infrastructure.KeepinCrm;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests.Crm.Customers;

/// <summary>KeepinCRM → Brevi clients (CRM-KC): match by KeepinCRM ID, hide missing, transfer deletes unlinked.</summary>
[TestFixture]
public sealed class KeepinCrmClientSyncTests
{
    private static readonly CrmUserId Owner = new(1);

    private static KeepinCrmClientSnapshot Client(string id, string? company, string? person = null,
        string[]? phones = null, bool archived = false) =>
        new(id, company, person, phones ?? [], [], archived);

    private static Customer Linked(long id, string keepinId, string name)
    {
        var customer = Unlinked(id, name);
        customer.AddExternalIdentity(KeepinCrmClientIdentity.Create(keepinId));
        return customer;
    }

    private static Customer Unlinked(long id, string name)
    {
        var customer = Customer.Create("organization", name, null, null, Owner);
        typeof(Customer).GetProperty(nameof(Customer.Id))!.SetValue(customer, new CustomerId(id));
        return customer;
    }

    private sealed class Store(List<Customer> customers, params long[] inUse) : ICustomerKeepinSyncStore
    {
        public List<Customer> Added { get; } = [];
        public List<Customer> Removed { get; } = [];
        public int Saves { get; private set; }

        public Task<IReadOnlyList<Customer>> ListWithDetailsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Customer>>(customers);

        public Task<IReadOnlySet<CustomerId>> ListInUseAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlySet<CustomerId>>(inUse.Select(x => new CustomerId(x)).ToHashSet());

        public void Add(Customer customer) => Added.Add(customer);
        public void Remove(Customer customer) => Removed.Add(customer);

        public Task SaveAsync(CancellationToken cancellationToken)
        {
            Saves++;
            return Task.CompletedTask;
        }
    }

    private static KeepinCrmClientSync Sync(Store store, params KeepinCrmClientSnapshot[] clients)
    {
        var source = new Mock<IKeepinCrmClientSource>();
        source.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(clients);
        return new KeepinCrmClientSync(source.Object, store, new KeepinCrmClientSyncSettings(Owner),
            NullLogger<KeepinCrmClientSync>.Instance);
    }

    [Test]
    public async Task Sync_creates_updates_by_id_and_hides_linked_clients_missing_from_KeepinCRM()
    {
        var renamed = Linked(1, "10", "ТОВ Старе");
        var missing = Linked(2, "11", "ТОВ Зникле");
        var manual = Unlinked(3, "Ручний клієнт");
        var store = new Store([renamed, missing, manual]);

        var result = await Sync(store,
                Client("10", "ТОВ Нове", "Олена", ["+380 67 123 45 67", "bad"]),
                Client("12", null, "Петро Іваненко", ["0501112233"]),
                Client("13", "Архівна", archived: true))
            .RunAsync(dryRun: false, removeUnlinked: false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That((result.Applied, result.Received, result.Created, result.Updated, result.Unchanged),
                Is.EqualTo((true, 3, 2, 1, 0)));
            Assert.That(renamed.DisplayName, Is.EqualTo("ТОВ Нове"));
            var contact = renamed.Contacts.Single();
            Assert.That((contact.Name, contact.Position, contact.IsPrimary), Is.EqualTo(("Олена", "KeepinCRM", true)));
            Assert.That(contact.Channels.Select(x => x.NormalizedValue), Is.EqualTo(new[] { "380671234567" }));
            Assert.That(missing.Active, Is.False);
            Assert.That(result.Deactivated, Is.EqualTo(new[] { "ТОВ Зникле (KeepinCRM 11)" }));
            Assert.That(manual.Active, Is.True);
            Assert.That(store.Removed, Is.Empty);
            var person = store.Added.Single(x => KeepinCrmClientIdentity.IdOf(x) == "12");
            Assert.That((person.Kind, person.DisplayName, person.OwnerId), Is.EqualTo(("person", "Петро Іваненко", Owner)));
            Assert.That(store.Added.Single(x => KeepinCrmClientIdentity.IdOf(x) == "13").Active, Is.False);
            Assert.That(store.Saves, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Repeating_the_same_data_changes_nothing()
    {
        var customer = Linked(1, "10", "ТОВ Нове");
        var store = new Store([customer]);
        var client = Client("10", "ТОВ Нове", "Олена", ["0671234567"]);
        await Sync(store, client).RunAsync(false, false, CancellationToken.None);
        var revision = customer.Revision;

        var again = await Sync(store, client).RunAsync(false, false, CancellationToken.None);

        Assert.That((again.Updated, again.Unchanged, customer.Revision), Is.EqualTo((0, 1, revision)));
    }

    [Test]
    public async Task Transfer_deletes_unlinked_clients_but_hides_those_quotes_use_and_preview_saves_nothing()
    {
        var unused = Unlinked(3, "Старий без КП");
        var quoted = Unlinked(4, "Старий з КП");
        var store = new Store([unused, quoted], inUse: 4);

        var preview = await Sync(store, Client("10", "ТОВ Нове"))
            .RunAsync(dryRun: true, removeUnlinked: true, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(preview.Applied, Is.False);
            Assert.That(preview.Deleted, Is.EqualTo(new[] { "Старий без КП" }));
            Assert.That(preview.HiddenInUse, Is.EqualTo(new[] { "Старий з КП" }));
            Assert.That(store.Removed, Is.EqualTo(new[] { unused }));
            Assert.That(quoted.Active, Is.False);
            Assert.That(store.Saves, Is.Zero);
        });
    }

    [Test]
    public void Empty_KeepinCRM_answer_changes_nothing()
    {
        var store = new Store([Unlinked(3, "Старий")]);

        _ = Assert.ThrowsAsync<KeepinCrmClientsUnavailableException>(() =>
            Sync(store).RunAsync(false, true, CancellationToken.None));
        Assert.That((store.Removed.Count, store.Saves), Is.EqualTo((0, 0)));
    }

    [Test]
    public void Parser_reads_items_company_person_phones_from_contacts_and_total_pages()
    {
        var page = KeepinCrmClientParser.ParsePage(JsonNode.Parse("""
            {"items":[
              {"id":10,"company":"ТОВ Нове","person":"Олена","phones":["+380671234567"],"email":"a@b.ua",
               "contacts":[{"person":"Іван","phones":["0501112233"]}],"archived":false},
              {"id":11,"company":"","person":"Петро","archived":true},
              {"company":"Без id"}
            ],"pagination":{"total_pages":3}}
            """));

        Assert.Multiple(() =>
        {
            Assert.That((page.Rows, page.TotalPages, page.Clients.Count), Is.EqualTo((3, (int?)3, 2)));
            var first = page.Clients[0];
            Assert.That((first.KeepinCrmId, first.Company, first.Person, first.Archived), Is.EqualTo(("10", "ТОВ Нове", "Олена", false)));
            Assert.That(first.Phones, Is.EqualTo(new[] { "+380671234567", "0501112233" }));
            Assert.That(first.Emails, Is.EqualTo(new[] { "a@b.ua" }));
            Assert.That((page.Clients[1].Company, page.Clients[1].Archived), Is.EqualTo(((string?)null, true)));
        });
    }
}
