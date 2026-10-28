using Ardalis.Specification.EntityFrameworkCore;
using Crm.Application.CustomSewing.GetTemplate.Specifications;
using Crm.Application.CustomSewing.GetTemplates.Specifications;
using Crm.Domain.CustomSewing.Templates;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace IntegrationTests.Crm.CustomSewing;

public sealed class TemplateQueryTranslationTests
{
    [Test]
    public void Template_scopes_and_scalar_list_projection_translate_without_loading_history_or_private_prices()
    {
        using var db = new CrmDbContext(new DbContextOptionsBuilder<CrmDbContext>()
            .UseNpgsql("Host=localhost;Database=sql_translation_only;Username=test;Password=test").Options);
        var detail = SpecificationEvaluator.Default.GetQuery(db.Set<CustomTemplateVersion>(),
            new TemplateDetailSpec(Guid.NewGuid(), Guid.NewGuid(), new(17, false))).ToQueryString();
        var page = SpecificationEvaluator.Default.GetQuery(db.Set<CustomTemplateVersion>(),
            new TemplatePageSpec(new(17, false), false, 2, 10)).ToQueryString();
        Assert.That(detail, Does.Contain("CustomSewingTemplateVersions").And.Contain("OwnerId"));
        Assert.That(page, Does.Contain("LIMIT").And.Contain("OFFSET").And.Not.Contain("Selections"));
    }
}
