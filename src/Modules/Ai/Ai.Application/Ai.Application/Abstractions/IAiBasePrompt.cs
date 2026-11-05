namespace Ai.Application.Abstractions;

/// <summary>The fixed instruction a module sends for one request kind; shown read-only in «Налаштування ШІ».</summary>
public interface IAiBasePrompt
{
    /// <summary>One of <see cref="Settings.AiPromptKeys"/>.</summary>
    string Key { get; }

    string Text { get; }
}
