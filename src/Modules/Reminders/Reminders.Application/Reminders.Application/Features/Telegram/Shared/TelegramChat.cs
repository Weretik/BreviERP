using System.Text.Json.Serialization;

namespace Reminders.Application.Features.Telegram.Shared;

public sealed record TelegramChat(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("first_name")] string? FirstName,
    [property: JsonPropertyName("last_name")] string? LastName,
    [property: JsonPropertyName("username")] string? Username)
{
    public string DisplayTitle => Title
        ?? string.Join(' ', new[] { FirstName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x))) switch
        {
            "" => Username is null ? $"id {Id}" : "@" + Username,
            var name => name
        };
}
