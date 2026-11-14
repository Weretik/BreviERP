using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Persistence;

namespace Crm.Application.Features.Leads.Commands.SaveLeadCategory;

/// <summary>Writes what a category letter means (R-11). Codes come only from the sheet.</summary>
public sealed record SaveLeadCategoryCommand(string Code, SaveLeadCategoryInput Input)
    : ICommand<Result<LeadCategoryResponse>>;
