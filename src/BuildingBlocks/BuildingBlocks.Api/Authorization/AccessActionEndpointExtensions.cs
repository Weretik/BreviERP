using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Api.Authorization;

/// <summary>Minimal API form of <see cref="AccessActionAttribute"/>: the «Права доступу» action of the endpoint.</summary>
public static class AccessActionEndpointExtensions
{
    public static TBuilder RequireAccessAction<TBuilder>(this TBuilder builder, string action)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithMetadata(new AccessActionAttribute(action));
}
