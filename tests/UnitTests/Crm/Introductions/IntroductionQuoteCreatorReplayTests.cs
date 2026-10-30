using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Introductions;
using Moq;

namespace UnitTests.Crm.Introductions;

public sealed class IntroductionQuoteCreatorReplayTests
{
    [TestCase(ResultStatus.Ok)]
    [TestCase(ResultStatus.Conflict)]
    [TestCase(ResultStatus.NotFound)]
    public async Task Replay_outcome_returns_before_template_catalog_or_price_reads(ResultStatus status)
    {
        var actor = new CrmActor(17, false);
        var id = Guid.NewGuid();
        var template = Guid.NewGuid();
        var intent = new IntroductionQuoteApplicationIntent(template, null, null, null, "recipient");
        var saved = new QuoteDetail(Guid.NewGuid(), null, null, "recipient", 1, "draft", false, null, 17,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, true,
            new QuoteVersionView(Guid.NewGuid(), 1, "created", "recipient", null, null, "UAH", "no_extra_vat", 17,
                DateTimeOffset.UtcNow, []));
        var expected = status switch
        {
            ResultStatus.Ok => Result.Success(saved),
            ResultStatus.Conflict => Result<QuoteDetail>.Conflict("request_id_payload_conflict"),
            _ => Result<QuoteDetail>.NotFound()
        };
        var reader = new Mock<IIntroductionQuoteReplayReader>(MockBehavior.Strict);
        reader.Setup(x => x.ReadAsync(id, actor, intent, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        // Null downstream ports deliberately prove replay cannot touch archived/current input providers.
        var creator = new IntroductionQuoteCreator(null!, null!, null!, reader.Object);
        var result = await creator.CreateAsync(actor, id, template, null, null, null, "recipient", default);
        Assert.That(result, Is.SameAs(expected));
        reader.VerifyAll();
    }
    [Test]
    public void Ordinary_create_fingerprint_omits_new_null_intent_and_intro_has_explicit_kind()
    {
        var command = new global::Crm.Application.Quotes.CreateQuote.CreateQuoteCommand(null, null, "recipient",
            new global::Crm.Application.Quotes.Contracts.Inputs.QuoteDraftContent([]), Guid.NewGuid(), new CrmActor(17, false));
        var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
        Assert.That(System.Text.Json.JsonSerializer.Serialize(command, options), Does.Not.Contain("introductionApplicationIntent"));
        var intent = new IntroductionQuoteApplicationIntent(Guid.NewGuid(), null, null, null, "recipient");
        Assert.That(System.Text.Json.JsonSerializer.Serialize(intent, options), Does.Contain("introductionTemplateApplication-v1"));
    }
}
