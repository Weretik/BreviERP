using Crm.Application.Customers.Models;
using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
namespace Crm.Application.Customers.ReviseCustomer;

public sealed class ReviseCustomerCommandHandler(ICustomerMutationStore store) : ICommandHandler<ReviseCustomerCommand, Result<CustomerDetail>>
{
    public async ValueTask<Result<CustomerDetail>> Handle(ReviseCustomerCommand command, CancellationToken cancellationToken)
    {
        return await store.ExecuteAsync("ReviseCustomer", command.RequestId, command.Actor, command, async (session, ct) =>
        {

        var customer = await session.LoadAsync(command.CustomerId, ct);
        if (customer is null || !command.Actor.CanAccess(customer)) return Result.NotFound();
        customer.Revise(command.ExpectedRevision, command.Input.Kind, command.Input.DisplayName, command.Input.LegalName, command.Input.TaxIdentity, command.Active);
        foreach (var identity in command.Input.ExternalIdentities)
            if (!customer.ExternalIdentities.Any(x => x.Source == identity.Source.Trim().ToLowerInvariant() && x.Type == identity.Type.Trim().ToLowerInvariant() && x.ExternalId == identity.ExternalId.Trim()))
                customer.AddExternalIdentity(CustomerExternalIdentity.Create(identity.Source, identity.Type, identity.ExternalId));
        await session.FlushAsync(ct);
        return Result.Success(CustomerDetail.From(customer, command.Actor));

        }, cancellationToken);
    }
}
