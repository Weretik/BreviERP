namespace Host.Api.Security.AnonymousApiAccess;

public sealed class AnonymousApiAccessOptions
{
    public const string SectionName = "ApiAccess";
    public bool AnonymousEnabled { get; set; }
    public int? ActorUserId { get; set; }
}
