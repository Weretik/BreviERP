using Crm.Application.Features.Customers.Commands.SaveCustomerContact;
namespace Crm.Api.Customers.Requests;

public sealed record SaveCustomerContactRequest(Guid RequestId, long ExpectedRevision, ContactInput Input);
