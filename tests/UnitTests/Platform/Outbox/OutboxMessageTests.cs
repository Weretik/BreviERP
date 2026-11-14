using BuildingBlocks.Application.Outbox;

namespace UnitTests.Platform.Outbox;

/// <summary>PLT-003 R-03, R-04: the outbox row waits longer after each failure and gives up after ten.</summary>
public sealed class OutboxMessageTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 9, 0, 0, TimeSpan.Zero);

    [Test]
    public void A_new_message_is_due_at_once_with_its_payload()
    {
        var message = OutboxMessage.Create("crm.site-request.notify", new Payload(42), Now);

        Assert.Multiple(() =>
        {
            Assert.That(message.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(message.NextAttemptAt, Is.EqualTo(Now));
            Assert.That(message.Attempts, Is.Zero);
            Assert.That(OutboxMessage.Read<Payload>(message.Payload), Is.EqualTo(new Payload(42)));
        });
    }

    [Test]
    public void Each_failure_waits_twice_as_long_up_to_an_hour()
    {
        var message = OutboxMessage.Create("t", new Payload(1), Now);
        var delays = new List<double>();
        for (var i = 0; i < 8; i++)
        {
            message.Fail("Telegram недоступний.", Now);
            delays.Add((message.NextAttemptAt - Now).TotalMinutes);
        }

        Assert.That(delays, Is.EqualTo(new double[] { 1, 2, 4, 8, 16, 32, 60, 60 }));
    }

    [Test]
    public void The_tenth_failure_gives_up_and_keeps_a_short_error()
    {
        var message = OutboxMessage.Create("t", new Payload(1), Now);
        for (var i = 0; i < OutboxMessage.MaxAttempts; i++)
            message.Fail(new string('x', 5000), Now);

        Assert.Multiple(() =>
        {
            Assert.That(message.GaveUp, Is.True);
            Assert.That(message.LastError, Has.Length.EqualTo(OutboxMessage.ErrorMaxLength));
        });
    }

    [Test]
    public void Success_clears_the_error()
    {
        var message = OutboxMessage.Create("t", new Payload(1), Now);
        message.Fail("збій", Now);
        message.Succeed(Now.AddMinutes(1));

        Assert.Multiple(() =>
        {
            Assert.That(message.ProcessedAt, Is.EqualTo(Now.AddMinutes(1)));
            Assert.That(message.LastError, Is.Null);
            Assert.That(message.GaveUp, Is.False);
        });
    }

    [Test]
    public void A_type_is_required() =>
        Assert.Throws<ArgumentException>(() => OutboxMessage.Create(" ", new Payload(1), Now));

    private sealed record Payload(long Id);
}
