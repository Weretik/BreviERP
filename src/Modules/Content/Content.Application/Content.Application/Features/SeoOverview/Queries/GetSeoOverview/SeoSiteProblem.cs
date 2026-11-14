using System.Text.Json.Serialization;

namespace Content.Application.Features.SeoOverview.Queries.GetSeoOverview;

/// <summary>Site-wide settings that are not filled (CNT-008 R-03).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SeoSiteProblem>))]
public enum SeoSiteProblem
{
    NoAnalytics,
    NoGoogleVerification,
    NoBingVerification,
    NoSharePicture,
}
