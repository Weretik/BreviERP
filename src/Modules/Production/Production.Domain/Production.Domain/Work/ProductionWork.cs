using BuildingBlocks.Domain.Errors;
using BuildingBlocks.Domain.Results;
using Production.Domain.Orders;
using Production.Domain.Orders.Rules;

namespace Production.Domain.Work;

public enum ProductionWorkKind
{
    Sewing = 1,
    Cutting = 2
}

/// <summary>Why a worker did more than the item asks for (PRD-D35).</summary>
public static class OverPlanReasons
{
    public static IReadOnlyList<string> All { get; } =
        ["Запас", "Заміна браку", "Зміна кількості", "Зразок", "Повторне пошиття", "Інше"];

    public static bool IsKnown(string? reason) => reason is not null && All.Contains(reason.Trim());
}

/// <summary>Who did the work and for which item, as it was when the work was recorded.</summary>
public sealed record WorkTarget(int OrderId, int ItemId, int EmployeeId, string EmployeeName);

/// <summary>What was done.</summary>
public sealed record WorkFacts(ProductionWorkKind Kind, int Quantity, DateOnly WorkDate, bool OverPlan, string? Reason,
    string? Comment, decimal Rate);

/// <summary>
/// «Журнал робіт»: a worker sewed or cut some pieces of an item on a day (PRD-D34). The sewing rate of the item is kept
/// as it was, so a later rate change does not change the pay of past work. A record is never edited or deleted: a wrong
/// one is cancelled by «Сторно» with a reason (PRD-D36).
/// </summary>
public sealed class ProductionWork
{
    public const int MaxQuantity = 100_000;
    public const int CommentMaxLength = 1000;
    public const int ReasonMaxLength = 500;
    public const int LegacyIdMaxLength = 64;

    private ProductionWork() { }

    public static DomainResult<ProductionWork> Create(WorkTarget target, WorkFacts facts, string actorName,
        DateTimeOffset now, string? legacyId = null)
    {
        if (facts.Quantity is <= 0 or > MaxQuantity)
            return Error.Validation("production.work_quantity", $"Кількість — від 1 до {MaxQuantity}.", "quantity");
        if (facts.Rate <= 0)
            return Error.Conflict("production.work_no_rate", "У позиції немає розцінки пошиття. Вкажіть її в картці позиції перед обліком роботи.");
        if (facts.OverPlan && !OverPlanReasons.IsKnown(facts.Reason))
            return Error.Validation("production.work_over_plan_reason", "Для роботи понад план оберіть причину.", "reason");
        var comment = OrderText.Check(facts.Comment, CommentMaxLength, "comment");
        return comment.IsFailure ? comment.Error! : new ProductionWork(target, facts, actorName, now, legacyId);
    }

    private ProductionWork(WorkTarget target, WorkFacts facts, string actorName, DateTimeOffset now, string? legacyId)
    {
        OrderId = target.OrderId;
        ItemId = target.ItemId;
        EmployeeId = target.EmployeeId;
        EmployeeName = OrderText.Required(target.EmployeeName, ProductionOrder.PersonMaxLength);
        Kind = facts.Kind;
        Quantity = facts.Quantity;
        WorkDate = facts.WorkDate;
        OverPlan = facts.OverPlan;
        Reason = facts.OverPlan ? facts.Reason!.Trim() : null;
        Comment = OrderText.Optional(facts.Comment, CommentMaxLength);
        Rate = decimal.Round(facts.Rate, 2, MidpointRounding.AwayFromZero);
        CreatedAt = now;
        CreatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        LegacyId = OrderText.Optional(legacyId, LegacyIdMaxLength);
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int ItemId { get; private set; }
    public int EmployeeId { get; private set; }
    public string EmployeeName { get; private set; } = null!;
    public ProductionWorkKind Kind { get; private set; }
    public int Quantity { get; private set; }
    public DateOnly WorkDate { get; private set; }

    /// <summary>Beyond the item's quantity; does not count toward the plan.</summary>
    public bool OverPlan { get; private set; }

    public string? Reason { get; private set; }
    public string? Comment { get; private set; }

    /// <summary>The item's sewing rate in UAH when the work was recorded.</summary>
    public decimal Rate { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset? VoidedAt { get; private set; }
    public string? VoidedByName { get; private set; }
    public string? VoidReason { get; private set; }
    public string? LegacyId { get; private set; }

    public bool IsVoided => VoidedAt is not null;

    /// <summary>Counts toward «пошито» or «покроєно» of the item.</summary>
    public bool CountsToPlan => !IsVoided && !OverPlan;

    public DomainResult Void(string reason, string actorName, DateTimeOffset now)
    {
        if (IsVoided) return Error.Conflict("production.work_already_voided", "Цей запис уже сторновано.");
        var text = OrderText.Check(reason, ReasonMaxLength, "reason", required: true);
        if (text.IsFailure) return text;
        VoidReason = OrderText.Required(reason, ReasonMaxLength);
        VoidedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        VoidedAt = now;
        return DomainResult.Success();
    }
}
