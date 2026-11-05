namespace Ai.Domain.Prompts;

/// <summary>
/// Extra rules the admin writes for one kind of request (e.g. «Умови розпізнавання рахунків»). They are added after the
/// fixed instruction of the module, which keeps the answer format.
/// </summary>
public sealed class AiPrompt
{
    public const int KeyMaxLength = 60;
    public const int TextMaxLength = 4_000;

    private AiPrompt() { }

    public AiPrompt(string key) => Id = key;

    public string Id { get; private set; } = null!;
    public string Text { get; private set; } = string.Empty;
    public string? UpdatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public bool Update(string text, string by, DateTimeOffset now)
    {
        text = text.Trim();
        if (text == Text) return false;
        Text = text;
        UpdatedBy = by.Length > 256 ? by[..256] : by;
        UpdatedAt = now;
        return true;
    }
}
