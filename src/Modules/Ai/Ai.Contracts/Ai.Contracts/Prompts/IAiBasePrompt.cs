namespace Ai.Contracts.Prompts;

/// <summary>The fixed instruction a module sends for one request kind; shown read-only in «Налаштування ШІ».</summary>
public interface IAiBasePrompt
{
    /// <summary>One of <see cref="AiPromptKeys"/>.</summary>
    string Key { get; }

    string Text { get; }
}
