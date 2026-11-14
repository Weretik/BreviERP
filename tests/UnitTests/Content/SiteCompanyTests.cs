using Ardalis.Result;
using FluentValidation.TestHelper;
using Content.Application.Features.SiteCompany.Commands.SaveSiteCompany;
using Content.Application.Features.SiteCompany.Queries.GetSiteCompany;
using Content.Application.Features.SiteCompany.Shared;
using Content.Application.Features.SiteCompany.Shared.Company;
using Content.Application.Features.SiteCompany.Shared.Public;

namespace UnitTests.Content;

/// <summary>«Контент → Контакти та компанія» and the public company facts of the site (CNT-009).</summary>
public sealed class SiteCompanyTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static SaveSiteCompanyCommand Save(int revision, string phone = "+38 (050) 469-86-75",
        string iban = "ua21305299000002600700000000 1", IReadOnlyList<CompanyManager>? managers = null,
        CompanySchedule? schedule = null, string? instagram = "https://instagram.com/brevi") =>
        new(revision, phone, " pp.brevi@gmail.com ", new CompanyText("Полтава", "Полтава"),
            new CompanyText("вул.  Кагамлика, 3А", "ул. Кагамлыка, 3А"), "Полтава, вулиця Кагамлика, 3А",
            schedule ?? new CompanySchedule([5, 1, 2, 3, 4, 1], "09:00", "18:00"),
            managers ?? [new CompanyManager(new CompanyText("Анастасія", "Анастасия"), "https://cdn.test/photos/1.jpg",
                ["0503090660", "380674467546"], "a@brevi.ua")],
            new CompanyBankDetails(new CompanyText("ПП «Бреві»", "ЧП «Бреви»"), "35521636", iban,
                new CompanyText("ПриватБанк", "ПриватБанк")),
            new CompanySocials(instagram, null, " "), ContentTestHarness.Editor);

    [Test]
    public async Task Facts_are_saved_with_a_revision_and_shown_to_the_site_with_tel_numbers()
    {
        using var h = new ContentTestHarness();

        var empty = (await h.Company().Handle(new GetSiteCompanyQuery(), None)).Value;
        var noSite = await h.PublicCompany().GetAsync(None);
        var saved = await h.SaveCompany().Handle(Save(0), None);
        var stale = await h.SaveCompany().Handle(Save(0), None);
        var read = (await h.Company().Handle(new GetSiteCompanyQuery(), None)).Value;
        var site = await h.PublicCompany().GetAsync(None);

        Assert.Multiple(() =>
        {
            Assert.That(empty.Revision, Is.Zero);
            Assert.That(noSite, Is.Null);
            Assert.That(saved.Value.Revision, Is.EqualTo(1));
            Assert.That(stale.Errors, Does.Contain(SiteCompanyMessages.Stale));
            Assert.That(read.Phone, Is.EqualTo("050 469 86 75"));
            Assert.That(read.Email, Is.EqualTo("pp.brevi@gmail.com"));
            Assert.That(read.Street.Uk, Is.EqualTo("вул. Кагамлика, 3А"));
            Assert.That(read.Schedule.Days, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
            Assert.That(read.Managers.Single().Phones, Is.EqualTo(new[] { "050 309 06 60", "067 446 75 46" }));
            Assert.That(read.BankDetails.Iban, Is.EqualTo("UA21 305299 00000 26007000000001"));
            Assert.That(read.Socials, Is.EqualTo(new CompanySocials("https://instagram.com/brevi", null, null)));
            Assert.That(site!.Phone, Is.EqualTo(new PublicPhone("+380504698675", "050 469 86 75")));
            Assert.That(site.Managers.Single().Phones[1].E164, Is.EqualTo("+380674467546"));
        });
    }

    [Test]
    public void Validator_names_each_wrong_field()
    {
        var validator = new SaveSiteCompanyValidator();

        Assert.Multiple(() =>
        {
            validator.TestValidate(Save(0, phone: "12345")).ShouldHaveValidationErrorFor(x => x.Phone)
                .WithErrorMessage(SiteCompanyMessages.Phone);
            validator.TestValidate(Save(0, iban: "UA12")).ShouldHaveValidationErrorFor(x => x.BankDetails.Iban)
                .WithErrorMessage(SiteCompanyMessages.Iban);
            validator.TestValidate(Save(0, instagram: "instagram.com/brevi"))
                .ShouldHaveValidationErrorFor(x => x.Socials.Instagram).WithErrorMessage(SiteCompanyMessages.Link);
            validator.TestValidate(Save(0, schedule: new CompanySchedule([1], "18:00", "09:00")))
                .ShouldHaveValidationErrorFor(x => x.Schedule).WithErrorMessage(SiteCompanyMessages.Time);
            Assert.That(validator.TestValidate(Save(0, managers: [new CompanyManager(new CompanyText("А", ""), "http://x", [], null)]))
                .Errors.Select(e => e.ErrorMessage), Is.SupersetOf(new[]
                {
                    SiteCompanyMessages.Required, SiteCompanyMessages.ManagerPhones, SiteCompanyMessages.Photo,
                }));
            validator.TestValidate(Save(0)).ShouldNotHaveAnyValidationErrors();
        });
    }
}
