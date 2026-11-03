using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SkiaSharp;

namespace IntegrationTests.Identity;

[NonParallelizable]
public sealed class ProfileApiTests
{
    private IdentityApiHost _host = null!;

    [OneTimeSetUp]
    public async Task StartHost() => _host = await IdentityApiHost.CreateAsync();

    [OneTimeTearDown]
    public async Task StopHost() => await _host.DisposeAsync();

    [Test]
    public async Task Manager_changes_name_and_photo_but_not_email_or_password()
    {
        using var admin = await CookieAuthApiTests.SignInAsync(_host, IdentityApiHost.AdminEmail, IdentityApiHost.AdminPassword);
        await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "profile-manager@brevi.test", fullName = "Олена", role = "Manager", password = "Manager-Pass-9" });
        using var manager = await CookieAuthApiTests.SignInAsync(_host, "profile-manager@brevi.test", "Manager-Pass-9");

        var rename = await manager.Http.PutAsJsonAsync("/api/profile/name", new { fullName = "Олена Петрівна" });
        var upload = await manager.Http.PutAsync("/api/profile/avatar", Photo(400, 300));
        var avatarUrl = (await upload.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("avatarUrl").GetString();
        var profile = await manager.Http.GetFromJsonAsync<JsonElement>("/api/profile");
        var me = await manager.Http.GetFromJsonAsync<JsonElement>("/api/auth/session/me");
        var email = await manager.Http.PutAsJsonAsync("/api/profile/email", new { email = "x@brevi.test", currentPassword = "Manager-Pass-9" });
        var password = await manager.Http.PutAsJsonAsync("/api/profile/password", new { currentPassword = "Manager-Pass-9", newPassword = "Other-Pass-9" });
        var remove = await manager.Http.DeleteAsync("/api/profile/avatar");

        Assert.Multiple(() =>
        {
            Assert.That(rename.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(upload.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(avatarUrl, Does.StartWith("https://cdn.brevi.test/avatars/").And.EndWith(".webp"));
            Assert.That(profile.GetProperty("fullName").GetString(), Is.EqualTo("Олена Петрівна"));
            Assert.That(profile.GetProperty("role").GetString(), Is.EqualTo("Manager"));
            Assert.That(profile.GetProperty("avatarUrl").GetString(), Is.EqualTo(avatarUrl));
            Assert.That(me.GetProperty("avatarUrl").GetString(), Is.EqualTo(avatarUrl));
            Assert.That(email.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(password.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(remove.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(_host.Avatars.Files.Keys.Any(key => avatarUrl!.EndsWith(key)), Is.False, "old file deleted");
        });
    }

    [Test]
    public async Task Small_or_non_image_photo_is_rejected()
    {
        using var admin = await CookieAuthApiTests.SignInAsync(_host, IdentityApiHost.AdminEmail, IdentityApiHost.AdminPassword);

        var tiny = await admin.Http.PutAsync("/api/profile/avatar", Photo(64, 64));
        var text = await admin.Http.PutAsync("/api/profile/avatar", File("not a photo"u8.ToArray(), "photo.png"));

        Assert.Multiple(() =>
        {
            Assert.That(tiny.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(text.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task Admin_changes_own_password_and_email_only_with_the_current_password()
    {
        using var admin = await CookieAuthApiTests.SignInAsync(_host, IdentityApiHost.AdminEmail, IdentityApiHost.AdminPassword);
        await admin.Http.PostAsJsonAsync("/api/admin/users",
            new { email = "profile-admin@brevi.test", fullName = "Адмін 2", role = "Admin", password = "Admin-Pass-2a" });
        using var second = await CookieAuthApiTests.SignInAsync(_host, "profile-admin@brevi.test", "Admin-Pass-2a");

        var wrong = await second.Http.PutAsJsonAsync("/api/profile/password", new { currentPassword = "nope-nope", newPassword = "Admin-Pass-2b" });
        var changed = await second.Http.PutAsJsonAsync("/api/profile/password", new { currentPassword = "Admin-Pass-2a", newPassword = "Admin-Pass-2b" });
        var stillSignedIn = await second.Http.GetAsync("/api/profile");
        var taken = await second.Http.PutAsJsonAsync("/api/profile/email", new { email = IdentityApiHost.AdminEmail, currentPassword = "Admin-Pass-2b" });
        var email = await second.Http.PutAsJsonAsync("/api/profile/email", new { email = "profile-admin-new@brevi.test", currentPassword = "Admin-Pass-2b" });

        using var browser = _host.CreateBrowser();
        var newLogin = await browser.Http.PostAsJsonAsync("/api/auth/cookie/login",
            new { email = "profile-admin-new@brevi.test", password = "Admin-Pass-2b" });

        Assert.Multiple(() =>
        {
            Assert.That(wrong.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(changed.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(stillSignedIn.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(taken.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(email.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(newLogin.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    private static MultipartFormDataContent Photo(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap)) canvas.Clear(SKColors.Orange);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return File(data.ToArray(), "photo.png");
    }

    private static MultipartFormDataContent File(byte[] bytes, string name)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return new MultipartFormDataContent { { file, "file", name } };
    }
}
