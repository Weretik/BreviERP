using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Contracts.Inputs;
using Crm.Application.Customers.Access;
namespace Crm.Application.Customers.ReviseCustomer;

public sealed record ReviseCustomerCommand(Guid CustomerId, long ExpectedRevision, CustomerInput Input, bool Active, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
