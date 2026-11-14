namespace Crm.Api.Endpoints.Customers.MergeCrmCustomers;

public sealed record MergeCustomersRequest(Guid RequestId, long ExpectedRevision, Guid VictimId, long VictimRevision, string Reason);
