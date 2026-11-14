using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.Views;
namespace Crm.Application.Features.Customers.Queries.GetCustomer;

public sealed record GetCustomerQuery(Guid CustomerId, CrmActor Actor) : IQuery<Result<CustomerDetail>>;
