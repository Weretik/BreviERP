using Crm.Application.Features.Customers.Shared.Inputs;
using Crm.Application.Abstractions.Customers;
namespace Crm.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(CustomerInput Input, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
