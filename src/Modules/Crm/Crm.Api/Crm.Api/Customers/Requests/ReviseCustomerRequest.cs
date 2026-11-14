using Crm.Application.Features.Customers.Shared.Inputs;
namespace Crm.Api.Customers.Requests;

public sealed record ReviseCustomerRequest(Guid RequestId, long ExpectedRevision, CustomerInput Input, bool Active);
