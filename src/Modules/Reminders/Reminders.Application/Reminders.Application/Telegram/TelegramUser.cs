using System.Text.Json.Serialization;

namespace Reminders.Application.Telegram;

public sealed record TelegramUser(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("is_bot")] bool IsBot,
    [property: JsonPropertyName("first_name")] string? FirstName,
    [property: JsonPropertyName("last_name")] string? LastName,
    [property: JsonPropertyName("username")] string? Username);
