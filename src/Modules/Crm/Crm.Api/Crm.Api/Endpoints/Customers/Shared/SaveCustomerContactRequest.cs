using Crm.Application.Features.Customers.Commands.SaveCustomerContact;
namespace Crm.Api.Endpoints.Customers.Shared;

public sealed record SaveCustomerContactRequest(Guid RequestId, long ExpectedRevision, ContactInput Input);
