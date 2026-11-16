using ActivityLog.Contracts.Activity;
using BuildingBlocks.Application.Actors;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Domain.Events;

namespace ActivityLog.Application.Features.Entries.EventHandlers;

/// <summary>
/// Turns a domain event of any module into an <c>activity.recorded</c> outbox message (ADR-0016): the module's
/// <see cref="IActivityDescriber"/> says what happened, this adds who did it and when. The message goes into the
/// saving module's own outbox. Without a signed-in person nothing is written (ACT-001 R-01).
/// </summary>
public sealed class ActivityDomainEventOutbox(IEnumerable<IActivityDescriber> describers) : IDomainEventOutbox
{
    private readonly IActivityDescriber[] _describers = describers.ToArray();

    public IReadOnlyList<OutboxMessage> Write(IDomainEvent domainEvent, Actor? actor, DateTimeOffset now)
    {
        if (actor is null) return [];
        return _describers.Select(describer => describer.Describe(domainEvent)).OfType<ActivityDescription>()
            .Select(description => OutboxMessage.Create(ActivityMessage.Type, new ActivityRecorded(
                Guid.CreateVersion7(now), now, actor.UserId, actor.Login, actor.Roles, description.Action,
                description.SubjectType, description.SubjectTypeTitle, description.SubjectId, description.SubjectTitle,
                description.Summary, description.Changes, description.SourceModule), now))
            .ToArray();
    }
}
