using System.Text.Json.Serialization;

namespace Ai.Domain.Settings;

/// <summary>Fast is the cheap model tried first; Strong is the retry when the fast answer fails its checks.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<AiModelTier>))]
public enum AiModelTier
{
    Fast = 0,
    Strong = 1,
}
