using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Contracts.Inputs;
using Crm.Application.Customers.Access;
namespace Crm.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(CustomerInput Input, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
