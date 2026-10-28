using System.Net;
using Ardalis.Result;
using Crm.Application.CustomSewing.GetTemplate;
using Crm.Application.CustomSewing.Templates.Contracts;
using System.Net.Http.Json;
using Crm.Application.CustomSewing.CreateTemplate;
using Crm.Application.CustomSewing.ApplyTemplate;
using IntegrationTests.Crm.Support;
using Moq;
namespace IntegrationTests.Crm.CustomSewing;

[NonParallelizable]
public sealed class CustomTemplateApiTests
{
    [Test]
    public async Task Anonymous_and_user_roles_cannot_read_templates_or_suggestions()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        Assert.That((await host.Client.GetAsync("/api/crm/quote-templates")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using var user = CrmApiTestHost.AsRole(host.Client, "User", "22");
        Assert.That((await host.Client.GetAsync("/api/crm/quote-templates")).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That((await host.Client.GetAsync("/api/crm/custom-sewing/suggestions?productType=Куртка&season=winter")).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
    [Test]
    public async Task Price_or_private_note_in_template_body_is_rejected_before_dispatch()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var manager = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var selections = new { name = "Куртка", productType = "Куртка", season = "winter", description = "", clientComment = "",
            internalNote = "Must never become reusable", fabrics = Array.Empty<object>(), accessories = Array.Empty<object>(),
            operations = Array.Empty<object>(), applications = Array.Empty<object>() };
        var response = await host.Client.PostAsJsonAsync("/api/crm/quote-templates",
            new { requestId = Guid.NewGuid(), kind = "customConfiguration", introduction = "Intro", selections });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<CreateTemplateCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Test]
    public async Task First_contact_and_missing_archived_flag_and_posted_price_on_apply_are_rejected()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var manager = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var id = Guid.NewGuid();
        var unknown = await host.Client.PostAsJsonAsync("/api/crm/quote-templates", new { requestId = Guid.NewGuid(), kind = "firstContact",
            introduction = "", selections = new { name = "X", productType = "Куртка", season = "winter", description = "", clientComment = "",
            fabrics = Array.Empty<object>(), accessories = Array.Empty<object>(), operations = Array.Empty<object>(), applications = Array.Empty<object>() } });
        Assert.That(unknown.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That((await host.Client.PutAsJsonAsync($"/api/crm/quote-templates/{id}/archive",
            new { requestId = Guid.NewGuid(), expectedRevision = 1 })).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That((await host.Client.PostAsJsonAsync($"/api/crm/quote-templates/{id}/apply",
            new { requestId = Guid.NewGuid(), expectedRevision = 1, versionId = Guid.NewGuid(), retail = 99 })).StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<ApplyTemplateCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Test]
    public async Task Known_inaccessible_template_returns_not_found_and_keeps_authenticated_actor()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var id = Guid.NewGuid();
        host.Sender.Setup(x => x.Send(It.IsAny<GetTemplateQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<TemplateDetail>>(Result.NotFound()));
        using var manager = CrmApiTestHost.AsRole(host.Client, "Manager", "18");
        Assert.That((await host.Client.GetAsync($"/api/crm/quote-templates/{id}")).StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        host.Sender.Verify(x => x.Send(It.Is<GetTemplateQuery>(q => q.TemplateId == id && q.Actor.UserId == 18 && !q.Actor.IsAdmin),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
