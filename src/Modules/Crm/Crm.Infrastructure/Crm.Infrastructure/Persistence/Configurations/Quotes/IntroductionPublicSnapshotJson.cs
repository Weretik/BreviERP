using System.Text.Json;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

internal static class IntroductionPublicSnapshotJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    internal static string Serialize(IntroductionPublicSnapshot snapshot) =>
        JsonSerializer.Serialize(snapshot, Options);

    internal static IntroductionPublicSnapshot Deserialize(string json)
    {
        var snapshot = JsonSerializer.Deserialize<IntroductionPublicSnapshot>(json, Options)
            ?? throw new InvalidDataException("Introduction public snapshot is empty.");
        if (snapshot.Slots is null || snapshot.MappingVersionId == Guid.Empty ||
            snapshot.EmailTemplateVersionId == Guid.Empty ||
            snapshot.Slots.Count != IntroductionSlot.Required(snapshot.Layout).Count)
            throw new InvalidDataException("Introduction public snapshot is invalid.");
        return snapshot with { Slots = Array.AsReadOnly(snapshot.Slots.ToArray()) };
    }
}
