using BuildingBlocks.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;

namespace UnitTests.TestSupport;

/// <summary>A module's Minimal API endpoints as mapped: operation name and the policies it needs.</summary>
internal static class MappedEndpoints
{
    /// <param name="Tag">The endpoint group's tag: the name of the controller it replaced.</param>
    /// <param name="Action">The «Права доступу» action the endpoint needs beyond viewing, if any.</param>
    /// <param name="Roles">The roles the endpoint itself requires, if any.</param>
    public sealed record Operation(string Name, string[] Policies, string Tag, string? Action = null,
        string? Roles = null, bool Anonymous = false, string Route = "", string Method = "");

    /// <summary>One endpoint by its operation name.</summary>
    public static Operation Named(Action<IEndpointRouteBuilder> map, string name) => Of(map).Single(x => x.Name == name);

    /// <summary>The endpoints of the given groups (tags), in the order the tags are given.</summary>
    public static Operation[] Of(Action<IEndpointRouteBuilder> map, params string[] tags) =>
        Of(map).Where(x => tags.Contains(x.Tag)).OrderBy(x => Array.IndexOf(tags, x.Tag)).ToArray();

    /// <summary>The single policy every endpoint of a group needs.</summary>
    public static string Policy(Action<IEndpointRouteBuilder> map, string tag) =>
        Of(map, tag).SelectMany(x => x.Policies).Distinct().Single();

    public static Operation[] Of(Action<IEndpointRouteBuilder> map)
    {
        var app = WebApplication.CreateSlimBuilder().Build();
        map(app);
        return ((IEndpointRouteBuilder)app).DataSources.SelectMany(x => x.Endpoints)
            .Where(x => x.Metadata.GetMetadata<HttpMethodMetadata>() is not null)
            .Select(x => new Operation(x.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName ?? "-",
                x.Metadata.GetOrderedMetadata<IAuthorizeData>().Select(p => p.Policy).OfType<string>().Distinct()
                    .ToArray(),
                x.Metadata.GetMetadata<ITagsMetadata>()?.Tags.FirstOrDefault() ?? string.Empty,
                x.Metadata.GetMetadata<AccessActionAttribute>()?.Action,
                x.Metadata.GetOrderedMetadata<IAuthorizeData>().Select(p => p.Roles).OfType<string>().SingleOrDefault(),
                x.Metadata.GetMetadata<IAllowAnonymous>() is not null,
                ((RouteEndpoint)x).RoutePattern.RawText ?? string.Empty,
                x.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.First()))
            .ToArray();
    }
}
