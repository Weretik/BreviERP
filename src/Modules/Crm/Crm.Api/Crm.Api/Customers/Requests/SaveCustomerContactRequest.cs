using Crm.Application.Customers.SaveCustomerContact.Inputs;
namespace Crm.Api.Customers.Requests;

public sealed record SaveCustomerContactRequest(Guid RequestId, long ExpectedRevision, ContactInput Input);
