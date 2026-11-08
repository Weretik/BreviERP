using Crm.Application.Leads.Access;
using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.Persistence;
using Crm.Domain.Leads;

namespace Crm.Application.Leads.References;

/// <summary>Writes what a category letter means (R-11). Codes come only from the sheet.</summary>
public sealed record SaveLeadCategoryCommand(string Code, SaveLeadCategoryInput Input)
    : ICommand<Result<LeadCategoryResponse>>;

public sealed class SaveLeadCategoryCommandValidator : AbstractValidator<SaveLeadCategoryCommand>
{
    public SaveLeadCategoryCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(LeadLimits.CategoryMaxLength);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Description).MaximumLength(LeadLimits.CategoryDescriptionMaxLength)
            .When(x => x.Input is not null)
            .WithMessage($"Опис до {LeadLimits.CategoryDescriptionMaxLength} символів.");
    }
}

public sealed class SaveLeadCategoryCommandHandler(ILeadStore store)
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
