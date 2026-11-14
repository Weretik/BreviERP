using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Persistence;

namespace Crm.Application.Features.Leads.Queries.GetLeadCategories;

/// <summary>«Довідники › Категорії» (R-11) with the number of leads in each.</summary>
public sealed record GetLeadCategoriesQuery : IQuery<Result<IReadOnlyList<LeadCategoryResponse>>>;
