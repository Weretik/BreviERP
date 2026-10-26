using Crm.Application.Customers.Models;
using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
namespace Crm.Application.Customers.SaveCustomerContact;

public sealed class SaveCustomerContactCommandHandler(ICustomerMutationStore store) : ICommandHandler<SaveCustomerContactCommand, Result<CustomerDetail>>
{
    public async ValueTask<Result<CustomerDetail>> Handle(SaveCustomerContactCommand command, CancellationToken cancellationToken)
    {
        return await store.ExecuteAsync("SaveCustomerContact", command.RequestId, command.Actor, command, async (session, ct) =>
        {

        var customer = await session.LoadAsync(command.CustomerId, ct);
        if (customer is null || !command.Actor.CanAccess(customer)) return Result.NotFound();
        customer.SaveContact(command.ExpectedRevision, command.ContactId, command.Input.Name, command.Input.Position,
            command.Input.Active, command.Input.IsPrimary, command.Input.Channels.Select(x => CustomerChannel.Create(x.Kind, x.Value)));
        await session.FlushAsync(ct);
        return Result.Success(CustomerDetail.From(customer, command.Actor));

        }, cancellationToken);
    }
}
