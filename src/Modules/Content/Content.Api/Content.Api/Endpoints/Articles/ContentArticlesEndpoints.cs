using Content.Api.Access;
using Content.Api.Endpoints.Articles.CreateContentArticle;
using Content.Api.Endpoints.Articles.DeleteContentArticle;
using Content.Api.Endpoints.Articles.GetContentArticle;
using Content.Api.Endpoints.Articles.GetContentArticles;
using Content.Api.Endpoints.Articles.ImportOldSiteArticles;
using Content.Api.Endpoints.Articles.SaveContentArticle;
using Content.Api.Endpoints.Articles.SetContentArticleVisibility;

namespace Content.Api.Endpoints.Articles;

/// <summary>«Контент → Статті»: articles of the public site with their SEO and photos (CNT-004).</summary>
internal static class ContentArticlesEndpoints
{
    public static IEndpointRouteBuilder MapContentArticlesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/articles").RequireAuthorization(ContentAccess.ArticlesPolicy)
            .WithTags("ContentArticles");
        GetContentArticlesEndpoint.Map(group);
        GetContentArticleEndpoint.Map(group);
        CreateContentArticleEndpoint.Map(group);
        SaveContentArticleEndpoint.Map(group);
        SetContentArticleVisibilityEndpoint.Map(group);
        ImportOldSiteArticlesEndpoint.Map(group);
        DeleteContentArticleEndpoint.Map(group);
        return app;
    }
}
