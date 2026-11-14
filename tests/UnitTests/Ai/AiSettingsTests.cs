using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Application.Calls;
using Ai.Application.Contracts;
using Ai.Application.Extraction;
using Ai.Application.Settings;
using Ai.Domain.Settings;
using Ardalis.Result;

namespace UnitTests.Ai;

/// <summary>AI-001 «Налаштування ШІ»: write-only key, versioned settings, monthly limit and use, extra rules.</summary>
public sealed class AiSettingsTests
{
    private const string Admin = "admin@brevi.com.ua";
    private const string Key = "sk-proj-abcdefghijklmnopqrstuvwxyz1234";
    private AiTestHarness harness = null!;

    [SetUp]
    public void SetUp() => harness = new AiTestHarness();

    [TearDown]
    public void TearDown() => harness.Dispose();

    private Task<Result<AiSettingsResponse>> SetKeyAsync() =>
        new SetAiKeyCommandHandler(harness.Db, harness.Store(), harness.Protector, harness.Time)
            .Handle(new SetAiKeyCommand(Key, Admin), CancellationToken.None).AsTask();

    private Task<Result<AiSettingsResponse>> UpdateAsync(bool enabled, int version, decimal limit = 5m, string fast = "gpt-6-luna") =>
        new UpdateAiSettingsCommandHandler(harness.Db, harness.Store(), harness.Time)
            .Handle(new UpdateAiSettingsCommand(enabled, fast, 0.10m, 0.50m, "gpt-6.1-terra", 1m, 5m, limit, version, Admin),
                CancellationToken.None).AsTask();

    private Task<Result<AiExtraction>> ExtractAsync() =>
        new ExtractStructuredDataCommandHandler(harness.Caller()).Handle(new ExtractStructuredDataCommand(AiModelTier.Fast,
            "Read the invoice.", "file", [], "purchase_invoice", "{}", AiPromptKeys.PurchaseInvoices), CancellationToken.None).AsTask();

    [Test]
    public async Task Defaults_are_shown_before_the_first_save()
    {
        var settings = (await new GetAiSettingsQueryHandler(harness.Db, harness.Store())
            .Handle(new GetAiSettingsQuery(), CancellationToken.None)).Value;

        Assert.That((settings.HasKey, settings.Enabled, settings.FastModel, settings.StrongModel),
            Is.EqualTo((false, false, "gpt-6-luna", "gpt-6.1-terra")));
        Assert.That(settings.Usage, Is.EqualTo(new AiUsageResponse("2026-11", 0, 0, 0, 0m)));
        Assert.That(settings.Prompts.Single().Key, Is.EqualTo(AiPromptKeys.PurchaseInvoices));
    }

    [Test]
    public async Task The_fixed_instruction_is_shown_next_to_the_extra_rules()
    {
        var prompt = (await new GetAiSettingsQueryHandler(harness.Db, harness.Store())
            .Handle(new GetAiSettingsQuery(), CancellationToken.None)).Value.Prompts.Single();

        Assert.That((prompt.BaseText, prompt.Text), Is.EqualTo((FakeBasePrompt.Instruction, string.Empty)));
    }

    [Test]
    public async Task The_key_is_stored_encrypted_and_only_its_last_four_characters_come_back()
    {
        var response = (await SetKeyAsync()).Value;

        Assert.That((response.HasKey, response.KeyHint, response.Version), Is.EqualTo((true, "1234", 1)));
        Assert.That(System.Text.Json.JsonSerializer.Serialize(response), Does.Not.Contain(Key));
        using var db = harness.Fresh();
        Assert.That(db.Settings.Single().ProtectedApiKey, Is.Not.EqualTo(Key).And.StartWith("protected:"));
        Assert.That(new SetAiKeyCommand(Key, Admin).ToString(), Does.Not.Contain(Key));
    }

    [Test]
    public async Task Switching_on_needs_a_key_and_a_stale_version_is_refused()
    {
        Assert.That((await UpdateAsync(true, 0)).Status, Is.EqualTo(ResultStatus.Invalid));

        await SetKeyAsync();
        var on = await UpdateAsync(true, 1);
        var stale = await UpdateAsync(false, 1);

        Assert.That((on.Value.Enabled, on.Value.Version), Is.EqualTo((true, 2)));
        Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Removing_the_key_switches_the_ai_off()
    {
        await SetKeyAsync();
        await UpdateAsync(true, 1);

        var removed = (await new RemoveAiKeyCommandHandler(harness.Db, harness.Store(), harness.Time)
            .Handle(new RemoveAiKeyCommand(Admin), CancellationToken.None)).Value;

        Assert.That((removed.HasKey, removed.KeyHint, removed.Enabled), Is.EqualTo((false, (string?)null, false)));
    }

    [Test]
    public async Task A_request_uses_the_model_adds_the_extra_rules_and_counts_the_cost()
    {
        await SetKeyAsync();
        await UpdateAsync(true, 1);
        await new UpdateAiPromptCommandHandler(harness.Db, harness.Store(), harness.Time).Handle(
            new UpdateAiPromptCommand(AiPromptKeys.PurchaseInvoices, "  Номер замовлення — після «№».  ", Admin), CancellationToken.None);

        var first = (await ExtractAsync()).Value;
        await ExtractAsync();

        var request = harness.OpenAi.Requests.First();
        Assert.That((request.ApiKey, request.Model), Is.EqualTo((Key, "gpt-6-luna")));
        Assert.That(request.Instructions, Does.StartWith("Read the invoice.").And.EndWith("Номер замовлення — після «№»."));
        Assert.That(first.CostUsd, Is.EqualTo(0.15m), "1M input × 0.10 + 0.1M output × 0.50");
        using var db = harness.Fresh();
        var usage = db.UsageMonths.Single();
        Assert.That((usage.Id, usage.Requests, usage.InputTokens, usage.CostUsd), Is.EqualTo(("2026-11", 2, 2_000_000L, 0.30m)));
    }

    [Test]
    public async Task Nothing_is_sent_when_the_ai_is_off_or_the_limit_is_reached()
    {
        Assert.That((await ExtractAsync()).Errors, Is.EqualTo(new[] { AiCaller.NoKey }));
        await SetKeyAsync();
        Assert.That((await ExtractAsync()).Errors, Is.EqualTo(new[] { AiCaller.Disabled }));

        await UpdateAsync(true, 1, limit: 0.20m);
        await ExtractAsync();
        await ExtractAsync();
        var limited = await ExtractAsync();

        Assert.That((limited.Status, limited.Errors.Single()), Is.EqualTo((ResultStatus.Unavailable, AiCaller.LimitReached)));
        Assert.That(harness.OpenAi.Requests, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task The_test_works_while_off_and_explains_a_refused_key()
    {
        await SetKeyAsync();
        var handler = new TestAiConnectionCommandHandler(harness.Caller());

        var ok = await handler.Handle(new TestAiConnectionCommand(AiModelTier.Strong), CancellationToken.None);
        harness.OpenAi.Failure = new OpenAiException(OpenAiFailure.InvalidKey, "unauthorized");
        var refused = await handler.Handle(new TestAiConnectionCommand(AiModelTier.Fast), CancellationToken.None);

        Assert.That(ok.Value.Model, Is.EqualTo("gpt-6.1-terra"));
        Assert.That((refused.Status, refused.Errors.Single()), Is.EqualTo((ResultStatus.Error, "OpenAI не прийняв ключ. Перевірте його в «Налаштування ШІ».")));
    }

    [Test]
    public void Model_names_and_keys_are_checked()
    {
        var settings = new UpdateAiSettingsCommandValidator();
        Assert.That(settings.Validate(new UpdateAiSettingsCommand(true, "gpt 6", 0, 0, "gpt-6.1-terra", 0, 0, 5, 0, Admin)).IsValid, Is.False);
        Assert.That(settings.Validate(new UpdateAiSettingsCommand(true, "gpt-6-luna", 0.1m, 0.5m, "gpt-6.1-terra", 1, 5, 5, 0, Admin)).IsValid, Is.True);
        var key = new SetAiKeyCommandValidator();
        Assert.That(key.Validate(new SetAiKeyCommand("sk short", Admin)).IsValid, Is.False);
    }
}
