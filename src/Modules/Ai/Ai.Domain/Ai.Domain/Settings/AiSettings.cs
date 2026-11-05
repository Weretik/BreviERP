namespace Ai.Domain.Settings;

/// <summary>
/// «Налаштування ШІ»: the one OpenAI account of Brevi. The API key is stored only encrypted (<see cref="ProtectedApiKey"/>)
/// and never shown again; <see cref="KeyHint"/> keeps its last four characters so the admin can tell which key is set.
/// </summary>
public sealed class AiSettings
{
    public const string OpenAi = "openai";
    public const int ModelMaxLength = 80;
    public const int UpdatedByMaxLength = 256;
    public const decimal MonthlyLimitMax = 10_000m;
    public const string DefaultFastModel = "gpt-6-luna";
    public const string DefaultStrongModel = "gpt-6.1-terra";

    private AiSettings() { }

    public string Id { get; private set; } = OpenAi;
    public string? ProtectedApiKey { get; private set; }
    public string? KeyHint { get; private set; }

    /// <summary>Off: other modules get «ШІ вимкнено» and nothing is sent to OpenAI.</summary>
    public bool Enabled { get; private set; }

    public string FastModel { get; private set; } = DefaultFastModel;
    public AiModelPrice FastPrice { get; private set; } = new(0.10m, 0.50m);
    public string StrongModel { get; private set; } = DefaultStrongModel;
    public AiModelPrice StrongPrice { get; private set; } = new(1.00m, 5.00m);

    /// <summary>Requests stop once the cost of the current month (UTC) reaches it.</summary>
    public decimal MonthlyLimitUsd { get; private set; } = 5m;

    public int Version { get; private set; }
    public string? UpdatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public bool HasKey => ProtectedApiKey is not null;

    public static AiSettings CreateDefault() => new();

    public string Model(AiModelTier tier) => tier == AiModelTier.Strong ? StrongModel : FastModel;

    public AiModelPrice Price(AiModelTier tier) => tier == AiModelTier.Strong ? StrongPrice : FastPrice;

    /// <summary>Returns false when nothing changed (no new version).</summary>
    public bool Update(bool enabled, string fastModel, AiModelPrice fastPrice, string strongModel, AiModelPrice strongPrice,
        decimal monthlyLimitUsd, string by, DateTimeOffset now)
    {
        fastModel = fastModel.Trim();
        strongModel = strongModel.Trim();
        if (Enabled == enabled && FastModel == fastModel && FastPrice == fastPrice && StrongModel == strongModel
            && StrongPrice == strongPrice && MonthlyLimitUsd == monthlyLimitUsd)
            return false;

        Enabled = enabled;
        FastModel = fastModel;
        FastPrice = fastPrice;
        StrongModel = strongModel;
        StrongPrice = strongPrice;
        MonthlyLimitUsd = monthlyLimitUsd;
        Touch(by, now);
        return true;
    }

    public void SetKey(string protectedApiKey, string hint, string by, DateTimeOffset now)
    {
        ProtectedApiKey = protectedApiKey;
        KeyHint = hint;
        Touch(by, now);
    }

    /// <summary>Removing the key also switches the AI off.</summary>
    public bool RemoveKey(string by, DateTimeOffset now)
    {
        if (ProtectedApiKey is null && !Enabled) return false;
        ProtectedApiKey = null;
        KeyHint = null;
        Enabled = false;
        Touch(by, now);
        return true;
    }

    /// <summary>Last four characters of the key; the rest is never kept in clear text.</summary>
    public static string HintOf(string apiKey) => apiKey.Length <= 4 ? apiKey : apiKey[^4..];

    private void Touch(string by, DateTimeOffset now)
    {
        UpdatedBy = by.Length > UpdatedByMaxLength ? by[..UpdatedByMaxLength] : by;
        UpdatedAt = now;
        Version++;
    }
}
