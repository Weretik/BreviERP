using Crm.Application.Features.Customers.Shared.Inputs;
namespace Crm.Api.Customers;

public sealed record CreateCustomerRequest(Guid RequestId, CustomerInput Input);
