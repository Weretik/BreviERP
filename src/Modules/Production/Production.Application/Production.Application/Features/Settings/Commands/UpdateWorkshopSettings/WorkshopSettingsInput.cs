namespace Production.Application.Features.Settings.Commands.UpdateWorkshopSettings;

/// <param name="TelegramChatId">The workshop group; sending needs it.</param>
/// <param name="TelegramThreadId">The topic in a group with topics; null posts to the main chat.</param>
public sealed record WorkshopSettingsInput(bool TelegramEnabled, long? TelegramChatId, int? TelegramThreadId);
