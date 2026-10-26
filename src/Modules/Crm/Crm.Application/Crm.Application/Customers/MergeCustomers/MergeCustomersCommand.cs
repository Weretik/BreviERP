using Crm.Application.Customers.Models;
namespace Crm.Application.Customers.MergeCustomers;
public sealed record MergeCustomersCommand(Guid CustomerId, Guid VictimId, long ExpectedRevision, long VictimRevision, string Reason, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
