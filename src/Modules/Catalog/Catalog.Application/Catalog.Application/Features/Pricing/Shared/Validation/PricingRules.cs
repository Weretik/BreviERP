namespace Catalog.Application.Features.Pricing.Shared.Validation;

/// <summary>Rules shared by the pricing requests: the actor, product and article IDs, and ID lists.</summary>
public static class PricingRules
{
    public const int MaxActorLength = 200;
    public const int MaxIdsPerRequest = 100;

    /// <summary>The signed-in user the API passes; never typed by a person.</summary>
    public static IRuleBuilderOptions<T, string?> ValidActor<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(actor => !string.IsNullOrWhiteSpace(actor) && actor.Length <= MaxActorLength)
            .WithMessage("Не вдалося визначити користувача.");

    public static IRuleBuilderOptions<T, int> ValidProductId<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThan(0).WithMessage("Неправильний товар.");

    public static IRuleBuilderOptions<T, long> ValidArticleId<T>(this IRuleBuilder<T, long> rule) =>
        rule.GreaterThan(0).WithMessage("Неправильний варіант.");

    public static IRuleBuilderOptions<T, Guid> ValidRequestId<T>(this IRuleBuilder<T, Guid> rule) =>
        rule.NotEmpty().WithMessage("Немає ідентифікатора запиту. Оновіть сторінку й спробуйте ще раз.");

    public static IRuleBuilderOptions<T, IReadOnlyList<int>?> DistinctProductIds<T>(
        this IRuleBuilder<T, IReadOnlyList<int>?> rule) =>
        rule.Must(ids => ids is { Count: >= 1 and <= MaxIdsPerRequest } && ids.All(id => id > 0)
                && ids.Distinct().Count() == ids.Count)
            .WithMessage($"Вкажіть від 1 до {MaxIdsPerRequest} різних товарів.");
}
