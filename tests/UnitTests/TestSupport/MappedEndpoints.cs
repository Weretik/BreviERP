using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace UnitTests.TestSupport;

/// <summary>A module's Minimal API endpoints as mapped: operation name and the policies it needs.</summary>
internal static class MappedEndpoints
{
    public sealed record Operation(string Name, string[] Policies);

    public static Operation[] Of(Action<IEndpointRouteBuilder> map)
    {
        var app = WebApplication.CreateSlimBuilder().Build();
        map(app);
        return ((IEndpointRouteBuilder)app).DataSources.SelectMany(x => x.Endpoints)
            .Where(x => x.Metadata.GetMetadata<HttpMethodMetadata>() is not null)
            .Select(x => new Operation(x.Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName,
                x.Metadata.GetOrderedMetadata<IAuthorizeData>().Select(p => p.Policy).OfType<string>().Distinct()
                    .ToArray()))
            .ToArray();
    }
}
