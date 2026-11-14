namespace Production.Application.Features.Work.Queries.GetWorkChoices;

/// <summary>An active worker of «Працівники цеху» to pick on a record.</summary>
public sealed record WorkEmployeeChoice(int Id, string FullName, string Position);
