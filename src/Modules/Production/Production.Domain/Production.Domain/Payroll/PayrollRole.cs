using Production.Domain.Orders;

namespace Production.Domain.Payroll;

public enum PayrollRoleKind
{
    /// <summary>«Майстер цеху»: a share of the sewing of all seamstresses.</summary>
    Master = 1,

    /// <summary>«Начальник цеху»: a share of the sewing of all seamstresses.</summary>
    Chief = 2
}

/// <summary>«Керівні частки»: who is paid the master's or the chief's share (PRD-D44). Holds until changed.</summary>
public sealed class PayrollRole
{
    private PayrollRole() { }

    public PayrollRole(PayrollRoleKind role) => Role = role;

    public PayrollRoleKind Role { get; private set; }
    public int? EmployeeId { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedByName { get; private set; }

    public void Assign(int? employeeId, string actorName, DateTimeOffset now)
    {
        EmployeeId = employeeId;
        UpdatedAt = now;
        UpdatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
    }
}
