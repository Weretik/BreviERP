using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Sewing;

/// <summary>One material line of a sewing composition. Error keys stay camelCase: <c>materials[i].quantity</c>.</summary>
public sealed class SewingMaterialInputValidator : AbstractValidator<SewingMaterialInput>
{
    public SewingMaterialInputValidator()
    {
        RuleFor(x => x.Kind).Must(kind => Kind(kind) is not null).OverridePropertyName("kind")
            .WithMessage("Невідомий вид матеріалу.");
        RuleFor(x => x.Quantity).Must(SewingRules.PositiveDecimal).OverridePropertyName("quantity")
            .WithMessage("Вкажіть число більше 0, не більше шести знаків після коми.");
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).OverridePropertyName("sortOrder")
            .WithMessage("Порядок не може бути від'ємним.");
        RuleFor(x => x.Comment).Must(comment => comment is null || comment.Trim().Length <= SewingMaterialLine.CommentMaxLength)
            .OverridePropertyName("comment")
            .WithMessage($"Коментар — не більше {SewingMaterialLine.CommentMaxLength} символів.");
        RuleFor(x => x).Must(UnitFits).OverridePropertyName("unit")
            .WithMessage("Одиниця не підходить для цього виду матеріалу.");
        RuleFor(x => x).Must(line => line.ReferenceId is null && !string.IsNullOrWhiteSpace(line.CostCode)
                && line.CostCode.Trim().Length <= SewingRules.MaxCodeLength)
            .When(line => Kind(line.Kind) == SewingMaterialKind.OtherCost)
            .OverridePropertyName("costCode")
            .WithMessage($"Для інших витрат вкажіть код витрати до {SewingRules.MaxCodeLength} символів, без матеріалу.");
        RuleFor(x => x).Must(line => line.ReferenceId is > 0 && string.IsNullOrWhiteSpace(line.CostCode))
            .When(line => Kind(line.Kind) != SewingMaterialKind.OtherCost)
            .OverridePropertyName("referenceId")
            .WithMessage("Оберіть матеріал; код витрати має бути порожнім.");
    }

    private static SewingMaterialKind? Kind(string? value) =>
        Enum.TryParse<SewingMaterialKind>(value, out var kind) && Enum.IsDefined(kind) && kind.ToString() == value
            ? kind
            : null;

    private static bool UnitFits(SewingMaterialInput line)
    {
        var kind = Kind(line.Kind);
        var unit = kind switch
        {
            SewingMaterialKind.Fabric => "Metre",
            SewingMaterialKind.Accessory => "Piece",
            SewingMaterialKind.OtherCost => "Service",
            _ => null
        };
        return unit is not null && (line.Unit == unit || (kind == SewingMaterialKind.Accessory && line.Unit == "Metre"));
    }
}
