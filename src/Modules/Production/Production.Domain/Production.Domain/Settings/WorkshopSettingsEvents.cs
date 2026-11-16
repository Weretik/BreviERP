using BuildingBlocks.Domain.Events;

namespace Production.Domain.Settings;

/// <summary>The workshop Telegram settings were saved (ACT-001).</summary>
public sealed record WorkshopSettingsChanged(bool TelegramEnabled) : IDomainEvent;
