using System.Text.Json.Serialization;
namespace Crm.Api.Documents.EmailTemplates;
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateEmailTemplateRequest([property: JsonRequired] Guid RequestId,
    [property: JsonRequired] string Name, [property: JsonRequired] string Subject,
    [property: JsonRequired] string Body);
