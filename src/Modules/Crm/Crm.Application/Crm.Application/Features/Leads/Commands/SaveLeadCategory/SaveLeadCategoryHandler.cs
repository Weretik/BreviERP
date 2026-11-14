using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Commands.SaveLeadCategory;

public sealed class SaveLeadCategoryHandler(ILeadStore store)
    : ICommandHandler<SaveLeadCategoryCommand, Result<LeadCategoryResponse>>
{
    public async ValueTask<Result<LeadCategoryResponse>> Handle(SaveLeadCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await store.LeadCategories.FindAsync([command.Code], cancellationToken);
        if (category is null) return Result.NotFound(LeadMessages.CategoryNotFound);
        category.Describe(command.Input.Description);
        await store.SaveChangesAsync(cancellationToken);
        var leads = await store.Leads.CountAsync(x => x.Category == category.Code, cancellationToken);
        return Result.Success(new LeadCategoryResponse(category.Code, category.Description, leads));
    }
}
