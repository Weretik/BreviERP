using Ardalis.Result;
using Content.Application.SiteSettings.Get;
using Content.Application.SiteSettings.Save;
using Content.Application.SiteSettings.Shared;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Caching.Memory;

namespace UnitTests.Content;

/// <summary>«Контент → Налаштування SEO сайту» and the public settings of the site (CNT-006).</summary>
public sealed class SiteSeoSettingsTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static SaveSiteSeoSettingsCommand Save(int revision, string? ga = " g-abc1234 ", string? gtm = null,
        string? google = "abcDEF123_-xyz", string? bing = null, int? photo = null) =>
        new(revision, ga, gtm, google, bing, photo, ContentTestHarness.Editor);

    [Test]
    public async Task Settings_are_empty_until_saved_then_kept_with_a_revision()
    {
        using var h = new ContentTestHarness();
        h.AddPhoto(7);

        var empty = (await h.SeoSettings().Handle(new GetSiteSeoSettingsQuery(), None)).Value;
        var saved = await h.SaveSeoSettings().Handle(Save(0, photo: 7), None);
        var again = await h.SaveSeoSettings().Handle(Save(1, ga: null, gtm: "gtm-ab12cd", photo: null), None);
        var stale = await h.SaveSeoSettings().Handle(Save(1), None);
        var read = (await h.SeoSettings().Handle(new GetSiteSeoSettingsQuery(), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That((empty.Revision, empty.GoogleAnalyticsId, empty.UpdatedBy), Is.EqualTo((0, (string?)null, (string?)null)));
            Assert.That(saved.Value.GoogleAnalyticsId, Is.EqualTo("G-ABC1234"));
            Assert.That(saved.Value.DefaultOgImage?.Url, Is.EqualTo("https://cdn.test/photos/7.jpg"));
            Assert.That(again.Value.Revision, Is.EqualTo(2));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(stale.Errors, Does.Contain(SiteSettingsMessages.Stale));
            Assert.That((read.GoogleAnalyticsId, read.GoogleTagManagerId, read.DefaultOgImage),
                Is.EqualTo(((string?)null, "GTM-AB12CD", (object?)null)));
        });
    }

    [Test]
    public async Task A_picture_missing_from_the_library_is_refused()
    {
        using var h = new ContentTestHarness();

        var result = await h.SaveSeoSettings().Handle(Save(0, photo: 404), None);

        Assert.That(result.Errors, Does.Contain(SiteSettingsMessages.PhotoNotFound));
    }

    [Test]
    public void Validator_names_the_wrong_code()
    {
        var validator = new SaveSiteSeoSettingsCommandValidator();

        validator.TestValidate(Save(0, ga: "UA-12345-1")).ShouldHaveValidationErrorFor(x => x.GoogleAnalyticsId)
            .WithErrorMessage(SiteSettingsMessages.AnalyticsId);
        validator.TestValidate(Save(0, gtm: "G-ABC1234")).ShouldHaveValidationErrorFor(x => x.GoogleTagManagerId);
        validator.TestValidate(Save(0, google: "<meta name=\"google-site-verification\" content=\"x\">"))
            .ShouldHaveValidationErrorFor(x => x.GoogleSiteVerification);
        validator.TestValidate(Save(0, ga: "", google: " ", bing: "0123456789ABCDEF0123456789ABCDEF"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async Task Public_settings_carry_codes_and_picture_and_are_kept_for_a_minute()
    {
        using var h = new ContentTestHarness();
        h.AddPhoto(7);
        await h.SaveSeoSettings().Handle(Save(0, bing: "0123456789ABCDEF", photo: 7), None);
        var cache = new MemoryCache(new MemoryCacheOptions());

        var first = await h.PublicSiteSettings(cache).GetAsync(None);
        await h.SaveSeoSettings().Handle(Save(1, ga: null), None);
        var cached = await h.PublicSiteSettings(cache).GetAsync(None);
        var fresh = await h.PublicSiteSettings().GetAsync(None);

        Assert.Multiple(() =>
        {
            Assert.That(first.GoogleAnalyticsId, Is.EqualTo("G-ABC1234"));
            Assert.That(first.BingSiteVerification, Is.EqualTo("0123456789ABCDEF"));
            Assert.That(first.DefaultOgImage?.Width, Is.EqualTo(1200));
            Assert.That(cached.GoogleAnalyticsId, Is.EqualTo("G-ABC1234"));
            Assert.That(fresh.GoogleAnalyticsId, Is.Null);
            Assert.That(fresh.DefaultOgImage, Is.Null);
        });
    }
}
