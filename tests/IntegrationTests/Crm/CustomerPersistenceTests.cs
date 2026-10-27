using Crm.Infrastructure.Persistence.Customers.Writing;
using Crm.Infrastructure.Persistence.Customers.Reading;
using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Contracts.Inputs;
using Crm.Application.Customers.Access;
using Ardalis.Result;
using Crm.Application.Customers.CreateCustomer;
using Crm.Application.Customers.GetCustomer;
using Crm.Application.Customers.GetCustomers;
using Crm.Application.Customers.MergeCustomers;
using Crm.Application.Customers.ReviseCustomer;
using Crm.Application.Customers.SaveCustomerContact;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace IntegrationTests.Crm;

[NonParallelizable]
public sealed class CustomerPersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Manager = new(17, false);
    private static readonly CrmActor Admin = new(1, true);
    private static CustomerInput Input(string name = "Synthetic client", bool external = false) => new("organization", name, null, "SYNTHETIC",
        external ? [new("fixture", "client", "source-1")] : []);

    private async Task<CustomerDetail> Create(CrmActor? actor = null, CustomerInput? input = null)
    {
        await using var db = Context();
        var result = await new CreateCustomerCommandHandler(new CustomerMutationStore(db))
            .Handle(new(input ?? Input(), Guid.NewGuid(), actor ?? Manager), default);
        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        return result.Value;
    }

    [Test]
    public async Task GeneratedTypedIdsAndSqlScopedProjectionsPreservePagedMetadata()
    {
        var customer = await Create();
        await Create(new(18, false));
        await using var db = Context();
        var handler = new GetCustomersQueryHandler(new CustomerReadRepository(db));
        var own = await handler.Handle(new(Manager, "Synthetic", true, 1, 25), default);
        var all = await handler.Handle(new(Admin, null, null, 1, 1), default);
        Assert.That(own.Value.Single().PublicId, Is.EqualTo(customer.PublicId));
        Assert.That(own.PagedInfo.TotalRecords, Is.EqualTo(1));
        Assert.That(all.PagedInfo.TotalRecords, Is.EqualTo(2));
        Assert.That(all.PagedInfo.TotalPages, Is.EqualTo(2));
        Assert.That(await db.Customers.AllAsync(x => x.Id != default), Is.True);
    }

    [Test]
    public async Task DetailAndForeignUpdateEnforceOwnerScope()
    {
        var customer = await Create();
        await using var db = Context();
        var foreign = new CrmActor(19, false);
        var detail = await new GetCustomerQueryHandler(new CustomerReadRepository(db)).Handle(new(customer.PublicId, foreign), default);
        var update = await new ReviseCustomerCommandHandler(new CustomerMutationStore(db))
            .Handle(new(customer.PublicId, 1, Input("Unauthorized"), true, Guid.NewGuid(), foreign), default);
        Assert.That(detail.Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(update.Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(await db.MutationRequests.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task IdenticalRequestReplaysAndChangedPayloadOrSourceKeyConflicts()
    {
        var request = Guid.NewGuid();
        var command = new CreateCustomerCommand(Input(external: true), request, Manager);
        await using var db = Context();
        var handler = new CreateCustomerCommandHandler(new CustomerMutationStore(db));
        var first = await handler.Handle(command, default);
        var repeat = await handler.Handle(command, default);
        var changed = await handler.Handle(command with { Input = Input("Different") }, default);
        var duplicate = await handler.Handle(command with { RequestId = Guid.NewGuid() }, default);
        Assert.That(repeat.Value.PublicId, Is.EqualTo(first.Value.PublicId));
        Assert.That(changed.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(duplicate.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(await db.Customers.CountAsync(), Is.EqualTo(1));
        Assert.That(await db.MutationRequests.CountAsync(), Is.EqualTo(1));
    }


    [Test]
    public async Task ConcurrentIdenticalRequestCreatesOneRootAndOneAuditRecord()
    {
        var command = new CreateCustomerCommand(Input(), Guid.NewGuid(), Manager);
        async Task<Result<CustomerDetail>> Save()
        {
            await using var context = Context();
            return await new CreateCustomerCommandHandler(new CustomerMutationStore(context)).Handle(command, default);
        }
        var results = await Task.WhenAll(Save(), Save());
        Assert.That(results.All(x => x.IsSuccess), Is.True);
        Assert.That(results.Select(x => x.Value.PublicId).Distinct().Count(), Is.EqualTo(1));
        await using var db = Context();
        Assert.That(await db.Customers.CountAsync(), Is.EqualTo(1));
        Assert.That(await db.MutationRequests.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ConcurrentExternalIdentityConflictRollsBackLosingCustomerGraph()
    {
        async Task<Result<CustomerDetail>> Save()
        {
            await using var context = Context();
            return await new CreateCustomerCommandHandler(new CustomerMutationStore(context))
                .Handle(new(Input(external: true), Guid.NewGuid(), Manager), default);
        }
        var results = await Task.WhenAll(Save(), Save());
        Assert.That(results.Count(x => x.IsSuccess), Is.EqualTo(1));
        Assert.That(results.Count(x => x.Status == ResultStatus.Conflict), Is.EqualTo(1));
        await using var db = Context();
        Assert.That(await db.Customers.CountAsync(), Is.EqualTo(1));
        Assert.That(await db.MutationRequests.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task MutationReplayCannotReadAClientAfterMergeRemovedAccessToThatRoot()
    {
        var survivor = await Create();
        var oldOwner = new CrmActor(18, false);
        var command = new CreateCustomerCommand(Input("Victim"), Guid.NewGuid(), oldOwner);
        await using var db = Context();
        var writer = new CreateCustomerCommandHandler(new CustomerMutationStore(db));
        var victim = await writer.Handle(command, default);
        var merge = await new MergeCustomersCommandHandler(new CustomerMutationStore(db))
            .Handle(new(survivor.PublicId, victim.Value.PublicId, 1, 1, "Verified duplicate", Guid.NewGuid(), Admin), default);
        Assert.That(merge.IsSuccess, Is.True);
        var replay = await writer.Handle(command, default);
        Assert.That(replay.Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(await db.MutationRequests.CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task ContactPrimarySwitchAndDeactivationRoundTripWithoutUniqueViolation()
    {
        var customer = await Create();
        await using var db = Context();
        var handler = new SaveCustomerContactCommandHandler(new CustomerMutationStore(db));
        var first = await handler.Handle(new(customer.PublicId, null, 1, new("First", null, true, true, [new("email", "synthetic@example.test")]), Guid.NewGuid(), Manager), default);
        var second = await handler.Handle(new(customer.PublicId, null, 2, new("Second", null, true, true, [new("email", "synthetic@example.test")]), Guid.NewGuid(), Manager), default);
        Assert.That(first.IsSuccess && second.IsSuccess, Is.True);
        var selected = second.Value.Contacts.Single(x => x.IsPrimary);
        var deactivated = await handler.Handle(new(customer.PublicId, selected.PublicId, 3, new("Second", null, false, true, []), Guid.NewGuid(), Manager), default);
        Assert.That(deactivated.Value.Contacts.Count(x => x.IsPrimary), Is.Zero);
        var detail = await new GetCustomerQueryHandler(new CustomerReadRepository(db)).Handle(new(customer.PublicId, Manager), default);
        Assert.That(detail.Value.Contacts, Has.Count.EqualTo(2));
        Assert.That(detail.Value.Contacts.Single(x => x.Name == "First").Channels.Single().Value, Is.EqualTo("synthetic@example.test"));
    }

    [Test]
    public async Task ParallelSavesAllowOneRevisionAndRollbackTheLoser()
    {
        var customer = await Create();
        async Task<Result<CustomerDetail>> Save(string name)
        {
            await using var db = Context();
            return await new ReviseCustomerCommandHandler(new CustomerMutationStore(db))
                .Handle(new(customer.PublicId, 1, Input(name), true, Guid.NewGuid(), Manager), default);
        }
        var results = await Task.WhenAll(Save("One"), Save("Two"));
        Assert.That(results.Count(x => x.IsSuccess), Is.EqualTo(1));
        Assert.That(results.Count(x => x.Status == ResultStatus.Conflict), Is.EqualTo(1));
        await using var db = Context();
        Assert.That(await db.MutationRequests.CountAsync(), Is.EqualTo(2));
        Assert.That((await db.Customers.SingleAsync()).Revision, Is.EqualTo(2));
    }

    [Test]
    public async Task AdminMergeMovesContactAndExternalIdentityAndRetainsVictimAudit()
    {
        var survivor = await Create(input: new CustomerInput("organization", "Survivor", null, "SYNTHETIC", [new("fixture", "client", "source-survivor")]));
        var victim = await Create(input: Input("Victim", true));
        await using var db = Context();
        var contact = await new SaveCustomerContactCommandHandler(new CustomerMutationStore(db))
            .Handle(new(victim.PublicId, null, 1, new("Victim contact", null, true, true, []), Guid.NewGuid(), Manager), default);
        var merged = await new MergeCustomersCommandHandler(new CustomerMutationStore(db))
            .Handle(new(survivor.PublicId, victim.PublicId, 1, contact.Value.Revision, "Proven duplicate", Guid.NewGuid(), Admin), default);
        Assert.That(merged.IsSuccess, Is.True, string.Join(",", merged.Errors));
        Assert.That(merged.Value.Contacts.Single().PublicId, Is.EqualTo(contact.Value.Contacts.Single().PublicId));
        Assert.That(merged.Value.ExternalIdentities.Select(x => x.ExternalId), Is.EquivalentTo(new[] { "source-survivor", "source-1" }));
        Assert.That((await db.Customers.SingleAsync(x => x.PublicId == victim.PublicId)).MergedIntoId, Is.Not.Null);
        Assert.That(await db.MutationRequests.CountAsync(x => x.Operation == "MergeCustomers"), Is.EqualTo(1));
    }
}
