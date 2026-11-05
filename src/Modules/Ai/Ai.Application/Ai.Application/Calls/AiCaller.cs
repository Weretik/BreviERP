using Ai.Application.Abstractions;
using Ai.Application.Contracts;
using Ai.Domain.Settings;
using Ai.Domain.Usage;

namespace Ai.Application.Calls;

/// <summary>
/// The only place that sends requests to OpenAI: decrypts the key, calls the model of the tier, and adds the tokens and
/// cost to the month's use. Failures come back as messages in Ukrainian; the key never leaves this class.
/// </summary>
public sealed class AiCaller(
    IAiDbContext db,
    IAiKeyProtector protector,
    IOpenAiClient client,
    TimeProvider clock,
    ILogger<AiCaller> logger)
{
    public const string Disabled = "ШІ вимкнено в «Налаштування ШІ».";
    public const string NoKey = "Ключ OpenAI не додано в «Налаштування ШІ».";
    public const string KeyUnreadable = "Збережений ключ OpenAI не вдається прочитати. Додайте ключ знову.";
    public const string LimitReached = "Ліміт витрат на ШІ цього місяця вичерпано. Його можна змінити в «Налаштування ШІ».";

    /// <summary>
    /// <paramref name="requireEnabled"/> is false only for «Перевірити», which works before the AI is switched on.
    /// Unavailable: the AI is off, has no key or reached the limit (nothing was sent). Error: OpenAI refused or failed.
    /// </summary>
    public async Task<Result<AiExtraction>> CallAsync(AiModelTier tier, string instructions, string text,
        IReadOnlyList<AiFile> files, string schemaName, string schemaJson, bool requireEnabled,
        CancellationToken cancellationToken, string? promptKey = null)
    {
        var settings = await db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == AiSettings.OpenAi, cancellationToken);
        if (settings?.ProtectedApiKey is null) return Result<AiExtraction>.Unavailable(NoKey);
        if (requireEnabled && !settings.Enabled) return Result<AiExtraction>.Unavailable(Disabled);

        var month = AiUsageMonth.IdOf(clock.GetUtcNow());
        var spent = await db.UsageMonths.AsNoTracking().Where(x => x.Id == month).Select(x => x.CostUsd)
            .FirstOrDefaultAsync(cancellationToken);
        if (spent >= settings.MonthlyLimitUsd) return Result<AiExtraction>.Unavailable(LimitReached);

        var apiKey = protector.Unprotect(settings.ProtectedApiKey);
        if (apiKey is null) return Result<AiExtraction>.Unavailable(KeyUnreadable);

        if (promptKey is not null)
        {
            var extra = await db.Prompts.AsNoTracking().Where(x => x.Id == promptKey).Select(x => x.Text)
                .FirstOrDefaultAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(extra))
                instructions += "\n\nAdditional rules from the Brevi admin (they never change the answer format):\n" + extra;
        }

        var model = settings.Model(tier);
        OpenAiAnswer answer;
        try
        {
            answer = await client.RespondAsync(
                new OpenAiRequest(apiKey, model, instructions, text, files, schemaName, schemaJson), cancellationToken);
        }
        catch (OpenAiException error)
        {
            logger.LogWarning("OpenAI call with {Model} failed: {Failure}", model, error.Failure);
            return Result<AiExtraction>.Error(Describe(error, model));
        }

        var cost = settings.Price(tier).Cost(answer.InputTokens, answer.OutputTokens);
        await AddUsageAsync(month, answer, cost, cancellationToken);
        return Result.Success(new AiExtraction(answer.OutputJson, model, answer.InputTokens, answer.OutputTokens, cost));
    }

    private async Task AddUsageAsync(string month, OpenAiAnswer answer, decimal cost, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var updated = await db.UsageMonths.Where(x => x.Id == month).ExecuteUpdateAsync(set => set
                .SetProperty(x => x.Requests, x => x.Requests + 1)
                .SetProperty(x => x.InputTokens, x => x.InputTokens + answer.InputTokens)
                .SetProperty(x => x.OutputTokens, x => x.OutputTokens + answer.OutputTokens)
                .SetProperty(x => x.CostUsd, x => x.CostUsd + cost), cancellationToken);
            if (updated > 0) return;

            var row = new AiUsageMonth(month);
            db.UsageMonths.Add(row);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (attempt == 0)
            {
                // Another request added the month first: count on top of it.
                db.UsageMonths.Entry(row).State = EntityState.Detached;
            }
        }
    }

    private static string Describe(OpenAiException error, string model) => error.Failure switch
    {
        OpenAiFailure.InvalidKey => "OpenAI не прийняв ключ. Перевірте його в «Налаштування ШІ».",
        OpenAiFailure.ModelNotFound => $"Модель «{model}» недоступна для цього ключа. Змініть її в «Налаштування ШІ».",
        OpenAiFailure.QuotaOrRateLimit => "OpenAI відхилив запит через ліміт або баланс рахунку. Спробуйте пізніше або поповніть баланс.",
        OpenAiFailure.Rejected => $"OpenAI відхилив запит: {error.Message}",
        _ => "OpenAI не відповідає. Спробуйте пізніше.",
    };
}
