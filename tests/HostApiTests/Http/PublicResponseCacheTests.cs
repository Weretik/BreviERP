using System.Net;
using Ardalis.Result;
using Content.Application.Features.Redirects.Queries.GetPublicRedirects;
using HostApiTests.TestSupport;

namespace HostApiTests.Http;

/// <summary>Public site answers may be kept for five minutes, as MVC's [ResponseCache] allowed.</summary>
public sealed class PublicResponseCacheTests
{
    [Test]
    public async Task Public_site_answers_are_public_for_five_minutes()
    {
        var (sender, _) = RecordingSender.Create(Result<PublicRedirectList>.NotFound());
        await using var host = await ApiTestHost.CreateAsync(sender);

        var response = await host.Client.GetAsync("/api/public/redirects");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Headers.CacheControl?.Public, Is.True);
            Assert.That(response.Headers.CacheControl?.MaxAge, Is.EqualTo(TimeSpan.FromSeconds(300)));
        });
    }
}
