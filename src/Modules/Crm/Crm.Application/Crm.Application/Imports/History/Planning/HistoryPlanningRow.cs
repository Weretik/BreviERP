using System.Security.Cryptography;
using System.Text;
using Crm.Application.Imports.History.Contracts;
namespace Crm.Application.Imports.History.Planning;

internal sealed class HistoryPlanningRow(CrmHistoryRow source)
{
    internal CrmHistoryRow Source { get; } = source;
    internal (string Entity, string Id) Key => (Source.Entity, Source.SourceId);
    internal HistoryDecisionStatus Status { get; set; } = HistoryDecisionStatus.WouldImport;
    internal HashSet<string> Reasons { get; } = new(StringComparer.Ordinal);
    internal bool IsUnresolved => Status is HistoryDecisionStatus.Invalid or HistoryDecisionStatus.NeedsReview or HistoryDecisionStatus.Excluded;

    internal void Review(string reason)
    {
        if (Status is HistoryDecisionStatus.Invalid or HistoryDecisionStatus.Excluded) return;
        Status = HistoryDecisionStatus.NeedsReview;
        Reasons.Add(reason);
    }

    internal CrmHistoryRowDecision Decision() => new(Source.Entity,
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Source.Entity + "\0" + Source.SourceId))),
        Source.PayloadSha256, Status switch
        {
            HistoryDecisionStatus.WouldImport => "wouldImport",
            HistoryDecisionStatus.AlreadyStaged => "alreadyStaged",
            HistoryDecisionStatus.AlreadyMapped => "alreadyMapped",
            HistoryDecisionStatus.NeedsReview => "needsReview",
            HistoryDecisionStatus.Excluded => "excluded",
            _ => "invalid"
        }, Reasons.Order(StringComparer.Ordinal).ToArray());
}
