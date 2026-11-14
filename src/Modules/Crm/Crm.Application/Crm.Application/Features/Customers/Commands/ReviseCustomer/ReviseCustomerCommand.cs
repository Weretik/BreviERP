using Crm.Application.Features.Customers.Shared.Inputs;
using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.Views;
namespace Crm.Application.Features.Customers.Commands.ReviseCustomer;

public sealed record ReviseCustomerCommand(Guid CustomerId, long ExpectedRevision, CustomerInput Input, bool Active, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
