using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.Identity;

[NonParallelizable]
public sealed class CookieAuthApiTests
{
    private IdentityApiHost _host = null!;

    [OneTimeSetUp]
    public async Task StartHost() => _host = await IdentityApiHost.CreateAsync();

    [OneTimeTearDown]
    public async Task StopHost() => await _host.DisposeAsync();

    [Test]
    public async Task Login_sets_session_and_csrf_cookies_and_me_returns_the_admin()
    {
        using var browser = _host.CreateBrowser();

        var login = await browser.Http.PostAsJsonAsync("/api/auth/cookie/login",
            new { email = IdentityApiHost.AdminEmail, password = IdentityApiHost.AdminPassword });
        var me = await browser.Http.GetFromJsonAsync<JsonElement>("/api/auth/session/me");

        Assert.Multiple(() =>
        {
            Assert.That(login.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(browser.Cookie("__Host-brevi.session"), Is.Not.Null.And.Not.Empty);
            Assert.That(browser.Cookie("brevi.csrf"), Is.Not.Null.And.Not.Empty);
            Assert.That(me.GetProperty("email").GetString(), Is.EqualTo(IdentityApiHost.AdminEmail));
            Assert.That(me.GetProperty("fullName").GetString(), Is.EqualTo("Test Admin"));
            Assert.That(me.GetProperty("roles").EnumerateArray().Select(x => x.GetString()), Does.Contain("Admin"));
        });
    }

    [Test]
    public async Task Wrong_password_returns_401_without_a_session()
    {
        using var browser = _host.CreateBrowser();

        var login = await browser.Http.PostAsJsonAsync("/api/auth/cookie/login",
            new { email = IdentityApiHost.AdminEmail, password = "wrong-password" });

        Assert.Multiple(() =>
        {
            Assert.That(login.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(browser.Cookie("__Host-brevi.session"), Is.Null);
        });
    }

    [Test]
    public async Task Without_a_session_me_and_admin_api_return_401()
    {
        using var browser = _host.CreateBrowser();

        Assert.Multiple(async () =>
        {
            Assert.That((await browser.Http.GetAsync("/api/auth/session/me")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That((await browser.Http.GetAsync("/api/admin/users")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    public async Task Changing_request_with_session_needs_the_csrf_header()
    {
        using var browser = await SignedInAdminAsync();
        browser.Http.DefaultRequestHeaders.Remove("X-CSRF-Token");

        var withoutHeader = await browser.Http.PostAsync("/api/auth/cookie/logout", null);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/cookie/logout");
        request.Headers.Add("X-CSRF-Token", browser.Cookie("brevi.csrf"));
        var withHeader = await browser.Http.SendAsync(request);
        var meAfterLogout = await browser.Http.GetAsync("/api/auth/session/me");

        Assert.Multiple(() =>
        {
            Assert.That(withoutHeader.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(withHeader.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(meAfterLogout.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    public async Task Old_token_endpoints_are_gone()
    {
        using var browser = _host.CreateBrowser();

        foreach (var path in new[] { "/api/auth/session/login", "/api/auth/session/refresh", "/api/auth/session/logout" })
        {
            var response = await browser.Http.PostAsJsonAsync(path, new { email = IdentityApiHost.AdminEmail, password = IdentityApiHost.AdminPassword });
            // No endpoint: the fallback "signed in" policy answers 401 before routing can say 404.
            Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.Unauthorized), path);
            Assert.That(browser.Cookie("__Host-brevi.session"), Is.Null, path);
        }
    }

    internal static async Task<BrowserClient> SignInAsync(IdentityApiHost host, string email, string password)
    {
        var browser = host.CreateBrowser();
        var login = await browser.Http.PostAsJsonAsync("/api/auth/cookie/login", new { email, password });
        Assert.That(login.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"login {email}");
        browser.Http.DefaultRequestHeaders.Add("X-CSRF-Token", browser.Cookie("brevi.csrf"));
        return browser;
    }

    private Task<BrowserClient> SignedInAdminAsync() =>
        SignInAsync(_host, IdentityApiHost.AdminEmail, IdentityApiHost.AdminPassword);
}
