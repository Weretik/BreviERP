using Content.Domain.SiteCompany;

namespace Content.Application.Features.SiteCompany.Shared;

internal static class SiteCompanyReads
{
    private static readonly CompanyText Empty = new(string.Empty, string.Empty);

    public static CompanyText Text(SiteText text) => new(text.Uk, text.Ru);

    public static SiteText Text(CompanyText text) => new(text.Uk, text.Ru);

    public static CompanyBankDetails Bank(SiteBankDetails bank) =>
        new(Text(bank.Recipient), bank.Edrpou, bank.Iban, Text(bank.Bank));

    public static CompanySchedule Schedule(SiteSchedule schedule) => new(schedule.Days, schedule.Opens, schedule.Closes);

    public static CompanySocials Socials(SiteSocials socials) => new(socials.Instagram, socials.Youtube, socials.Facebook);

    public static SiteCompanyResponse Response(SiteCompanyInfo? info) =>
        info is null
            ? new SiteCompanyResponse(string.Empty, string.Empty, Empty, Empty, string.Empty,
                new CompanySchedule([], string.Empty, string.Empty), [],
                new CompanyBankDetails(Empty, string.Empty, string.Empty, Empty), new CompanySocials(null, null, null),
                0, null, null)
            : new SiteCompanyResponse(info.Phone, info.Email, Text(info.City), Text(info.Street), info.MapQuery,
                Schedule(info.Schedule),
                info.Managers.Select(m => new CompanyManager(Text(m.Name), m.PhotoUrl, m.Phones, m.Email)).ToArray(),
                Bank(info.BankDetails), Socials(info.Socials), info.Revision, info.UpdatedAtUtc, info.UpdatedBy);

    public static PublicPhone Phone(string formatted) => new(SiteCompanyInfo.PhoneE164(formatted), formatted);

    public static PublicSiteCompanyResponse Public(SiteCompanyInfo info) =>
        new(Phone(info.Phone), info.Email, Text(info.City), Text(info.Street), info.MapQuery, Schedule(info.Schedule),
            info.Managers.Select(m => new PublicCompanyManager(Text(m.Name), m.PhotoUrl,
                m.Phones.Select(Phone).ToArray(), m.Email)).ToArray(),
            Bank(info.BankDetails), Socials(info.Socials));
}
