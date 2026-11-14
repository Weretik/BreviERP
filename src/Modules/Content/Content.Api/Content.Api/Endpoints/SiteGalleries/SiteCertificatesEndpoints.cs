using Content.Api.Access;

namespace Content.Api.Endpoints.SiteGalleries;

/// <summary>«Контент → Сертифікати» (CNT-011).</summary>
internal static class SiteCertificatesEndpoints
{
    public static IEndpointRouteBuilder MapSiteCertificatesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/certificates").RequireAuthorization(ContentAccess.CertificatesPolicy)
            .WithTags("SiteCertificates");
        GetSiteCertificatesEndpoint.Map(group);
        SaveSiteCertificatesEndpoint.Map(group);
        TransferSiteCertificatesEndpoint.Map(group);
        return app;
    }
}
