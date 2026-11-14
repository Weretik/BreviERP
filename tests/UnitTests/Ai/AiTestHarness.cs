using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Application.Calls;
using Ai.Application.Settings;
using Ai.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace UnitTests.Ai;

/// <summary>Reversible stand-in for Data Protection, so tests can see that the stored value is not the key.</summary>
internal sealed class FakeKeyProtector : IAiKeyProtector
{
    public string Protect(string apiKey) => "protected:" + new string(apiKey.Reverse().ToArray());

    public string? Unprotect(string protectedApiKey) =>
        protectedApiKey.StartsWith("protected:", StringComparison.Ordinal)
            ? new string(protectedApiKey["protected:".Length..].Reverse().ToArray())
            : null;
}

internal sealed class FakeBasePrompt : IAiBasePrompt
{
    public const string Instruction = "Read the invoice as printed.";

    public string Key => AiPromptKeys.PurchaseInvoices;

    public string Text => Instruction;
}

internal sealed class FakeOpenAiClient : IOpenAiClient
{
    public List<OpenAiRequest> Requests { get; } = [];
    public OpenAiException? Failure { get; set; }
    public OpenAiAnswer Answer { get; set; } = new("""{"ok":true}""", 1_000_000, 100_000);

    public Task<OpenAiAnswer> RespondAsync(OpenAiRequest request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Failure is not null ? Task.FromException<OpenAiAnswer>(Failure) : Task.FromResult(Answer);
    }
}

internal sealed class AiTestHarness : IDisposable
{
    private readonly SqliteConnection connection;

    public AiTestHarness()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        Db = Fresh();
        Db.Database.EnsureCreated();
    }

    public AiDbContext Db { get; }
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 5, 8, 0, 0, TimeSpan.Zero));
    public FakeKeyProtector Protector { get; } = new();
    public FakeOpenAiClient OpenAi { get; } = new();

    public AiSettingsStore Store() => new(Db, Time, [new FakeBasePrompt()]);

    public AiCaller Caller() => new(Db, Protector, OpenAi, Time, NullLogger<AiCaller>.Instance);

    public AiDbContext Fresh() => new(new DbContextOptionsBuilder<AiDbContext>().UseSqlite(connection).Options);

    public void Dispose()
    {
        Db.Dispose();
        connection.Dispose();
    }
}
