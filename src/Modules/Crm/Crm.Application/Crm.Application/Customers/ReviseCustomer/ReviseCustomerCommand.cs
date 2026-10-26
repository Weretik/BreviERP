using Crm.Application.Customers.Models;
namespace Crm.Application.Customers.ReviseCustomer;
public sealed record ReviseCustomerCommand(Guid CustomerId, long ExpectedRevision, CustomerInput Input, bool Active, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
