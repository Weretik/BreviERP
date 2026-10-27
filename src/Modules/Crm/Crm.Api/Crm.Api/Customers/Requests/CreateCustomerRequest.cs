using Crm.Application.Customers.Contracts.Inputs;
namespace Crm.Api.Customers.Requests;

public sealed record CreateCustomerRequest(Guid RequestId, CustomerInput Input);
