namespace BuildingBlocks.Api.Authorization;

/// <summary>
/// Names the «Права доступу» action an endpoint needs when its HTTP method says otherwise, such as a POST that edits
/// (<c>edit</c>) or reads (<c>view</c>). Values: <c>view</c>, <c>create</c>, <c>edit</c>, <c>delete</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AccessActionAttribute(string action) : Attribute
{
    public const string View = "view";
    public const string Create = "create";
    public const string Edit = "edit";
    public const string Delete = "delete";

    public string Action { get; } = action;
}
