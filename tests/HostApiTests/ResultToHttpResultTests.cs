using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace HostApiTests;

/// <summary>Minimal API endpoints answer with the same status codes and bodies as the controllers they replace.</summary>
public sealed class ResultToHttpResultTests
{
    [Test]
    public async Task Success_is_200_with_the_value()
    {
        var (status, body) = await RunAsync(Result.Success(new { Name = "ok" }).ToHttpResult());
        Assert.That(status, Is.EqualTo(200));
        Assert.That(body, Is.EqualTo("{\"name\":\"ok\"}"));
    }

    [Test]
    public async Task Invalid_is_400_with_the_validation_errors_list()
    {
        var (status, body) = await RunAsync(Result<int>.Invalid(new ValidationError("Name", "Вкажіть назву.")).ToHttpResult());
        Assert.That(status, Is.EqualTo(400));
        Assert.That(body, Does.Contain("\"identifier\":\"Name\"").And.Contain("\"errorMessage\":\"Вкажіть назву.\""));
    }

    [Test]
    public async Task Conflict_is_409_with_the_messages()
    {
        var (status, body) = await RunAsync(Result<int>.Conflict("Дані змінено.").ToHttpResult());
        Assert.That(status, Is.EqualTo(409));
        Assert.That(body, Is.EqualTo("[\"Дані змінено.\"]"));
    }

    [TestCase(ResultStatus.NotFound, 404)]
    [TestCase(ResultStatus.Unauthorized, 401)]
    [TestCase(ResultStatus.Error, 500)]
    public async Task Other_statuses_have_no_body(ResultStatus kind, int expected)
    {
        var result = kind switch
        {
            ResultStatus.NotFound => Result<int>.NotFound(),
            ResultStatus.Unauthorized => Result<int>.Unauthorized(),
            _ => Result<int>.Error("x")
        };
        var (status, body) = await RunAsync(result.ToHttpResult());
        Assert.That(status, Is.EqualTo(expected));
        Assert.That(body, Is.Empty);
    }

    [Test]
    public async Task No_content_success_is_204()
    {
        var (status, _) = await RunAsync(Result.Success().ToNoContentHttpResult());
        Assert.That(status, Is.EqualTo(204));
    }

    private static async Task<(int Status, string Body)> RunAsync(Microsoft.AspNetCore.Http.IResult result)
    {
        var services = new ServiceCollection().AddLogging().AddSingleton(NullLoggerFactory.Instance).BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        using var stream = new MemoryStream();
        context.Response.Body = stream;
        await result.ExecuteAsync(context);
        return (context.Response.StatusCode, System.Text.Encoding.UTF8.GetString(stream.ToArray()));
    }
}
