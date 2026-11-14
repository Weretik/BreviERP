using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
using Crm.Application.Features.Customers.Shared.Mapping;
using Crm.Application.Abstractions.Customers;
namespace Crm.Application.Features.Customers.Commands.ReviseCustomer;

public sealed class ReviseCustomerHandler(ICustomerMutationStore store) : ICommandHandler<ReviseCustomerCommand, Result<CustomerDetail>>
{
    public async ValueTask<Result<CustomerDetail>> Handle(ReviseCustomerCommand command, CancellationToken cancellationToken)
    {
        return await store.ExecuteAsync("ReviseCustomer", command.RequestId, command.Actor, command, async (session, ct) =>
        {

            var customer = await session.LoadAsync(command.CustomerId, ct);
            if (customer is null || !command.Actor.CanAccess(customer)) return Result.NotFound();
            var revised = customer.Revise(command.ExpectedRevision, command.Input.Kind, command.Input.DisplayName,
                command.Input.LegalName, command.Input.TaxIdentity, command.Active);
            if (revised.IsFailure) return revised.Error!.ToResult<CustomerDetail>();
            foreach (var input in command.Input.ExternalIdentities)
            {
                var identity = CustomerExternalIdentity.Create(input.Source, input.Type, input.ExternalId);
                if (identity.IsFailure) return identity.Error!.ToResult<CustomerDetail>();
                var known = identity.Value;
                if (customer.ExternalIdentities.Any(x => x.Source == known.Source && x.Type == known.Type && x.ExternalId == known.ExternalId))
                    continue;
                var added = customer.AddExternalIdentity(known);
                if (added.IsFailure) return added.Error!.ToResult<CustomerDetail>();
            }
            await session.FlushAsync(ct);
            return Result.Success(CustomerDetailMapper.Map(customer, command.Actor));

        }, cancellationToken);
    }
}
