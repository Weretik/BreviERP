using Crm.Application.Customers.Models;
namespace Crm.Application.Customers.GetCustomer;
public sealed record GetCustomerQuery(Guid CustomerId, CrmActor Actor) : IQuery<Result<CustomerDetail>>;
