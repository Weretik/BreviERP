using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ai.Application.Abstractions;

namespace Ai.Infrastructure.Integrations.OpenAi;

/// <summary>
/// POST /v1/responses with a strict <c>json_schema</c> answer and <c>store: false</c>. A PDF goes as <c>input_file</c>,
/// a picture as <c>input_image</c>, both as base64 data URLs. The key is only in the Authorization header.
/// </summary>
internal sealed class OpenAiResponsesClient(HttpClient http) : IOpenAiClient
{
    public const string BaseUrl = "https://api.openai.com/v1/";
    private const int MaxOutputTokens = 16_000;

    public async Task<OpenAiAnswer> RespondAsync(OpenAiRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "responses")
        {
            Content = new StringContent(Body(request).ToJsonString(), Encoding.UTF8, "application/json"),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(message, cancellationToken);
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new OpenAiException(OpenAiFailure.Unavailable, "no answer");
        }

        using (response)
        {
            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode) throw Failure(response.StatusCode, text);
            return Parse(text);
        }
    }

    internal static JsonObject Body(OpenAiRequest request)
    {
        var content = new JsonArray();
        if (!string.IsNullOrWhiteSpace(request.Text)) content.Add(new JsonObject { ["type"] = "input_text", ["text"] = request.Text });
        foreach (var file in request.Files)
        {
            var data = $"data:{file.MediaType};base64,{Convert.ToBase64String(file.Content)}";
            content.Add(file.MediaType.StartsWith("image/", StringComparison.Ordinal)
                ? new JsonObject { ["type"] = "input_image", ["image_url"] = data, ["detail"] = "high" }
                : new JsonObject { ["type"] = "input_file", ["filename"] = file.FileName, ["file_data"] = data });
        }

        return new JsonObject
        {
            ["model"] = request.Model,
            ["instructions"] = request.Instructions,
            ["input"] = new JsonArray(new JsonObject { ["role"] = "user", ["content"] = content }),
            ["text"] = new JsonObject
            {
                ["format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["name"] = request.SchemaName,
                    ["schema"] = JsonNode.Parse(request.SchemaJson),
                    ["strict"] = true,
                },
            },
            ["max_output_tokens"] = MaxOutputTokens,
            ["store"] = false,
        };
    }

    internal static OpenAiAnswer Parse(string text)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(text);
        }
        catch (JsonException)
        {
            throw new OpenAiException(OpenAiFailure.Unavailable, "unreadable answer");
        }

        var output = root?["output"]?.AsArray() ?? [];
        var parts = output.Where(x => x?["type"]?.GetValue<string>() == "message")
            .SelectMany(x => x!["content"]?.AsArray() ?? []).ToArray();
        var refusal = parts.FirstOrDefault(x => x?["type"]?.GetValue<string>() == "refusal");
        if (refusal is not null)
            throw new OpenAiException(OpenAiFailure.Rejected, refusal["refusal"]?.GetValue<string>() ?? "модель відмовилась відповідати");

        var json = parts.FirstOrDefault(x => x?["type"]?.GetValue<string>() == "output_text")?["text"]?.GetValue<string>();
        if (root?["status"]?.GetValue<string>() == "incomplete" || string.IsNullOrWhiteSpace(json))
            throw new OpenAiException(OpenAiFailure.Rejected, "відповідь неповна (документ завеликий або обірваний)");

        var usage = root!["usage"];
        return new OpenAiAnswer(json, usage?["input_tokens"]?.GetValue<long>() ?? 0, usage?["output_tokens"]?.GetValue<long>() ?? 0);
    }

    private static OpenAiException Failure(HttpStatusCode status, string body)
    {
        string? code = null, message = null;
        try
        {
            var error = JsonNode.Parse(body)?["error"];
            code = error?["code"]?.GetValue<string>();
            message = error?["message"]?.GetValue<string>();
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException)
        {
            // Not an OpenAI error body (a proxy page): the status decides.
        }

        return status switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new OpenAiException(OpenAiFailure.InvalidKey, "unauthorized"),
            HttpStatusCode.NotFound => new OpenAiException(OpenAiFailure.ModelNotFound, "model not found"),
            HttpStatusCode.TooManyRequests => new OpenAiException(OpenAiFailure.QuotaOrRateLimit, code ?? "rate limit"),
            HttpStatusCode.BadRequest when code == "model_not_found" => new OpenAiException(OpenAiFailure.ModelNotFound, "model not found"),
            HttpStatusCode.BadRequest => new OpenAiException(OpenAiFailure.Rejected, Short(message) ?? "invalid request"),
            _ => new OpenAiException(OpenAiFailure.Unavailable, $"HTTP {(int)status}"),
        };
    }

    private static string? Short(string? message) => message is { Length: > 300 } ? message[..300] : message;
}
