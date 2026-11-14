using Crm.Application.Features.Customers.Shared.Inputs;
namespace Crm.Api.Customers.Requests;

public sealed record CreateCustomerRequest(Guid RequestId, CustomerInput Input);
