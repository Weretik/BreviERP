using System.Text.Json.Serialization;

namespace Catalog.Api.Endpoints.Sewing.CreateSewingSettingsVersion;

public sealed record CreateSewingSettingsVersionRequest(
    [property: JsonRequired] long ExpectedVersionId,
    [property: JsonRequired] Guid RequestId,
    [property: JsonRequired] SewingSettingsValuesRequest Settings);
