using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.Identity;

[NonParallelizable]
public sealed class AdminUsersApiTests
{
    private IdentityApiHost _host = null!;

    [OneTimeSetUp]
    public async Task StartHost() => _host = await IdentityApiHost.CreateAsync();

    [OneTimeTearDown]
    public async Task StopHost() => await _host.DisposeAsync();

    [Test]
    public async Task Admin_creates_a_manager_who_signs_in_but_cannot_manage_users()
    {
        using var admin = await AdminAsync();
        var created = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "manager1@brevi.test", fullName = "Менеджер", role = "Manager", password = "Manager-Pass-1", phoneNumber = "+380 50 123 45 67" });
        var list = await admin.Http.GetFromJsonAsync<JsonElement>("/api/admin/users");

        using var manager = await CookieAuthApiTests.SignInAsync(_host, "manager1@brevi.test", "Manager-Pass-1");
        var managerList = await manager.Http.GetAsync("/api/admin/users");

        Assert.Multiple(() =>
        {
            Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(list.EnumerateArray().Select(x => x.GetProperty("email").GetString()), Does.Contain("manager1@brevi.test"));
            Assert.That(managerList.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(
                list.EnumerateArray().Single(x => x.GetProperty("email").GetString() == "manager1@brevi.test").GetProperty("phoneNumber").GetString(),
                Is.EqualTo("+380 50 123 45 67"));
        });
    }

    [Test]
    public async Task Invalid_phone_is_rejected_on_create()
    {
        using var admin = await AdminAsync();

        var response = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "badphone@brevi.test", fullName = "Phone", role = "Manager", password = "Long-Enough-1", phoneNumber = "abc" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Short_password_and_duplicate_email_are_rejected()
    {
        using var admin = await AdminAsync();

        var shortPassword = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "short@brevi.test", fullName = "Short", role = "Manager", password = "1234567" });
        var duplicate = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = IdentityApiHost.AdminEmail, fullName = "Dup", role = "Manager", password = "Long-Enough-1" });

        Assert.Multiple(() =>
        {
            Assert.That(shortPassword.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(duplicate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        });
    }

    [Test]
    public async Task Blocked_staff_cannot_sign_in_and_admin_cannot_block_self()
    {
        using var admin = await AdminAsync();
        var created = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "manager2@brevi.test", fullName = "Менеджер 2", role = "Manager", password = "Manager-Pass-2" });
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var me = await admin.Http.GetFromJsonAsync<JsonElement>("/api/auth/session/me");
        var adminId = int.Parse(me.GetProperty("userId").GetString()!);

        var block = await admin.Http.PutAsJsonAsync($"/api/admin/users/{id}/blocked", new { blocked = true });
        var blockSelf = await admin.Http.PutAsJsonAsync($"/api/admin/users/{adminId}/blocked", new { blocked = true });
        var demoteSelf = await admin.Http.PutAsJsonAsync($"/api/admin/users/{adminId}/role", new { role = "Manager" });

        using var browser = _host.CreateBrowser();
        var blockedLogin = await browser.Http.PostAsJsonAsync("/api/auth/cookie/login",
            new { email = "manager2@brevi.test", password = "Manager-Pass-2" });

        Assert.Multiple(() =>
        {
            Assert.That(block.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(blockSelf.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(demoteSelf.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(blockedLogin.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    public async Task New_password_works_and_five_wrong_passwords_lock_the_account()
    {
        using var admin = await AdminAsync();
        var created = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "manager3@brevi.test", fullName = "Менеджер 3", role = "Manager", password = "Manager-Pass-3" });
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var reset = await admin.Http.PutAsJsonAsync($"/api/admin/users/{id}/password", new { password = "New-Manager-Pass-3" });
        using (await CookieAuthApiTests.SignInAsync(_host, "manager3@brevi.test", "New-Manager-Pass-3"))
        {
        }

        using var browser = _host.CreateBrowser();
        for (var i = 0; i < 5; i++)
            await browser.Http.PostAsJsonAsync("/api/auth/cookie/login", new { email = "manager3@brevi.test", password = "wrong-password" });
        var afterLockout = await browser.Http.PostAsJsonAsync("/api/auth/cookie/login",
            new { email = "manager3@brevi.test", password = "New-Manager-Pass-3" });

        Assert.Multiple(() =>
        {
            Assert.That(reset.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(afterLockout.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    public async Task One_director_whom_only_the_director_changes()
    {
        using var admin = await AdminAsync();
        var director = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "director@brevi.test", fullName = "Директор", role = "Director", password = "Director-Pass-1" });
        var directorId = (await director.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var secondDirector = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "director2@brevi.test", fullName = "Другий", role = "Director", password = "Director-Pass-2" });
        var accountant = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "accountant@brevi.test", fullName = "Бухгалтер", role = "Accountant", password = "Accountant-Pass-1" });
        var accountantId = (await accountant.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var promoteToDirector = await admin.Http.PutAsJsonAsync($"/api/admin/users/{accountantId}/role", new { role = "Director" });

        var adminBlocksDirector = await admin.Http.PutAsJsonAsync($"/api/admin/users/{directorId}/blocked", new { blocked = true });
        var adminResetsDirector = await admin.Http.PutAsJsonAsync($"/api/admin/users/{directorId}/password", new { password = "Taken-Over-1" });
        var adminDeletesDirector = await admin.Http.DeleteAsync($"/api/admin/users/{directorId}");
        var adminEditsDirector = await admin.Http.PutAsJsonAsync($"/api/admin/users/{directorId}/details",
            new { fullName = "Інший", email = "director@brevi.test", phoneNumber = (string?)null });
        var list = await admin.Http.GetFromJsonAsync<JsonElement>("/api/admin/users");
        var directorRow = list.EnumerateArray().Single(x => x.GetProperty("id").GetInt32() == directorId);

        using var signedInDirector = await CookieAuthApiTests.SignInAsync(_host, "director@brevi.test", "Director-Pass-1");
        var directorMe = await signedInDirector.Http.GetFromJsonAsync<JsonElement>("/api/auth/session/me");
        var directorOwnPassword = await signedInDirector.Http.PutAsJsonAsync($"/api/admin/users/{directorId}/password", new { password = "Director-Pass-New" });
        var directorBlocksSelf = await signedInDirector.Http.PutAsJsonAsync($"/api/admin/users/{directorId}/blocked", new { blocked = true });
        var directorDeletesAccountant = await signedInDirector.Http.DeleteAsync($"/api/admin/users/{accountantId}");

        Assert.Multiple(() =>
        {
            Assert.That(director.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(secondDirector.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(accountant.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(promoteToDirector.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(adminBlocksDirector.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(adminResetsDirector.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(adminDeletesDirector.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(adminEditsDirector.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(directorRow.GetProperty("role").GetString(), Is.EqualTo("Director"));
            Assert.That(directorMe.GetProperty("roles").EnumerateArray().Select(x => x.GetString()), Is.SupersetOf(new[] { "Director", "Admin" }));
            Assert.That(directorOwnPassword.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(directorBlocksSelf.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(directorDeletesAccountant.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        });
    }

    [Test]
    public async Task Admin_deletes_a_staff_account_but_not_their_own()
    {
        using var admin = await AdminAsync();
        var created = await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "manager4@brevi.test", fullName = "Менеджер 4", role = "Manager", password = "Manager-Pass-4" });
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var me = await admin.Http.GetFromJsonAsync<JsonElement>("/api/auth/session/me");
        var adminId = int.Parse(me.GetProperty("userId").GetString()!);

        var deleted = await admin.Http.DeleteAsync($"/api/admin/users/{id}");
        var deleteSelf = await admin.Http.DeleteAsync($"/api/admin/users/{adminId}");
        using var browser = _host.CreateBrowser();
        var deletedLogin = await browser.Http.PostAsJsonAsync("/api/auth/cookie/login",
            new { email = "manager4@brevi.test", password = "Manager-Pass-4" });

        Assert.Multiple(() =>
        {
            Assert.That(deleted.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(deleteSelf.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deletedLogin.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    private Task<BrowserClient> AdminAsync() =>
        CookieAuthApiTests.SignInAsync(_host, IdentityApiHost.AdminEmail, IdentityApiHost.AdminPassword);
}
