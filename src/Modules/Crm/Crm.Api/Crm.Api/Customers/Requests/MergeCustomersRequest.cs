namespace Crm.Api.Customers.Requests;

public sealed record MergeCustomersRequest(Guid RequestId, long ExpectedRevision, Guid VictimId, long VictimRevision, string Reason);
