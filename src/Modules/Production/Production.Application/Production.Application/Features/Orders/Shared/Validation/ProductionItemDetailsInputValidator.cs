using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Orders.Shared.Validation;

public sealed class ProductionItemDetailsInputValidator : AbstractValidator<ProductionItemDetailsInput>
{
    public ProductionItemDetailsInputValidator()
    {
        RuleFor(x => x.Quantity).InclusiveBetween(1, ProductionItem.MaxQuantity)
            .WithMessage("Кількість — ціле число від 1.");
        RuleFor(x => x.DueDate).NotEmpty().WithMessage("Вкажіть «Готово до».");
        RuleFor(x => x.MainColor).NotEmpty().WithMessage("Вкажіть основний колір.")
            .MaximumLength(ProductionItem.ColorMaxLength);
        RuleFor(x => x.AdditionalColors).NotNull()
            .Must(x => x is null || x.Count(c => !string.IsNullOrWhiteSpace(c)) <= ProductionItem.MaxAdditionalColors)
            .WithMessage("Не більше двох додаткових кольорів.");
        RuleForEach(x => x.AdditionalColors).MaximumLength(ProductionItem.ColorMaxLength);
        RuleFor(x => x.SizesText).MaximumLength(ProductionItem.LongTextMaxLength);
        RuleFor(x => x.TechnicalChanges).MaximumLength(ProductionItem.LongTextMaxLength);
        RuleFor(x => x.Comment).MaximumLength(ProductionItem.LongTextMaxLength);
        RuleFor(x => x.Applications).NotNull()
            .Must(x => x is null || x.Count <= ProductionItem.MaxApplications)
            .WithMessage("Забагато нанесень.");
        RuleForEach(x => x.Applications).SetValidator(new ItemApplicationInputValidator());
    }
}

public sealed class ItemApplicationInputValidator : AbstractValidator<ItemApplicationInput>
{
    public ItemApplicationInputValidator()
    {
        RuleFor(x => x.Kind).Must(x => Enum.TryParse<ItemApplicationKind>(x, true, out var kind) && Enum.IsDefined(kind))
            .WithMessage("Вид нанесення — вишивка або друк.");
        RuleFor(x => x.Place).MaximumLength(ItemApplication.TextMaxLength);
        RuleFor(x => x.Description).MaximumLength(ItemApplication.TextMaxLength);
        RuleFor(x => x.Color).MaximumLength(ItemApplication.TextMaxLength);
        RuleFor(x => x.EmbroideryCalculationId).GreaterThan(0).When(x => x.EmbroideryCalculationId is not null);
        RuleFor(x => x.EmbroideryCalculationId).Null()
            .When(x => Enum.TryParse<ItemApplicationKind>(x.Kind, true, out var kind) &&
                kind != ItemApplicationKind.Embroidery)
            .WithMessage("Розрахунок вишивки можна обрати лише для вишивки.");
    }
}

internal static class OrderRules
{
    public static IRuleBuilderOptions<T, string> ClientName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Вкажіть клієнта.").MaximumLength(ProductionOrder.ClientMaxLength);

    public static IRuleBuilderOptions<T, ProductionActor> ValidActor<T>(this IRuleBuilder<T, ProductionActor> rule) =>
        rule.NotNull().Must(x => x is null || x.UserId > 0);
}
