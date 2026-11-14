using Catalog.Domain.Products;
using Catalog.Domain.Products.Sewing.Composition;

namespace Catalog.Application.Features.Product.Shared.Sewing.Inputs;

/// <summary>One operation line of a sewing composition. Error keys stay camelCase: <c>operations[i].quantity</c>.</summary>
public sealed class SewingOperationInputValidator : AbstractValidator<SewingOperationInput>
{
    public SewingOperationInputValidator()
    {
        RuleFor(x => x.OperationId).GreaterThan(0).OverridePropertyName("operationId")
            .WithMessage("Оберіть операцію.");
        RuleFor(x => x.Quantity).Must(SewingRules.PositiveDecimal).OverridePropertyName("quantity")
            .WithMessage("Вкажіть число більше 0, не більше шести знаків після коми.");
        RuleFor(x => x.ManualMinutes).Must(minutes => SewingRules.PositiveDecimal(minutes!.Value))
            .When(x => x.ManualMinutes.HasValue)
            .OverridePropertyName("manualMinutes")
            .WithMessage("Хвилини вручну — число більше 0, не більше шести знаків після коми.");
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).OverridePropertyName("sortOrder")
            .WithMessage("Порядок не може бути від'ємним.");
        RuleFor(x => x.Comment).Must(comment => comment is null || comment.Trim().Length <= SewingMaterialLine.CommentMaxLength)
            .OverridePropertyName("comment")
            .WithMessage($"Коментар — не більше {SewingMaterialLine.CommentMaxLength} символів.");
    }
}
