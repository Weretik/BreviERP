using Ardalis.Result;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Features.Documents.Commands.RecognizePurchaseFile;
using Purchasing.Application.Features.Documents.Commands.StartNewPurchaseFilesRecognition;
using Purchasing.Application.Features.Documents.Shared.Recognition;
using Scheduling.Application.Features.Catalog.Shared;
using Scheduling.Contracts.Tasks;

namespace UnitTests.Purchasing;

/// <summary>
/// AI calls never outlive the HTTP request (AI-02): «Розпізнати всі нові» only starts the background task, and
/// «Розпізнати» of one file stops at the request time limit.
/// </summary>
public sealed class PurchaseRecognitionStartTests
{
    private const string By = "buh@brevi.com.ua";
    private PurchasingTestHarness harness = null!;
    private FakeLauncher launcher = null!;

    [SetUp]
    public async Task SetUp()
    {
        harness = new PurchasingTestHarness();
        launcher = new FakeLauncher();
        harness.Sheet.Rows =
        [
            SheetRow.Of("391813863", messageId: "1"),
            SheetRow.Of("391813865", messageId: "3", fileLinks: "http://example.com/a.pdf"),
        ];
        await harness.Sync().RunAsync(CancellationToken.None);
    }

    [TearDown]
    public void TearDown() => harness.Dispose();

    private Task<Result<PurchaseRecognitionStartResponse>> StartAsync() =>
        new StartNewPurchaseFilesRecognitionHandler(harness.Db, launcher)
            .Handle(new StartNewPurchaseFilesRecognitionCommand(By), CancellationToken.None).AsTask();

    [Test]
    public async Task Recognize_new_starts_the_background_task_and_reads_nothing_itself()
    {
        var result = await StartAsync();

        Assert.That(result.Value, Is.EqualTo(new PurchaseRecognitionStartResponse(1, true)), "the unsafe link does not count");
        Assert.That(launcher.Requests, Is.EqualTo(new[] { By }));
        Assert.That(harness.Recognizer.Requests, Is.Empty);
        Assert.That(harness.Fresh().RecognitionAttempts, Is.Empty);
    }

    [Test]
    public async Task Recognize_new_starts_nothing_when_no_new_original_waits()
    {
        var fileId = harness.Fresh().Files.Single(x => x.Url == SheetRow.Dropbox).Id;
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop));
        await harness.Recognition().RecognizeAsync(fileId.Value, By, CancellationToken.None);

        var result = await StartAsync();

        Assert.That(result.Value, Is.EqualTo(new PurchaseRecognitionStartResponse(0, false)));
        Assert.That(launcher.Requests, Is.Empty);
    }

    [Test]
    public async Task Recognize_new_while_the_task_runs_or_waits_is_a_conflict_with_a_clear_reason()
    {
        launcher.Answer = Result.Conflict("Задача вже виконується.");

        var result = await StartAsync();

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(result.Errors, Is.EqualTo(new[] { StartNewPurchaseFilesRecognitionHandler.AlreadyStarted }));
    }

    [Test]
    public void The_background_task_is_in_the_scheduler_catalogue()
    {
        var task = ScheduledTaskCatalog.Find(BackgroundTaskKeys.PurchaseRecognition);

        Assert.That(task, Is.Not.Null);
        Assert.That((task!.RunsInTick, task.EnabledByDefault), Is.EqualTo((false, false)));
    }

    [Test]
    public async Task One_file_stops_at_the_request_time_limit_and_keeps_what_the_first_answer_cost()
    {
        var fileId = harness.Fresh().Files.Single(x => x.Url == SheetRow.Dropbox).Id;
        harness.Recognizer.Answers.Enqueue(FakeRecognizer.Json(PurchaseDocumentRulesTests.VatOnTop.Replace("1560.00", "1500.00")));
        harness.Recognizer.Before[1] = token =>
        {
            harness.Time.Advance(PurchaseFileRecognition.RequestTimeLimit);
            token.ThrowIfCancellationRequested();
        };

        var result = await new RecognizePurchaseFileHandler(harness.Recognition())
            .Handle(new RecognizePurchaseFileCommand(fileId.Value, By), CancellationToken.None);

        Assert.That((result.Value.Result, result.Value.CostUsd), Is.EqualTo((PurchaseRecognitionResult.Failed, 0.002m)));
        Assert.That(result.Value.Problems[^1], Is.EqualTo(PurchaseFileRecognition.TooSlow));
        Assert.That(harness.Fresh().RecognitionAttempts.Single().FileId, Is.EqualTo(fileId));
    }

    private sealed class FakeLauncher : IPurchaseRecognitionLauncher
    {
        public List<string> Requests { get; } = [];
        public Result Answer { get; set; } = Result.Success();

        public Task<Result> RequestAsync(string by, CancellationToken cancellationToken)
        {
            if (Answer.IsSuccess) Requests.Add(by);
            return Task.FromResult(Answer);
        }
    }
}
