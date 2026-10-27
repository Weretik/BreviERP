using Crm.Application.Customers.SaveCustomerContact.Inputs;
using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Access;
namespace Crm.Application.Customers.SaveCustomerContact;

public sealed record SaveCustomerContactCommand(Guid CustomerId, Guid? ContactId, long ExpectedRevision, ContactInput Input, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
