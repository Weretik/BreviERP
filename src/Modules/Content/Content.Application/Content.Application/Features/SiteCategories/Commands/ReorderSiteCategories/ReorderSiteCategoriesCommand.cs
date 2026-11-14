using Content.Application.Abstractions;
using Content.Domain.SiteCategories;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.ReorderSiteCategories;

/// <summary>
/// The new order of the categories of one parent (null for the top level). <paramref name="Ids"/> must list exactly
/// the categories of that parent; otherwise the list was changed meanwhile and nothing is saved.
/// </summary>
public sealed record ReorderSiteCategoriesCommand(int? ParentId, IReadOnlyList<int> Ids) : ICommand<Result>;

public sealed class ReorderSiteCategoriesValidator : AbstractValidator<ReorderSiteCategoriesCommand>
{
    public ReorderSiteCategoriesValidator()
    {
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId is not null);
        RuleFor(x => x.Ids).NotNull()
            .Must(ids => ids is null || (ids.Count <= SiteCategoryRules.MaxCategories && ids.Distinct().Count() == ids.Count))
            .WithMessage(SiteCategoryMessages.OrderChanged);
    }
}

public sealed class ReorderSiteCategoriesHandler(IContentDbContext db)
    : ICommandHandler<ReorderSiteCategoriesCommand, Result>
{
    public async ValueTask<Result> Handle(ReorderSiteCategoriesCommand command, CancellationToken cancellationToken)
    {
        var siblings = await db.SiteCategories.Where(x => x.ParentId == command.ParentId).ToListAsync(cancellationToken);
        if (siblings.Count != command.Ids.Count || siblings.Any(x => !command.Ids.Contains(x.Id)))
            return Result.Conflict(SiteCategoryMessages.OrderChanged);

        var byId = siblings.ToDictionary(x => x.Id);
        for (var index = 0; index < command.Ids.Count; index++) byId[command.Ids[index]].PlaceAt(index + 1);
        await db.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}
