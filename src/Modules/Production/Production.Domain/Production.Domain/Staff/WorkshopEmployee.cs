using BuildingBlocks.Domain.Results;

namespace Production.Domain.Staff;

/// <summary>
/// A worker of the sewing workshop («Працівники цеху»). Workers do not sign in: the head of the workshop records their
/// work, hours and pay. An employee who left is archived, not deleted, so old work and pay keep their name.
/// </summary>
public sealed class WorkshopEmployee
{
    public const int FullNameMaxLength = 120;
    public const int PositionMaxLength = 80;

    private WorkshopEmployee() { }

    public static DomainResult<WorkshopEmployee> Create(string fullName, string position, int? userId, DateTimeOffset now)
    {
        var valid = Check(fullName, position);
        return valid.IsFailure ? valid.Error! : new WorkshopEmployee(fullName, position, userId, now);
    }

    private WorkshopEmployee(string fullName, string position, int? userId, DateTimeOffset now)
    {
        SetDetails(fullName, position);
        IsActive = true;
        CreatedAt = now;
        Touch(userId, now);
    }

    public int Id { get; private set; }
    public string FullName { get; private set; } = null!;

    /// <summary>Lower-case <see cref="FullName"/>; unique among active employees.</summary>
    public string NameKey { get; private set; } = null!;

    public string Position { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int? UpdatedByUserId { get; private set; }

    public DomainResult Update(string fullName, string position, int? userId, DateTimeOffset now)
    {
        var valid = Check(fullName, position);
        if (valid.IsFailure) return valid;
        SetDetails(fullName, position);
        Touch(userId, now);
        return DomainResult.Success();
    }

    private static DomainResult Check(string fullName, string position) => DomainResult.Combine(
        ProductionText.Check(fullName, FullNameMaxLength, "fullName"),
        ProductionText.Check(position, PositionMaxLength, "position"));

    public void SetActive(bool isActive, int? userId, DateTimeOffset now)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        Touch(userId, now);
    }

    private void SetDetails(string fullName, string position)
    {
        FullName = ProductionText.Clean(fullName, FullNameMaxLength);
        NameKey = ProductionText.Key(FullName);
        Position = ProductionText.Clean(position, PositionMaxLength);
    }

    private void Touch(int? userId, DateTimeOffset now)
    {
        UpdatedAt = now;
        UpdatedByUserId = userId;
    }
}
