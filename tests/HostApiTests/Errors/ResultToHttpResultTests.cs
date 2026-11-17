using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using BuildingBlocks.Api.Errors;

namespace HostApiTests.Errors;

/// <summary>Successes answer as the controllers did; every failure is a Ukrainian ProblemDetails (API-02).</summary>
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
    public async Task Mapped_success_is_200_or_201_with_the_response_and_failure_keeps_the_mapping()
    {
        var ok = await RunAsync(Result.Success(5).ToHttpResult(x => new { Id = x }));
        var created = await RunAsync(Result.Success(5).ToCreatedHttpResult(x => new { Id = x }));
        var conflict = await RunAsync(Result<int>.Conflict("Дані змінено.").ToCreatedHttpResult(x => new { Id = x }));
        Assert.That(new[] { ok, created }, Is.EqualTo(new[] { (200, "{\"id\":5}"), (201, "{\"id\":5}") }));
        Assert.That(conflict.Status, Is.EqualTo(409));
        Assert.That(Json(conflict.Body).GetProperty("detail").GetString(), Is.EqualTo("Дані змінено."));
    }

    [Test]
    public async Task Success_without_a_value_is_204_as_MVC_answered_a_null_value()
    {
        var (status, body) = await RunAsync(Result.Success().ToHttpResult());
        var (nullStatus, _) = await RunAsync(Result<string?>.Success(null).ToHttpResult());
        Assert.That((status, body, nullStatus), Is.EqualTo((204, "", 204)));
    }

    [Test]
    public async Task Invalid_is_400_problem_with_errors_by_field()
    {
        var (status, body) = await RunAsync(Result<int>.Invalid(new ValidationError("Name", "Вкажіть назву.")).ToHttpResult());
        var problem = Json(body);
        Assert.That(status, Is.EqualTo(400));
        Assert.That(problem.GetProperty("title").GetString(), Is.EqualTo(ApiProblems.InvalidTitle));
        Assert.That(problem.GetProperty("errors").GetProperty("Name")[0].GetString(), Is.EqualTo("Вкажіть назву."));
    }

    [Test]
    public async Task Conflict_is_409_problem_with_the_messages_in_detail_and_errors()
    {
        var (status, body) = await RunAsync(Result<int>.Conflict("Дані змінено.", "Оновіть сторінку.").ToHttpResult());
        var problem = Json(body);
        Assert.That(status, Is.EqualTo(409));
        Assert.That(problem.GetProperty("detail").GetString(), Is.EqualTo("Дані змінено. Оновіть сторінку."));
        Assert.That(problem.GetProperty("errors").EnumerateArray().Select(x => x.GetString()),
            Is.EqualTo(new[] { "Дані змінено.", "Оновіть сторінку." }));
    }

    [Test]
    public async Task Unavailable_is_503_and_a_refusal_422_with_the_reasons()
    {
        var unavailable = await RunAsync(Result<int>.Unavailable("Google не відповідає.").ToHttpResult());
        var refused = await RunAsync(Result<int>.Error("Ключ не підійшов.").ToHttpResultWithRefusal());
        Assert.That((unavailable.Status, refused.Status), Is.EqualTo((503, 422)));
        Assert.That(Json(refused.Body).GetProperty("detail").GetString(), Is.EqualTo("Ключ не підійшов."));
    }

    [TestCase(ResultStatus.NotFound, 404)]
    [TestCase(ResultStatus.Error, 500)]
    public async Task Not_found_and_error_are_problems_without_internal_detail(ResultStatus kind, int expected)
    {
        var result = kind == ResultStatus.NotFound ? Result<int>.NotFound() : Result<int>.Error("stack trace");
        var (status, body) = await RunAsync(result.ToHttpResult());
        Assert.That(status, Is.EqualTo(expected));
        Assert.That(Json(body).GetProperty("status").GetInt32(), Is.EqualTo(expected));
        Assert.That(body, Does.Not.Contain("stack trace"));
    }

    [TestCase(ResultStatus.Unauthorized, 401)]
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

    private static System.Text.Json.JsonElement Json(string body) => System.Text.Json.JsonDocument.Parse(body).RootElement;

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
