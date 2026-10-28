using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.CustomSewing.Identifiers;

namespace UnitTests.Crm.Files;

public sealed class CustomFileLifecycleTests
{
    private static readonly DateTimeOffset At = DateTimeOffset.Parse("2026-10-28T12:00:00Z");
    private static CustomPrivateFile Reserve() => CustomPrivateFile.Reserve(new CustomDraftId(1), new CrmUserId(17),
        Guid.NewGuid(), new string('A', 64), "application/pdf", 30, new string('B', 64), At, At.AddHours(24));

    [Test]
    public void Verified_upload_completes_before_expiration()
    {
        var file = Reserve();
        file.Complete(At.AddMinutes(1));
        Assert.That(file.State, Is.EqualTo(CustomFileState.Uploaded));
    }

    [Test]
    public void Expired_reservation_cannot_become_a_finalized_file()
    {
        var file = Reserve();
        Assert.Throws<CustomSewingValidationException>(() => file.Complete(At.AddHours(24)));
        Assert.That(file.State, Is.EqualTo(CustomFileState.Reserved));
    }

    [TestCase(20971520, true)]
    [TestCase(20971521, false)]
    public void Domain_metadata_enforces_inclusive_twentieth_mib(long length, bool valid)
    {
        TestDelegate create = () => CustomPrivateFile.Reserve(new(1), new(17), Guid.NewGuid(),
            new string('A', 64), "application/pdf", length, new string('B', 64), At, At.AddHours(24));
        if (valid) Assert.DoesNotThrow(create);
        else Assert.Throws<CustomSewingValidationException>(create);
    }

    [Test]
    public void Malformed_internal_hash_is_a_validation_error()
    {
        Assert.Throws<CustomSewingValidationException>(() => CustomPrivateFile.Reserve(new(1), new(17), Guid.NewGuid(),
            null!, "application/pdf", 10, new string('B', 64), At, At.AddHours(24)));
        Assert.Throws<CustomSewingValidationException>(() => CustomPrivateFile.Reserve(new(1), new(17), Guid.NewGuid(),
            new string('A', 64), "application/pdf", 10, null!, At, At.AddHours(24)));
    }
}
