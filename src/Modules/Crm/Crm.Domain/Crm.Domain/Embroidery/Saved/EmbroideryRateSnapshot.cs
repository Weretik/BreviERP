using Crm.Domain.Embroidery.Settings;

namespace Crm.Domain.Embroidery.Saved;

/// <summary>
/// The rates a saved calculation was priced with, frozen at save time: the stitch rate from «Параметри
/// ціноутворення» and the whole «Налаштування». Changing the rates later never changes a saved result.
/// </summary>
public sealed record EmbroideryRateSnapshot(decimal StitchRate, EmbroiderySettingsContent Settings);
