using System.Text;
using Ai.Application.Abstractions;
using Ai.Infrastructure.Integrations.OpenAi;

namespace UnitTests.Ai;

/// <summary>AI-001: the Responses API request and answer.</summary>
public sealed class OpenAiResponsesClientTests
{
    [Test]
    public void A_pdf_goes_as_a_file_a_picture_as_an_image_and_the_answer_is_strict_json()
    {
        var body = OpenAiResponsesClient.Body(new OpenAiRequest("sk-secret", "gpt-6-luna", "Read.", "File name: a.pdf",
            [new AiFile("a.pdf", "application/pdf", Encoding.ASCII.GetBytes("%PDF")), new AiFile("b.png", "image/png", [1])],
            "purchase_invoice", """{"type":"object"}"""));

        var json = body.ToJsonString();
        Assert.That(json, Does.Not.Contain("sk-secret"));
        var content = body["input"]![0]!["content"]!.AsArray();
        Assert.That(content.Select(x => x!["type"]!.GetValue<string>()), Is.EqualTo(new[] { "input_text", "input_file", "input_image" }));
        Assert.That(content[1]!["file_data"]!.GetValue<string>(), Is.EqualTo("data:application/pdf;base64,JVBERg=="));
        Assert.That(body["text"]!["format"]!["strict"]!.GetValue<bool>(), Is.True);
        Assert.That(body["store"]!.GetValue<bool>(), Is.False);
    }

    [Test]
    public void The_output_text_and_usage_are_read()
    {
        var answer = OpenAiResponsesClient.Parse("""
            {"status":"completed","output":[{"type":"reasoning"},{"type":"message","content":[{"type":"output_text","text":"{\"ok\":true}"}]}],
             "usage":{"input_tokens":1200,"output_tokens":34}}
            """);
        Assert.That(answer, Is.EqualTo(new OpenAiAnswer("""{"ok":true}""", 1200, 34)));
    }

    [Test]
    public void A_refusal_or_an_incomplete_answer_is_rejected()
    {
        var refusal = Assert.Throws<OpenAiException>(() => OpenAiResponsesClient.Parse(
            """{"output":[{"type":"message","content":[{"type":"refusal","refusal":"no"}]}]}"""))!;
        var incomplete = Assert.Throws<OpenAiException>(() => OpenAiResponsesClient.Parse(
            """{"status":"incomplete","output":[{"type":"message","content":[{"type":"output_text","text":"{"}]}]}"""))!;
        Assert.That((refusal.Failure, incomplete.Failure), Is.EqualTo((OpenAiFailure.Rejected, OpenAiFailure.Rejected)));
    }
}
