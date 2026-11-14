using System.Net;
using System.Text.Json;
using BuildingBlocks.Api.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HostApiTests;

/// <summary>The one global exception handler: Ukrainian ProblemDetails with traceId, never the exception text.</summary>
public sealed class GlobalExceptionHandlerTests
{
    [Test]
    public async Task Unexpected_exception_is_a_500_problem_without_the_exception_text()
    {
        using var host = await Start(_ => throw new InvalidOperationException("secret connection string"));

        var response = await host.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(json.RootElement.GetProperty("title").GetString(), Is.EqualTo(GlobalExceptionHandler.ServerErrorTitle));
            Assert.That(json.RootElement.GetProperty("traceId").GetString(), Is.Not.Empty);
            Assert.That(body, Does.Not.Contain("secret").And.Not.Contain("InvalidOperationException"));
        });
    }

    [Test]
    public async Task Validation_exception_is_a_400_problem_with_field_errors()
    {
        using var host = await Start(_ => throw new ValidationException([new ValidationFailure("Page", "Значення має бути від 1 до 1000000.")]));

        var response = await host.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(json.RootElement.GetProperty("detail").GetString(), Is.EqualTo(GlobalExceptionHandler.ValidationDetail));
            Assert.That(json.RootElement.GetProperty("errors").GetProperty("Page")[0].GetString(),
                Is.EqualTo("Значення має бути від 1 до 1000000."));
        });
    }

    private static async Task<IHost> Start(RequestDelegate endpoint)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(web => web.UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddProblemDetails();
                    services.AddExceptionHandler<GlobalExceptionHandler>();
                })
                .Configure(app =>
                {
                    app.UseExceptionHandler();
                    app.Run(endpoint);
                }))
            .Build();
        await host.StartAsync();
        return host;
    }
}
