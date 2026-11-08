using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.Identity;

/// <summary>«Права доступу» on a real Identity database: roles, saved rights, the user's own rights and the checks.</summary>
[NonParallelizable]
public sealed class AccessRightsApiTests
{
    private IdentityApiHost _host = null!;

    [OneTimeSetUp]
    public async Task StartHost() => _host = await IdentityApiHost.CreateAsync();

    [OneTimeTearDown]
    public async Task StopHost() => await _host.DisposeAsync();

    [Test]
    public async Task Built_in_roles_start_with_the_old_rules_and_the_director_is_locked()
    {
        using var admin = await AdminAsync();

        var page = await admin.Http.GetFromJsonAsync<JsonElement>("/api/admin/access");
        var roles = page.GetProperty("roles").EnumerateArray().ToDictionary(x => x.GetProperty("name").GetString()!);
        var director = await admin.Http.PutAsJsonAsync("/api/admin/access/roles/Director",
            new { version = 0, grants = new Dictionary<string, string[]>() });

        Assert.Multiple(() =>
        {
            Assert.That(page.GetProperty("areas").GetArrayLength(), Is.GreaterThan(20));
            Assert.That(roles["Director"].GetProperty("locked").GetBoolean(), Is.True);
            Assert.That(roles["Manager"].GetProperty("title").GetString(), Is.EqualTo("Менеджер"));
            Assert.That(roles["Manager"].GetProperty("version").GetInt32(), Is.Zero);
            Assert.That(roles["Manager"].GetProperty("grants").GetProperty("quotes").EnumerateArray().Select(x => x.GetString()),
                Is.EqualTo(new[] { "view", "create", "edit" }));
            Assert.That(roles["Admin"].GetProperty("userCount").GetInt32(), Is.EqualTo(1));
            Assert.That(director.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task A_new_role_gets_rights_users_and_is_deleted_only_without_users()
    {
        using var admin = await AdminAsync();

        var created = await admin.Http.PostAsJsonAsync("/api/admin/access/roles", new { title = "Технолог" });
        var role = await created.Content.ReadFromJsonAsync<JsonElement>();
        var name = role.GetProperty("name").GetString()!;
        var duplicate = await admin.Http.PostAsJsonAsync("/api/admin/access/roles", new { title = "технолог" });

        var saved = await admin.Http.PutAsJsonAsync($"/api/admin/access/roles/{name}",
            new { version = 0, grants = new Dictionary<string, string[]> { ["products"] = ["view"], ["media"] = ["view", "edit"] } });
        var stale = await admin.Http.PutAsJsonAsync($"/api/admin/access/roles/{name}",
            new { version = 0, grants = new Dictionary<string, string[]>() });
        var badAction = await admin.Http.PutAsJsonAsync($"/api/admin/access/roles/{name}",
            new { version = 1, grants = new Dictionary<string, string[]> { ["products"] = ["fly"] } });

        var user = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "tech@brevi.test", fullName = "Технолог", role = name, password = "Tech-Pass-1234" });
        var userId = (await user.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var users = await admin.Http.GetFromJsonAsync<JsonElement>("/api/admin/users");

        using var tech = await CookieAuthApiTests.SignInAsync(_host, "tech@brevi.test", "Tech-Pass-1234");
        var mine = await tech.Http.GetFromJsonAsync<JsonElement>("/api/auth/session/access");
        var quotes = await tech.Http.GetAsync("/api/crm/quotes");
        var profile = await tech.Http.GetAsync("/api/profile");

        var withUsers = await admin.Http.DeleteAsync($"/api/admin/access/roles/{name}");
        var builtIn = await admin.Http.DeleteAsync("/api/admin/access/roles/Manager");
        await admin.Http.DeleteAsync($"/api/admin/users/{userId}");
        var deleted = await admin.Http.DeleteAsync($"/api/admin/access/roles/{name}");

        Assert.Multiple(() =>
        {
            Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(name, Does.StartWith("custom-"));
            Assert.That(role.GetProperty("grants").EnumerateObject(), Is.Empty);
            Assert.That(duplicate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(saved.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(badAction.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(user.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(users.EnumerateArray().Single(x => x.GetProperty("id").GetInt32() == userId).GetProperty("role").GetString(),
                Is.EqualTo(name));
            Assert.That(mine.GetProperty("allowAll").GetBoolean(), Is.False);
            Assert.That(mine.GetProperty("roles")[0].GetProperty("title").GetString(), Is.EqualTo("Технолог"));
            Assert.That(mine.GetProperty("grants").GetProperty("media").EnumerateArray().Select(x => x.GetString()),
                Is.EqualTo(new[] { "view", "edit" }));
            Assert.That(quotes.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(profile.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(withUsers.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(builtIn.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deleted.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        });
    }

    [Test]
    public async Task Saved_rights_replace_the_defaults_at_once()
    {
        using var admin = await AdminAsync();
        await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "chief@brevi.test", fullName = "Начальник", role = "WorkshopChief", password = "Chief-Pass-1234" });
        using var chief = await CookieAuthApiTests.SignInAsync(_host, "chief@brevi.test", "Chief-Pass-1234");
        var before = await chief.Http.GetFromJsonAsync<JsonElement>("/api/auth/session/access");

        var page = await admin.Http.GetFromJsonAsync<JsonElement>("/api/admin/access");
        var version = page.GetProperty("roles").EnumerateArray().Single(x => x.GetProperty("name").GetString() == "WorkshopChief")
            .GetProperty("version").GetInt32();
        var saved = await admin.Http.PutAsJsonAsync("/api/admin/access/roles/WorkshopChief",
            new { version, grants = new Dictionary<string, string[]> { ["quotes"] = ["view"] } });
        var after = await chief.Http.GetFromJsonAsync<JsonElement>("/api/auth/session/access");
        var payroll = await chief.Http.GetAsync("/api/production/payroll/history/2026");

        Assert.Multiple(() =>
        {
            Assert.That(before.GetProperty("grants").TryGetProperty("payroll", out _), Is.True);
            Assert.That(saved.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(after.GetProperty("grants").EnumerateObject().Select(x => x.Name), Is.EqualTo(new[] { "quotes" }));
            Assert.That(payroll.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        });
    }

    private Task<BrowserClient> AdminAsync() =>
        CookieAuthApiTests.SignInAsync(_host, IdentityApiHost.AdminEmail, IdentityApiHost.AdminPassword);
}
