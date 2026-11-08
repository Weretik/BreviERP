using Crm.Application.Leads.Contracts;

namespace Crm.Application.Leads.GetLeadCallResults;

/// <summary>Call results in order; <paramref name="ActiveOnly"/> for the «Мій день» buttons (R-06, R-11).</summary>
public sealed record GetLeadCallResultsQuery(bool ActiveOnly) : IQuery<Result<IReadOnlyList<LeadCallResultResponse>>>;
