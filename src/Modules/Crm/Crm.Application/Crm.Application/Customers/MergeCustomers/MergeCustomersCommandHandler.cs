using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Mapping;
using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
namespace Crm.Application.Customers.MergeCustomers;

public sealed class MergeCustomersCommandHandler(ICustomerMutationStore store) : ICommandHandler<MergeCustomersCommand, Result<CustomerDetail>>
{
    public async ValueTask<Result<CustomerDetail>> Handle(MergeCustomersCommand command, CancellationToken cancellationToken)
    {
        return await store.ExecuteAsync("MergeCustomers", command.RequestId, command.Actor, command, async (session, ct) =>
        {

            if (!command.Actor.IsAdmin) return Result.Forbidden();
            CustomerText.Required(command.Reason, 500, "merge.reason");
            var ids = new[] { command.CustomerId, command.VictimId }.Order().ToArray();
            var first = await session.LoadAsync(ids[0], ct);
            var second = await session.LoadAsync(ids[1], ct);
            var survivor = first?.PublicId == command.CustomerId ? first : second;
            var victim = first?.PublicId == command.VictimId ? first : second;
            if (survivor is null || victim is null) return Result.NotFound();
            survivor.MergeFrom(victim, command.ExpectedRevision, command.VictimRevision);
            await session.FlushAsync(ct);
            return Result.Success(CustomerDetailMapper.Map(survivor, command.Actor));

        }, cancellationToken);
    }
}
