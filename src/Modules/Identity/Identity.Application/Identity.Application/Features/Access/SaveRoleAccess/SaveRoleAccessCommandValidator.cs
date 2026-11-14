using FluentValidation;
using Identity.Domain.Authorization;

namespace Identity.Application.Features.Access.SaveRoleAccess;

public sealed class SaveRoleAccessCommandValidator : AbstractValidator<SaveRoleAccessCommand>
{
    public SaveRoleAccessCommandValidator()
    {
        RuleFor(x => x.Role).NotEmpty().MaximumLength(256)
            .Must(role => !AccessCatalog.IsLocked(role)).WithMessage("Директор завжди має всі права.");
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Grants).NotNull();
        RuleForEach(x => x.Grants).Must(BeKnown)
            .WithMessage("Невідома сторінка або дія, якої на цій сторінці немає.");
        RuleForEach(x => x.Grants).Must(SeeFirst)
            .WithMessage("A page must be visible («Перегляд») before anything else is allowed on it.");
    }

    private static bool BeKnown(KeyValuePair<string, IReadOnlyList<AccessAction>> grant)
    {
        var area = AccessCatalog.Find(grant.Key);
        return area is not null && grant.Value is not null
            && grant.Value.All(action => action != AccessAction.None && AccessActionsOf(area).Contains(action));
    }

    private static bool SeeFirst(KeyValuePair<string, IReadOnlyList<AccessAction>> grant) =>
        grant.Value is null || grant.Value.Count == 0 || grant.Value.Contains(AccessAction.View);

    private static AccessAction[] AccessActionsOf(AccessArea area) =>
        Shared.AccessActions.Ordered.Where(x => area.Actions.HasFlag(x)).ToArray();
}
