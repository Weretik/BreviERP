using Crm.Application.Features.Customers.Shared.Inputs;
using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.Views;
namespace Crm.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(CustomerInput Input, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
