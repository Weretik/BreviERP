using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
using Crm.Application.Features.Customers.Shared.Mapping;
using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.Views;
namespace Crm.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerHandler(ICustomerMutationStore store) : ICommandHandler<CreateCustomerCommand, Result<CustomerDetail>>
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
