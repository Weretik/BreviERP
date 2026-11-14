using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Queries.GetCustomerLookup;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByTaxIds;

/// <summary>
/// Active clients whose ЄДРПОУ/ІПН is one of these codes, merged ones never; for another module that recognises who
/// paid by the code in a bank statement (for example the statement import of «Бухгалтерія»).
/// </summary>
public sealed record GetCustomersByTaxIdsQuery(IReadOnlyCollection<string> TaxIds) : IQuery<IReadOnlyList<CustomerLookup>>;
