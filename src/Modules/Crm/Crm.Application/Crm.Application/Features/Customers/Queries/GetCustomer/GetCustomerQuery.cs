using Crm.Application.Abstractions.Customers;
namespace Crm.Application.Features.Customers.Queries.GetCustomer;

public sealed record GetCustomerQuery(Guid CustomerId, CrmActor Actor) : IQuery<Result<CustomerDetail>>;
