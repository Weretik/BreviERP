using Warehouse.Domain.Stock;

namespace Warehouse.Application.Features.Stock.Shared;

/// <summary>Field rules shared by the stock forms.</summary>
internal static class StockRules
{
    public static readonly DateOnly FirstDate = new(1900, 1, 1);
    public static readonly DateOnly LastDate = new(2100, 12, 31);

    /// <summary>0,01…1 000 000 with at most two decimals.</summary>
    public static IRuleBuilderOptions<T, decimal> PositiveMeters<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.Must(x => x > 0 && FabricStock.IsValidMeters(x)).WithMessage(StockMessages.Meters);

    public static IRuleBuilderOptions<T, string> Reason<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(StockMessages.Note).MaximumLength(FabricMovement.NoteMaxLength);

    /// <summary>«Скасувати», «Сторно» and an edit's reason: 3…500 characters.</summary>
    public static IRuleBuilderOptions<T, string?> RevisionReason<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(x => x?.Trim().Length is >= 3 and <= FabricMovementRevision.ReasonMaxLength)
            .WithMessage(StockMessages.Reason);

    public static IRuleBuilderOptions<T, DateOnly> OperationDate<T>(this IRuleBuilder<T, DateOnly> rule) =>
        rule.Must(x => x >= FirstDate && x <= LastDate).WithMessage(StockMessages.OperationDate);

    public static IRuleBuilderOptions<T, Guid> OperationId<T>(this IRuleBuilder<T, Guid> rule) =>
        rule.NotEqual(Guid.Empty).WithMessage(StockMessages.OperationId);

    public static IRuleBuilderOptions<T, string?> FormText<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(FabricMovement.FormTextMaxLength)
            .WithMessage($"Не більше {FabricMovement.FormTextMaxLength} символів.");
}
