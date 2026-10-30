using System.Text.Json;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Infrastructure.Persistence.Configurations.Introductions;

internal static class IntroductionTemplatePayloadJson
{
    public static string Serialize(IntroductionTemplatePayload payload) => JsonSerializer.Serialize(payload);

    public static IntroductionTemplatePayload Deserialize(string json)
    {
        var payload = JsonSerializer.Deserialize<IntroductionTemplatePayload>(json)
            ?? throw new InvalidDataException("Introduction template payload is empty.");
        payload.ToSnapshot();
        return payload;
    }
}
