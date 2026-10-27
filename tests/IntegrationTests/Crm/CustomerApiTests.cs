using Crm.Application.Customers.GetCustomers.DTOs;
using Crm.Application.Customers.Contracts.Responses;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Crm.Api;
using Crm.Api.Customers.Requests;
using Crm.Application.Customers.CreateCustomer;
using Crm.Application.Customers.GetCustomers;
using IntegrationTests.Crm.Support;
using Mediator;
using Moq;

namespace IntegrationTests.Crm;

[NonParallelizable]
public sealed class CustomerApiTests
{
    [Test]
    public async Task AnonymousAndUserCannotReadAndManagerCannotMerge()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        Assert.That((await host.Client.GetAsync("/api/crm/customers")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using (CrmApiTestHost.AsRole(host.Client, "User", "22"))
            Assert.That((await host.Client.GetAsync("/api/crm/customers")).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        using (CrmApiTestHost.AsRole(host.Client, "Manager", "22"))
            Assert.That((await host.Client.PostAsJsonAsync($"/api/crm/customers/{Guid.NewGuid()}/merge", new { })).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task ListPreservesValueAndPagedInfoAndRejectsInvalidPageBeforeSender()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<GetCustomersQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<PagedResult<IReadOnlyList<CustomerSummary>>>(new PagedResult<IReadOnlyList<CustomerSummary>>(new PagedInfo(2, 25, 3, 51), [])));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "22");
        var response = await host.Client.GetAsync("/api/crm/customers?page=2");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json.GetProperty("value").GetArrayLength(), Is.Zero);
        Assert.That(json.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt64(), Is.EqualTo(51));
        Assert.That((await host.Client.GetAsync("/api/crm/customers?page=0")).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.Is<GetCustomersQuery>(q => q.Actor.UserId == 22 && q.Page == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreationUsesAuthenticatedActorAndReturnsCreatedLocation()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var id = Guid.NewGuid();
        host.Sender.Setup(x => x.Send(It.IsAny<CreateCustomerCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<CustomerDetail>>(Result.Success(new CustomerDetail(id, "person", "Synthetic", null, null,
                22, true, 1, DateTimeOffset.UtcNow, true, false, [], []))));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "22");
        var response = await host.Client.PostAsJsonAsync("/api/crm/customers", new
        {
            requestId = Guid.NewGuid(),
            actor = new { userId = 999, isAdmin = true },
            input = new { kind = "person", displayName = "Synthetic", legalName = (string?)null, taxIdentity = (string?)null, externalIdentities = Array.Empty<object>() }
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location!.AbsolutePath, Is.EqualTo($"/api/crm/customers/{id}"));
        host.Sender.Verify(x => x.Send(It.Is<CreateCustomerCommand>(c => c.Actor.UserId == 22 && !c.Actor.IsAdmin), It.IsAny<CancellationToken>()), Times.Once);
    }

}
