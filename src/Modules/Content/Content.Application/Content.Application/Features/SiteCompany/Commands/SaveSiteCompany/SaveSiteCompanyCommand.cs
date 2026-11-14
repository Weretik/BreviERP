using Content.Application.Abstractions;
using Content.Domain.SiteCompany;
using Content.Application.Features.SiteCompany.Shared;

namespace Content.Application.Features.SiteCompany.Commands.SaveSiteCompany;

/// <summary>Replaces the company facts as a whole with a revision check (CNT-009 SC-02).</summary>
public sealed record SaveSiteCompanyCommand(
    int Revision,
    string Phone,
    string Email,
    CompanyText City,
    CompanyText Street,
    string MapQuery,
    CompanySchedule Schedule,
    IReadOnlyList<CompanyManager> Managers,
    CompanyBankDetails BankDetails,
    CompanySocials Socials,
    string UpdatedBy) : ICommand<Result<SiteCompanyResponse>>;

public sealed class SaveSiteCompanyValidator : AbstractValidator<SaveSiteCompanyCommand>
{
    public SaveSiteCompanyValidator()
    {
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Phone).Must(p => SiteCompanyInfo.FormatPhone(p) is not null).WithMessage(SiteCompanyMessages.Phone);
        RuleFor(x => x.Email).Must(SiteCompanyInfo.IsEmail).WithMessage(SiteCompanyMessages.Email);
        RuleFor(x => x.City).NotNull().Must(Filled).WithMessage(SiteCompanyMessages.Required)
            .Must(Short).WithMessage(SiteCompanyMessages.TooLong);
        RuleFor(x => x.Street).NotNull().Must(Filled).WithMessage(SiteCompanyMessages.Required)
            .Must(Short).WithMessage(SiteCompanyMessages.TooLong);
        RuleFor(x => x.MapQuery).NotEmpty().MaximumLength(SiteCompanyInfo.TextMaxLength);
        RuleFor(x => x.Schedule).NotNull();
        RuleFor(x => x.Schedule.Days).NotEmpty().WithMessage(SiteCompanyMessages.Days)
            .Must(days => days.All(d => d is >= 1 and <= 7)).WithMessage(SiteCompanyMessages.Days)
            .When(x => x.Schedule is not null);
        RuleFor(x => x.Schedule).Must(s => SiteCompanyInfo.IsTime(s.Opens) && SiteCompanyInfo.IsTime(s.Closes)
                && string.CompareOrdinal(s.Opens.Trim(), s.Closes.Trim()) < 0)
            .WithMessage(SiteCompanyMessages.Time).When(x => x.Schedule is not null);
        RuleFor(x => x.Managers).NotNull().Must(m => m.Count <= SiteCompanyInfo.MaxManagers)
            .WithMessage(SiteCompanyMessages.Managers);
        RuleForEach(x => x.Managers).ChildRules(manager =>
        {
            manager.RuleFor(m => m.Name).NotNull().Must(Filled).WithMessage(SiteCompanyMessages.Required)
                .Must(Short).WithMessage(SiteCompanyMessages.TooLong);
            manager.RuleFor(m => m.Phones).NotNull()
                .Must(p => p.Count is >= 1 and <= SiteCompanyInfo.MaxManagerPhones).WithMessage(SiteCompanyMessages.ManagerPhones);
            manager.RuleForEach(m => m.Phones).Must(p => SiteCompanyInfo.FormatPhone(p) is not null)
                .WithMessage(SiteCompanyMessages.Phone);
            manager.RuleFor(m => m.Email).Must(SiteCompanyInfo.IsEmail).WithMessage(SiteCompanyMessages.Email)
                .When(m => SiteCompanyInfo.Clean(m.Email) is not null);
            manager.RuleFor(m => m.PhotoUrl).Must(SiteCompanyInfo.IsPhotoUrl).WithMessage(SiteCompanyMessages.Photo);
        });
        RuleFor(x => x.BankDetails).NotNull();
        RuleFor(x => x.BankDetails.Recipient).Must(Filled).WithMessage(SiteCompanyMessages.Required)
            .Must(Short).WithMessage(SiteCompanyMessages.TooLong).When(x => x.BankDetails is not null);
        RuleFor(x => x.BankDetails.Bank).Must(Filled).WithMessage(SiteCompanyMessages.Required)
            .Must(Short).WithMessage(SiteCompanyMessages.TooLong).When(x => x.BankDetails is not null);
        RuleFor(x => x.BankDetails.Edrpou).Must(SiteCompanyInfo.IsEdrpou).WithMessage(SiteCompanyMessages.Edrpou)
            .When(x => x.BankDetails is not null);
        RuleFor(x => x.BankDetails.Iban).Must(i => SiteCompanyInfo.FormatIban(i) is not null)
            .WithMessage(SiteCompanyMessages.Iban).When(x => x.BankDetails is not null);
        RuleFor(x => x.Socials).NotNull();
        RuleFor(x => x.Socials.Instagram).Must(SiteCompanyInfo.IsHttpsUrl).WithMessage(SiteCompanyMessages.Link)
            .When(x => x.Socials is not null);
        RuleFor(x => x.Socials.Youtube).Must(SiteCompanyInfo.IsHttpsUrl).WithMessage(SiteCompanyMessages.Link)
            .When(x => x.Socials is not null);
        RuleFor(x => x.Socials.Facebook).Must(SiteCompanyInfo.IsHttpsUrl).WithMessage(SiteCompanyMessages.Link)
            .When(x => x.Socials is not null);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }

    private static bool Filled(CompanyText? text) =>
        text is not null && SiteCompanyInfo.Clean(text.Uk) is not null && SiteCompanyInfo.Clean(text.Ru) is not null;

    private static bool Short(CompanyText? text) =>
        text is null || (text.Uk.Length <= SiteCompanyInfo.TextMaxLength && text.Ru.Length <= SiteCompanyInfo.TextMaxLength);
}

public sealed class SaveSiteCompanyHandler(IContentDbContext db, TimeProvider clock)
    : ICommandHandler<SaveSiteCompanyCommand, Result<SiteCompanyResponse>>
{
    public async ValueTask<Result<SiteCompanyResponse>> Handle(SaveSiteCompanyCommand command,
        CancellationToken cancellationToken)
    {
        var info = await db.CompanyInfo.FirstOrDefaultAsync(cancellationToken);
        if ((info?.Revision ?? 0) != command.Revision) return Result.Conflict(SiteCompanyMessages.Stale);

        var now = clock.GetUtcNow();
        if (info is null)
        {
            info = new SiteCompanyInfo(command.UpdatedBy, now);
            db.CompanyInfo.Add(info);
        }

        var replaced = info.Replace(command.Phone, command.Email, SiteCompanyReads.Text(command.City),
            SiteCompanyReads.Text(command.Street), command.MapQuery,
            new SiteSchedule(command.Schedule.Days, command.Schedule.Opens, command.Schedule.Closes),
            command.Managers.Select(m => new SiteManager(SiteCompanyReads.Text(m.Name), m.PhotoUrl, m.Phones, m.Email))
                .ToArray(),
            new SiteBankDetails(SiteCompanyReads.Text(command.BankDetails.Recipient), command.BankDetails.Edrpou,
                command.BankDetails.Iban, SiteCompanyReads.Text(command.BankDetails.Bank)),
            new SiteSocials(command.Socials.Instagram, command.Socials.Youtube, command.Socials.Facebook),
            command.UpdatedBy, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<SiteCompanyResponse>();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two first saves at once, or a revision that moved under us.
            return Result.Conflict(SiteCompanyMessages.Stale);
        }

        return SiteCompanyReads.Response(info);
    }
}
