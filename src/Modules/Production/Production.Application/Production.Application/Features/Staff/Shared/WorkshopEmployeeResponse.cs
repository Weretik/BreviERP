namespace Production.Application.Features.Staff.Shared;

public sealed record WorkshopEmployeeResponse(
    int Id, string FullName, string Position, bool IsActive, DateTimeOffset UpdatedAt);
