using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
using Crm.Application.Features.Customers.Shared.Mapping;
using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.Views;
namespace Crm.Application.Features.Customers.Commands.MergeCustomers;

public sealed class MergeCustomersHandler(ICustomerMutationStore store) : ICommandHandler<MergeCustomersCommand, Result<CustomerDetail>>
{
    public async ValueTask<Result<CustomerDetail>> Handle(MergeCustomersCommand command, CancellationToken cancellationToken)
    {
        return await store.ExecuteAsync("MergeCustomers", command.RequestId, command.Actor, command, async (session, ct) =>
        {

            if (!command.Actor.IsAdmin) return Result.Forbidden();
            var reason = CustomerText.Required(command.Reason, 500, "reason");
            if (reason.IsFailure) return reason.Error!.ToResult<CustomerDetail>();
            var ids = new[] { command.CustomerId, command.VictimId }.Order().ToArray();
            var first = await session.LoadAsync(ids[0], ct);
            var second = await session.LoadAsync(ids[1], ct);
            var survivor = first?.PublicId == command.CustomerId ? first : second;
            var victim = first?.PublicId == command.VictimId ? first : second;
            if (survivor is null || victim is null) return Result.NotFound();
            var merged = survivor.MergeFrom(victim, command.ExpectedRevision, command.VictimRevision);
            if (merged.IsFailure) return merged.Error!.ToResult<CustomerDetail>();
            await session.FlushAsync(ct);
            return Result.Success(CustomerDetailMapper.Map(survivor, command.Actor));

        }, cancellationToken);
    }
}
