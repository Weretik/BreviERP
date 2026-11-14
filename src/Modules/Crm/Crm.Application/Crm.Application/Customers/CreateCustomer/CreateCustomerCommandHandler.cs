using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Mapping;
using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
namespace Crm.Application.Customers.CreateCustomer;

public sealed class CreateCustomerCommandHandler(ICustomerMutationStore store) : ICommandHandler<CreateCustomerCommand, Result<CustomerDetail>>
{
    public async ValueTask<Result<CustomerDetail>> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        return await store.ExecuteAsync("CreateCustomer", command.RequestId, command.Actor, command, async (session, ct) =>
        {

            var created = Customer.Create(command.Input.Kind, command.Input.DisplayName, command.Input.LegalName,
                command.Input.TaxIdentity, new CrmUserId(command.Actor.UserId));
            if (created.IsFailure) return created.Error!.ToResult<CustomerDetail>();
            var customer = created.Value;
            foreach (var input in command.Input.ExternalIdentities)
            {
                var identity = CustomerExternalIdentity.Create(input.Source, input.Type, input.ExternalId);
                if (identity.IsFailure) return identity.Error!.ToResult<CustomerDetail>();
                var added = customer.AddExternalIdentity(identity.Value);
                if (added.IsFailure) return added.Error!.ToResult<CustomerDetail>();
            }
            session.Add(customer);
            await session.FlushAsync(ct);
            return Result.Success(CustomerDetailMapper.Map(customer, command.Actor));

        }, cancellationToken);
    }
}
