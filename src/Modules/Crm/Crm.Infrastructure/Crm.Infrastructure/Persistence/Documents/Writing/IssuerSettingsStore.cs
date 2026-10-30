using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Contracts;
using Crm.Application.Documents.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.Documents;

public sealed class IssuerSettingsStore(CrmDbContext db) : IDocumentSettingsStore
{
    private const long SettingsLock = 484933447251L;

    public Task<ActiveIssuerSettings> GetActiveAsync(CancellationToken ct)
        => new DocumentSettingsReadStore(db).GetActiveAsync(ct);

    public async Task<Result<IssuerSettingsDetail>> CreateVersionAsync(Guid requestId, IssuerSettingsInput issuerInput,
        DocumentTemplateInput templateInput, CrmActor actor, CancellationToken ct)
    {
        if (!actor.IsAdmin || actor.UserId <= 0) return Result.Forbidden();
        var hash = DocumentSettingsMutationJournal.Hash(new { issuerInput, templateInput });
        var journal = new DocumentSettingsMutationJournal(db);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await LockAsync(ct);
            var replay = await journal.ReplayAsync<IssuerSettingsDetail>("CreateIssuerSettingsVersion", requestId, actor.UserId, hash, ct);
            if (replay is not null) return replay;
            var number = (await db.Set<IssuerSettingsVersion>().Select(x => (int?)x.Number).MaxAsync(ct) ?? 0) + 1;
            var actorId = new CrmUserId(actor.UserId);
            var issuer = IssuerSettingsVersion.Create(number, issuerInput.LegalName, issuerInput.Edrpou,
                issuerInput.TaxId, issuerInput.Iban, issuerInput.Address, issuerInput.Phone, issuerInput.Email,
                issuerInput.Site, issuerInput.BankName, issuerInput.Mfo, issuerInput.LogoAssetKey, actorId);
            var template = DocumentTemplateVersion.Create(number, templateInput.LayoutKey, templateInput.Title,
                templateInput.VatNotice, templateInput.OpeningText, templateInput.CtaTitle, templateInput.CtaText,
                templateInput.CtaContactTemplate, actorId);
            db.Add(issuer);
            db.Add(template);
            await db.SaveChangesAsync(ct);
            var revision = await db.Set<DocumentSettingsState>().AsNoTracking()
                .Where(x => x.Id == new DocumentSettingsStateId(1)).Select(x => (long?)x.Revision).SingleOrDefaultAsync(ct) ?? 1;
            var detail = DocumentSettingsMapper.Detail(revision, false, issuer, template);
            journal.Append("CreateIssuerSettingsVersion", requestId, actor.UserId, hash, detail);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Result.Success(detail);
        }
        catch (DocumentSettingsValidationException ex)
        {
            return Result.Invalid(new ValidationError("settings", ex.Message));
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Result.Conflict("settings_unique_conflict");
        }
        finally { db.ChangeTracker.Clear(); }
    }

    public async Task<Result<ActiveIssuerSettings>> ActivateAsync(Guid requestId, long expectedRevision,
        Guid issuerVersionId, Guid templateVersionId, CrmActor actor, CancellationToken ct)
    {
        if (!actor.IsAdmin || actor.UserId <= 0) return Result.Forbidden();
        var hash = DocumentSettingsMutationJournal.Hash(new { expectedRevision, issuerVersionId, templateVersionId });
        var journal = new DocumentSettingsMutationJournal(db);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await LockAsync(ct);
            var replay = await journal.ReplayAsync<ActiveIssuerSettings>("ActivateIssuerSettings", requestId, actor.UserId, hash, ct);
            if (replay is not null) return replay;
            var issuer = await db.Set<IssuerSettingsVersion>().SingleOrDefaultAsync(x => x.PublicId == issuerVersionId, ct);
            var template = await db.Set<DocumentTemplateVersion>().SingleOrDefaultAsync(x => x.PublicId == templateVersionId, ct);
            if (issuer is null || template is null) return Result.NotFound();
            if (issuer.Number != template.Number) return Result.Invalid(new ValidationError("settings", "version_pair_mismatch"));
            var state = await db.Set<DocumentSettingsState>().SingleOrDefaultAsync(x => x.Id == new DocumentSettingsStateId(1), ct);
            if (state is null)
            {
                state = DocumentSettingsState.Create();
                db.Add(state);
            }
            if (state.Revision != expectedRevision) return Result.Conflict("revision_conflict");
            state.Activate(expectedRevision, issuer.Id, template.Id);
            var active = new ActiveIssuerSettings(state.Revision, DocumentSettingsMapper.Detail(state.Revision, true, issuer, template));
            journal.Append("ActivateIssuerSettings", requestId, actor.UserId, hash, active);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Result.Success(active);
        }
        catch (DocumentSettingsValidationException ex)
        {
            return ex.Message == "revision_conflict"
                ? Result.Conflict("revision_conflict")
                : Result.Invalid(new ValidationError("settings", ex.Message));
        }
        catch (DbUpdateConcurrencyException) { return Result.Conflict("revision_conflict"); }
        finally { db.ChangeTracker.Clear(); }
    }

    private Task LockAsync(CancellationToken ct)
        => db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({SettingsLock})", ct);

}
