using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.ExternalIdentities;
using Crm.Domain.Customers.Identifiers;
using Crm.Application.Features.Customers.Shared.Mapping;
using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.Views;
using BuildingBlocks.Application;
namespace Crm.Application.Features.Customers.Commands.SaveCustomerContact;

public sealed class SaveCustomerContactHandler(ICustomerMutationStore store) : ICommandHandler<SaveCustomerContactCommand, Result<CustomerDetail>>
{
    public async ValueTask<Result<CustomerDetail>> Handle(SaveCustomerContactCommand command, CancellationToken cancellationToken)
    {
        return await store.ExecuteAsync("SaveCustomerContact", command.RequestId, command.Actor, command, async (session, ct) =>
        {

            var customer = await session.LoadAsync(command.CustomerId, ct);
            if (customer is null || !command.Actor.CanAccess(customer)) return Result.NotFound();
            var channels = new List<CustomerChannel>();
            foreach (var input in command.Input.Channels)
            {
                var channel = CustomerChannel.Create(input.Kind, input.Value);
                if (channel.IsFailure) return channel.Error!.ToResult<CustomerDetail>();
                channels.Add(channel.Value);
            }

            var saved = customer.SaveContact(command.ExpectedRevision, command.ContactId, command.Input.Name,
                command.Input.Position, command.Input.Active, command.Input.IsPrimary, channels);
            if (saved.IsFailure) return saved.Error!.ToResult<CustomerDetail>();
            await session.FlushAsync(ct);
            return Result.Success(CustomerDetailMapper.Map(customer, command.Actor));

        }, cancellationToken);
    }
}
