using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Access;
namespace Crm.Application.Customers.GetCustomer;

public sealed record GetCustomerQuery(Guid CustomerId, CrmActor Actor) : IQuery<Result<CustomerDetail>>;
