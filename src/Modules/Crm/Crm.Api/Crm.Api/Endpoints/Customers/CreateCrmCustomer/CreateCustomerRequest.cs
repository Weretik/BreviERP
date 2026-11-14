using Crm.Application.Features.Customers.Shared.Inputs;
namespace Crm.Api.Endpoints.Customers.CreateCrmCustomer;

public sealed record CreateCustomerRequest(Guid RequestId, CustomerInput Input);
