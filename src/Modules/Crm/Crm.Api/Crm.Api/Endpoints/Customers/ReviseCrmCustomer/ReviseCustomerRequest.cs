using Crm.Application.Features.Customers.Shared.Inputs;
namespace Crm.Api.Endpoints.Customers.ReviseCrmCustomer;

public sealed record ReviseCustomerRequest(Guid RequestId, long ExpectedRevision, CustomerInput Input, bool Active);
