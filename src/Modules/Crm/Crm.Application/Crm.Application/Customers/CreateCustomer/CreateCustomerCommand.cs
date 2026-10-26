using Crm.Application.Customers.Models;
namespace Crm.Application.Customers.CreateCustomer;
public sealed record CreateCustomerCommand(CustomerInput Input, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
