using Crm.Application.Customers.Contracts.Inputs;
namespace Crm.Api.Customers.Requests;

public sealed record ReviseCustomerRequest(Guid RequestId, long ExpectedRevision, CustomerInput Input, bool Active);
