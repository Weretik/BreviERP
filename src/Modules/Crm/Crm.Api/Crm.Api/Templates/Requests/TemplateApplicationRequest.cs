using System.Text.Json.Serialization;
namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record TemplateApplicationRequest([property: JsonRequired] string Kind, [property: JsonRequired] string Location);
