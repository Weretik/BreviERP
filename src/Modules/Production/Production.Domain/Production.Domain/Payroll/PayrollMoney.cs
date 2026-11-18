using BuildingBlocks.Domain;
using Production.Domain.Staff;
using Production.Domain.Payroll.Identifiers;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Results;
using Production.Domain.Orders;
using Production.Domain.Orders.Rules;

namespace Production.Domain.Payroll;

public enum PayrollPaymentKind
{
    Advance = 1,
    Card = 2,
    Cash = 3,
    Other = 4
}

/// <summary>Who and when for a payment or an adjustment of a worker in a month.</summary>
public sealed record PayrollEntryTarget(DateOnly Month, WorkshopEmployeeId EmployeeId, string EmployeeName);

/// <summary>
/// «Виплата» of a month: an advance, a card or cash payment (PRD-D45). Never edited or deleted: a wrong one is
/// cancelled with a reason and stays in the list.
/// </summary>
public sealed class PayrollPayment : DomainEventSource, IAggregateRoot
{
    public const decimal MaxAmount = 10_000_000m;
    public const int CommentMaxLength = 1000;
    public const int ReasonMaxLength = 500;

    private PayrollPayment() { }

    public static DomainResult<PayrollPayment> Create(PayrollEntryTarget target, DateOnly paidOn,
        PayrollPaymentKind kind, decimal amount, string? comment, string actorName, DateTimeOffset now,
        RequestKey? request = null)
    {
        if (amount is <= 0 or > MaxAmount) return PayrollErrors.AmountOutOfRange();
        var text = OrderText.Check(comment, CommentMaxLength, "comment");
        return text.IsFailure
            ? text.Error!
            : new PayrollPayment(target, paidOn, kind, amount, comment, actorName, now) { RequestId = request?.Id, RequestHash = request?.Hash };
    }

    private PayrollPayment(PayrollEntryTarget target, DateOnly paidOn, PayrollPaymentKind kind, decimal amount,
        string? comment, string actorName, DateTimeOffset now)
    {
        Month = target.Month;
        EmployeeId = target.EmployeeId;
        EmployeeName = OrderText.Required(target.EmployeeName, ProductionOrder.PersonMaxLength);
        PaidOn = paidOn;
        Kind = kind;
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Comment = OrderText.Optional(comment, CommentMaxLength);
        CreatedAt = now;
        CreatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        Raise(new PayrollPaymentRecorded(this, false));
    }

    public PayrollPaymentId Id { get; private set; }

    /// <summary>The id the form sent with «Зберегти»: a repeat of the same request adds nothing.</summary>
    public Guid? RequestId { get; private init; }
    /// <summary>Hash of what the request asked for; a repeat with other data answers 409.</summary>
    public string? RequestHash { get; private init; }

    public DateOnly Month { get; private set; }
    public WorkshopEmployeeId EmployeeId { get; private set; }
    public string EmployeeName { get; private set; } = null!;
    public DateOnly PaidOn { get; private set; }
    public PayrollPaymentKind Kind { get; private set; }
    public decimal Amount { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset? VoidedAt { get; private set; }
    public string? VoidedByName { get; private set; }
    public string? VoidReason { get; private set; }

    public bool IsVoided => VoidedAt is not null;

    public DomainResult Void(string reason, string actorName, DateTimeOffset now)
    {
        if (IsVoided) return PayrollErrors.AlreadyVoided();
        var text = OrderText.Check(reason, ReasonMaxLength, "reason", required: true);
        if (text.IsFailure) return text;
        VoidReason = OrderText.Required(reason, ReasonMaxLength);
        VoidedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        VoidedAt = now;
        Raise(new PayrollPaymentRecorded(this, true));
        return DomainResult.Success();
    }
}

/// <summary>
/// «Інше нарахування / утримання»: an extra accrual (positive) or a deduction (negative) with a reason (PRD-D45).
/// Cancelled like a payment.
/// </summary>
public sealed class PayrollAdjustment : DomainEventSource, IAggregateRoot
{
    private PayrollAdjustment() { }

    public static DomainResult<PayrollAdjustment> Create(PayrollEntryTarget target, decimal amount, string reason,
        string actorName, DateTimeOffset now, RequestKey? request = null)
    {
        if (amount == 0 || Math.Abs(amount) > PayrollPayment.MaxAmount) return PayrollErrors.AdjustmentAmountOutOfRange();
        var text = OrderText.Check(reason, PayrollPayment.ReasonMaxLength, "reason", required: true);
        return text.IsFailure
            ? text.Error!
            : new PayrollAdjustment(target, amount, reason, actorName, now) { RequestId = request?.Id, RequestHash = request?.Hash };
    }

    private PayrollAdjustment(PayrollEntryTarget target, decimal amount, string reason, string actorName,
        DateTimeOffset now)
    {
        Month = target.Month;
        EmployeeId = target.EmployeeId;
        EmployeeName = OrderText.Required(target.EmployeeName, ProductionOrder.PersonMaxLength);
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Reason = OrderText.Required(reason, PayrollPayment.ReasonMaxLength);
        CreatedAt = now;
        CreatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        Raise(new PayrollAdjustmentRecorded(this, false));
    }

    public PayrollAdjustmentId Id { get; private set; }

    /// <summary>The id the form sent with «Зберегти»: a repeat of the same request adds nothing.</summary>
    public Guid? RequestId { get; private init; }
    /// <summary>Hash of what the request asked for; a repeat with other data answers 409.</summary>
    public string? RequestHash { get; private init; }

    public DateOnly Month { get; private set; }
    public WorkshopEmployeeId EmployeeId { get; private set; }
    public string EmployeeName { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset? VoidedAt { get; private set; }
    public string? VoidedByName { get; private set; }
    public string? VoidReason { get; private set; }

    public bool IsVoided => VoidedAt is not null;

    public DomainResult Void(string reason, string actorName, DateTimeOffset now)
    {
        if (IsVoided) return PayrollErrors.AlreadyVoided();
        var text = OrderText.Check(reason, PayrollPayment.ReasonMaxLength, "reason", required: true);
        if (text.IsFailure) return text;
        VoidReason = OrderText.Required(reason, PayrollPayment.ReasonMaxLength);
        VoidedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        VoidedAt = now;
        Raise(new PayrollAdjustmentRecorded(this, true));
        return DomainResult.Success();
    }
}

/// <summary>«Видано»: the sum the director handed to a worker for the month, a note beside the calculation.</summary>
public sealed class PayrollIssued : IAggregateRoot
{
    private PayrollIssued() { }

    private PayrollIssued(DateOnly month, WorkshopEmployeeId employeeId) => (Month, EmployeeId) = (month, employeeId);

    public static PayrollIssued Create(DateOnly month, WorkshopEmployeeId employeeId) =>
        new(month, employeeId);

    public DateOnly Month { get; private set; }
    public WorkshopEmployeeId EmployeeId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string UpdatedByName { get; private set; } = null!;

    public DomainResult Set(decimal amount, string actorName, DateTimeOffset now)
    {
        if (amount is < 0 or > PayrollPayment.MaxAmount) return PayrollErrors.IssuedAmountOutOfRange();
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        UpdatedAt = now;
        UpdatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        return DomainResult.Success();
    }
}
