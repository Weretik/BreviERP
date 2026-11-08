namespace Crm.Domain.Leads;

/// <summary>
/// A company the AI found and wrote into the sheet «Лиды Brevi» (LEAD-001). The sheet owns what is known about the
/// company; the admin owns the calling work: who took it, the stage, the calls and the next contact.
/// <see cref="Version"/> grows with every change of the work and guards against two managers at once.
/// </summary>
public sealed class Lead
{
    private readonly List<LeadCall> _calls = [];

    private Lead() { }

    public Lead(LeadSheetRow row, DateTimeOffset now)
    {
        SourceId = row.SourceId;
        SheetNotes = LeadLimits.Cut(row.SheetNotes, LeadLimits.LongText);
        Stage = LeadStage.New;
        CreatedAt = now;
        Version = 1;
        ApplySheet(row, now);
    }

    public long Id { get; private set; }

    /// <summary>The «ID» column of the sheet: the key the sync matches rows by.</summary>
    public int SourceId { get; private set; }

    public DateOnly? AddedOn { get; private set; }
    public string? Category { get; private set; }
    public string Company { get; private set; } = null!;
    public string? Region { get; private set; }
    public string? Industry { get; private set; }
    public string? Scale { get; private set; }
    public string? Signal { get; private set; }
    public DateOnly? SignalDate { get; private set; }
    public string? FactOrPlan { get; private set; }
    public string? SignalUrl { get; private set; }
    public string? VolumeReason { get; private set; }
    public string? Offer { get; private set; }
    public string? Website { get; private set; }
    public string? Contacts { get; private set; }
    public string? WhomToCall { get; private set; }
    public string? TenderCheck { get; private set; }
    public string? CallReason { get; private set; }
    public string? Reliability { get; private set; }
    public string? Email { get; private set; }

    /// <summary>The manager columns of the sheet at the first sync («Кто взял», «Результат звонка», …).</summary>
    public string? SheetNotes { get; private set; }

    /// <summary>False when the row disappeared from the sheet; the lead and its calls stay.</summary>
    public bool InSheet { get; private set; }

    public int? OwnerUserId { get; private set; }
    public string? OwnerName { get; private set; }
    public DateTimeOffset? TakenAt { get; private set; }
    public LeadStage Stage { get; private set; }
    public string? LastResultName { get; private set; }
    public DateTimeOffset? LastCallAt { get; private set; }
    public DateTimeOffset? NextContactAt { get; private set; }
    public int CallCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset SyncedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public IReadOnlyList<LeadCall> Calls => _calls;

    public bool IsOpen => Stage is not (LeadStage.Client or LeadStage.Closed);

    /// <summary>Overwrites the sheet fields; returns true when something changed.</summary>
    public bool ApplySheet(LeadSheetRow row, DateTimeOffset now)
    {
        var changed = !InSheet
                      || AddedOn != row.AddedOn
                      || Category != Text(row.Category, LeadLimits.CategoryMaxLength)
                      || Company != Text(row.Company, LeadLimits.ShortText)
                      || Region != Text(row.Region, LeadLimits.ShortText)
                      || Industry != Text(row.Industry, LeadLimits.ShortText)
                      || Scale != Text(row.Scale, LeadLimits.LongText)
                      || Signal != Text(row.Signal, LeadLimits.LongText)
                      || SignalDate != row.SignalDate
                      || FactOrPlan != Text(row.FactOrPlan, LeadLimits.ShortText)
                      || SignalUrl != Text(row.SignalUrl, LeadLimits.LongText)
                      || VolumeReason != Text(row.VolumeReason, LeadLimits.LongText)
                      || Offer != Text(row.Offer, LeadLimits.LongText)
                      || Website != Text(row.Website, LeadLimits.LongText)
                      || Contacts != Text(row.Contacts, LeadLimits.LongText)
                      || WhomToCall != Text(row.WhomToCall, LeadLimits.ShortText)
                      || TenderCheck != Text(row.TenderCheck, LeadLimits.LongText)
                      || CallReason != Text(row.CallReason, LeadLimits.LongText)
                      || Reliability != Text(row.Reliability, LeadLimits.ShortText)
                      || Email != Text(row.Email, LeadLimits.ShortText);

        AddedOn = row.AddedOn;
        Category = Text(row.Category, LeadLimits.CategoryMaxLength);
        Company = Text(row.Company, LeadLimits.ShortText) ?? $"Лід {row.SourceId}";
        Region = Text(row.Region, LeadLimits.ShortText);
        Industry = Text(row.Industry, LeadLimits.ShortText);
        Scale = Text(row.Scale, LeadLimits.LongText);
        Signal = Text(row.Signal, LeadLimits.LongText);
        SignalDate = row.SignalDate;
        FactOrPlan = Text(row.FactOrPlan, LeadLimits.ShortText);
        SignalUrl = Text(row.SignalUrl, LeadLimits.LongText);
        VolumeReason = Text(row.VolumeReason, LeadLimits.LongText);
        Offer = Text(row.Offer, LeadLimits.LongText);
        Website = Text(row.Website, LeadLimits.LongText);
        Contacts = Text(row.Contacts, LeadLimits.LongText);
        WhomToCall = Text(row.WhomToCall, LeadLimits.ShortText);
        TenderCheck = Text(row.TenderCheck, LeadLimits.LongText);
        CallReason = Text(row.CallReason, LeadLimits.LongText);
        Reliability = Text(row.Reliability, LeadLimits.ShortText);
        Email = Text(row.Email, LeadLimits.ShortText);
        InSheet = true;
        SyncedAt = now;
        return changed;
    }

    public void MarkGoneFromSheet(DateTimeOffset now)
    {
        InSheet = false;
        SyncedAt = now;
    }

    public bool IsOwnedBy(int userId) => OwnerUserId == userId;

    /// <summary>Takes a free lead (R-04). False when somebody else already has it.</summary>
    public bool Take(LeadPerson by, DateTimeOffset now)
    {
        if (OwnerUserId == by.UserId) return true;
        if (OwnerUserId is not null) return false;
        OwnerUserId = by.UserId;
        OwnerName = by.Name;
        TakenAt = now;
        if (Stage == LeadStage.New) Stage = LeadStage.InWork;
        Touch(now);
        return true;
    }

    /// <summary>Gives the lead back to «Вільні»; calls stay, the next contact is cleared (R-04).</summary>
    public void Release(DateTimeOffset now)
    {
        OwnerUserId = null;
        OwnerName = null;
        TakenAt = null;
        NextContactAt = null;
        if (IsOpen) Stage = CallCount == 0 ? LeadStage.New : LeadStage.InWork;
        Touch(now);
    }

    /// <summary>
    /// Records a call by the owner (R-06, R-07). <paramref name="nextContactAt"/> must already be checked against the
    /// result: required for callback, talking and offer, cleared when the result closes the lead.
    /// </summary>
    public LeadCall RecordCall(LeadPerson by, LeadCallResult result, string? comment, DateTimeOffset? nextContactAt,
        DateTimeOffset now)
    {
        var next = result.Outcome.Closes() ? null : nextContactAt;
        var call = new LeadCall(by, result, comment, next, now);
        _calls.Add(call);
        Stage = result.Outcome.Stage();
        LastResultName = result.Name;
        LastCallAt = now;
        NextContactAt = next;
        CallCount++;
        Touch(now);
        return call;
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version++;
    }

    private static string? Text(string? value, int max) => LeadLimits.Cut(value, max);
}
