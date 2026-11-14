namespace Crm.Api.Customers;

public sealed record MergeCustomersRequest(Guid RequestId, long ExpectedRevision, Guid VictimId, long VictimRevision, string Reason);
