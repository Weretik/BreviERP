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

            var customer = Customer.Create(command.Input.Kind, command.Input.DisplayName, command.Input.LegalName, command.Input.TaxIdentity, new CrmUserId(command.Actor.UserId));
            foreach (var identity in command.Input.ExternalIdentities)
                customer.AddExternalIdentity(CustomerExternalIdentity.Create(identity.Source, identity.Type, identity.ExternalId));
            session.Add(customer);
            await session.FlushAsync(ct);
            return Result.Success(CustomerDetailMapper.Map(customer, command.Actor));

        }, cancellationToken);
    }
}
