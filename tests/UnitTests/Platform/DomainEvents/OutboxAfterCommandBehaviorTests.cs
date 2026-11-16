using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.DomainEvents;
using Mediator;

namespace UnitTests.Platform.DomainEvents;

/// <summary>ACT-001 R-05: the messages a command saved are tried once right after it.</summary>
public sealed class OutboxAfterCommandBehaviorTests
{
    [Test]
    public async Task Saved_messages_are_dispatched_once_after_the_command()
    {
        var saved = new SavedOutboxMessages();
        var outbox = new RecordingDispatcher();
        var behavior = new OutboxAfterCommandBehavior<Ping, string>(saved, outbox);
        var id = Guid.CreateVersion7();

        var answer = await behavior.Handle(new Ping(), (_, _) =>
        {
            saved.Add([id]);
            return ValueTask.FromResult("pong");
        }, CancellationToken.None);

        Assert.That(answer, Is.EqualTo("pong"));
        Assert.That(outbox.Dispatched, Is.EqualTo(new[] { new[] { id } }));
        Assert.That(saved.Take(), Is.Empty);
    }

    [Test]
    public async Task Nothing_is_dispatched_when_the_command_saved_no_message()
    {
        var outbox = new RecordingDispatcher();
        var behavior = new OutboxAfterCommandBehavior<Ping, string>(new SavedOutboxMessages(), outbox);

        await behavior.Handle(new Ping(), (_, _) => ValueTask.FromResult("pong"), CancellationToken.None);

        Assert.That(outbox.Dispatched, Is.Empty);
    }

    public sealed record Ping : IRequest<string>;

    private sealed class RecordingDispatcher : IOutboxDispatcher
    {
        public List<Guid[]> Dispatched { get; } = [];

        public Task DispatchAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        {
            Dispatched.Add(ids.ToArray());
            return Task.CompletedTask;
        }

        public Task<OutboxRun> DispatchDueAsync(CancellationToken cancellationToken) => Task.FromResult(OutboxRun.Empty);
    }
}
