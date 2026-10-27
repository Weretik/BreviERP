using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Access;
namespace Crm.Application.Customers.MergeCustomers;

public sealed record MergeCustomersCommand(Guid CustomerId, Guid VictimId, long ExpectedRevision, long VictimRevision, string Reason, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
