using Crm.Application.Customers.Models;
namespace Crm.Api.Customers.Requests;
public sealed record CreateCustomerRequest(Guid RequestId, CustomerInput Input);
