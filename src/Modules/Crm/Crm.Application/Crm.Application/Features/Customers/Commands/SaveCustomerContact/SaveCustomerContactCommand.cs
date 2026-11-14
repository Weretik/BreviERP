using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.Views;
namespace Crm.Application.Features.Customers.Commands.SaveCustomerContact;

public sealed record SaveCustomerContactCommand(Guid CustomerId, Guid? ContactId, long ExpectedRevision, ContactInput Input, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
