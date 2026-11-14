using Scheduling.Application.Features.Tasks.Shared;

namespace UnitTests.Scheduling.Application;

/// <summary>Admin limits of a schedule (SCH-001 R-02): Ukrainian messages, at least every 5 minutes.</summary>
public sealed class ScheduleInputValidatorTests
{
    private static readonly ScheduleInputValidator Validator = new();

    [TestCase("interval", 5, null, null)]
    [TestCase("interval", 60, 8, 20)]
    [TestCase("manual", null, null, null)]
    public void Valid_intervals_and_manual_pass(string kind, int? every, int? from, int? to) =>
        Assert.That(Validator.Validate(new ScheduleInput(kind, every, from, to)).IsValid, Is.True);

    [TestCase("interval", 1, null, null)]
    [TestCase("interval", 60, 8, null)]
    [TestCase("interval", 60, 20, 8)]
    [TestCase("weekly", null, null, null)]
    public void Invalid_intervals_fail(string kind, int? every, int? from, int? to) =>
        Assert.That(Validator.Validate(new ScheduleInput(kind, every, from, to)).IsValid, Is.False);

    [Test]
    public void Daily_needs_valid_distinct_times_and_days()
    {
        Assert.That(Validator.Validate(new ScheduleInput("daily", Times: ["07:00", "13:00"], Days: [1, 5])).IsValid, Is.True);
        Assert.That(Validator.Validate(new ScheduleInput("daily", Times: ["7"], Days: [1])).IsValid, Is.False);
        Assert.That(Validator.Validate(new ScheduleInput("daily", Times: ["07:00", "07:00"], Days: [1])).IsValid, Is.False);
        Assert.That(Validator.Validate(new ScheduleInput("daily", Times: ["07:00"], Days: [])).IsValid, Is.False);
        Assert.That(Validator.Validate(new ScheduleInput("daily", Times: ["07:00"], Days: [8])).IsValid, Is.False);
        var error = Validator.Validate(new ScheduleInput("daily", Times: [], Days: [1])).Errors.Single();
        Assert.That(error.ErrorMessage, Does.StartWith("Вкажіть"));
    }
}
