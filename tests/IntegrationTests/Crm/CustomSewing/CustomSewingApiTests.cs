using System.Net;
using System.Net.Http.Json;
using Ardalis.Result;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.CreateDraft;
using Crm.Application.CustomSewing.SaveVersion;
using IntegrationTests.Crm.Support;
using Moq;

namespace IntegrationTests.Crm.CustomSewing;

[NonParallelizable]
public sealed class CustomSewingApiTests
{
    [Test]
    public async Task Anonymous_and_User_cannot_read_private_custom_drafts()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var uri = "/api/crm/custom-sewing/" + Guid.NewGuid();
        Assert.That((await host.Client.GetAsync(uri)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using var actor = CrmApiTestHost.AsRole(host.Client, "User", "22");
        Assert.That((await host.Client.GetAsync(uri)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Posted_prices_are_rejected_on_version_save_before_the_handler()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var response = await host.Client.PostAsJsonAsync("/api/crm/custom-sewing/" + Guid.NewGuid() + "/versions",
            new { requestId = Guid.NewGuid(), expectedRevision = 1, basisToken = Guid.NewGuid(), retail = 1 });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<SaveCustomVersionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Nested_material_cost_cannot_be_submitted_as_a_trusted_reference_value()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var configuration = new
        {
            name = "Synthetic",
            productType = "Jacket",
            season = "winter",
            description = "",
            clientComment = "",
            internalNote = "",
            fabrics = new[] { new { referenceId = 1, quantity = 1, unit = "Meters", unitCost = 1 } },
            accessories = Array.Empty<object>(),
            operations = Array.Empty<object>(),
            applications = Array.Empty<object>(),
            delivery = 0,
            packaging = 0
        };
        var response = await host.Client.PostAsJsonAsync("/api/crm/custom-sewing", new { requestId = Guid.NewGuid(), configuration });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<CreateCustomDraftCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Create_uses_the_authenticated_actor_and_returns_a_private_draft_location()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var id = Guid.NewGuid();
        var detail = new CustomDetail(id, 1, 17, null, null, CustomSewingPersistenceTests.Configuration, null, null,
            DateTimeOffset.UtcNow, true);
        host.Sender.Setup(x => x.Send(It.IsAny<CreateCustomDraftCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<CustomDetail>>(Result.Success(detail)));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var response = await host.Client.PostAsJsonAsync("/api/crm/custom-sewing", new
        {
            requestId = Guid.NewGuid(),
            configuration = CustomSewingPersistenceTests.Configuration
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location!.ToString(), Does.EndWith(id.ToString()));
        host.Sender.Verify(x => x.Send(It.Is<CreateCustomDraftCommand>(command => command.Actor.UserId == 17
            && !command.Actor.IsAdmin && command.Configuration.InternalNote == "Private note"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
