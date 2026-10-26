using Crm.Application.Customers.Models;
namespace Crm.Api.Customers.Requests;
public sealed record SaveCustomerContactRequest(Guid RequestId, long ExpectedRevision, ContactInput Input);
