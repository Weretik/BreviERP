namespace BuildingBlocks.Application.Actors;

/// <summary>The signed-in person a change is made by: account id, login (e-mail or user name) and role names.</summary>
public sealed record Actor(int UserId, string Login, IReadOnlyList<string> Roles);
