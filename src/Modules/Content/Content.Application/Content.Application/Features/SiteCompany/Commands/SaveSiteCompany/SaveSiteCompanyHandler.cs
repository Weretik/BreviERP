using Content.Application.Abstractions;
using Content.Domain.SiteCompany;
using Content.Application.Features.SiteCompany.Shared;
using BuildingBlocks.Application;

namespace Content.Application.Features.SiteCompany.Commands.SaveSiteCompany;

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
