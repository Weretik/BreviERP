using System.Text.Json.Serialization;

namespace Content.Application.Features.SeoOverview.Shared;

/// <summary>Where a page of the site comes from in «Контент».</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SeoArea>))]
public enum SeoArea
{
    Product,
    Category,
    Article,
    Page,
}
