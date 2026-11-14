using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;

namespace Crm.Application.Features.Leads.Queries.GetLeads;

/// <summary>«Усі ліди» (R-04): every lead, newest first, with search, a scope chip, category and stage.</summary>
/// <param name="Scope">`All` (default), `Mine`, `Free`.</param>
/// <param name="Stage">A <see cref="Crm.Domain.Leads.Enums.LeadStage"/> name, or `Open` for every stage still in work.</param>
public sealed record GetLeadsQuery(
    LeadActor Actor,
    string? Search,
    string? Scope,
    string? Category,
    string? Stage,
    int Page,
    int PageSize) : IQuery<Result<LeadPageResponse>>;
