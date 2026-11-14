using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.Views;
namespace Crm.Application.Features.Customers.Commands.MergeCustomers;

public sealed record MergeCustomersCommand(Guid CustomerId, Guid VictimId, long ExpectedRevision, long VictimRevision, string Reason, Guid RequestId, CrmActor Actor) : ICommand<Result<CustomerDetail>>;
