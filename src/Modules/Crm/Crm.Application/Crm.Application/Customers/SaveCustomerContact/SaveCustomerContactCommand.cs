using Crm.Application.Customers.Models;
namespace Crm.Application.Customers.SaveCustomerContact;
public sealed record SaveCustomerContactCommand(Guid CustomerId, Guid? ContactId, long ExpectedRevision, ContactInput Input, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
