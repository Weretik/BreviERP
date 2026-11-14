using System.Security.Cryptography;
using System.Text;
using Purchasing.Domain.Ingestion;
using Purchasing.Domain.Messages;
using Purchasing.Domain.Ingestion.Normalization;

namespace UnitTests.Purchasing.Sources;

public sealed class PurchaseSourceRulesTests
{
    private static string Sha(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private static NormalizedSourceRow Normalize(string?[] row, int rowNumber = 2) =>
        SourceRowNormalizer.Normalize(row, rowNumber, FakeSheetReader.SpreadsheetId, FakeSheetReader.SheetId).Value!;

    [Test]
    public void IdsFollowTheCalculatorFormulas()
    {
        var row = Normalize(SheetRow.Of(" 123456789 ", chatId: " -1001 ", messageId: "10 "));

        Assert.That(row.TelegramKey, Is.EqualTo("-1001:10"));
        Assert.That(row.MessageId, Is.EqualTo(Sha("telegram:-1001:10")));
        Assert.That(row.SourceId, Is.EqualTo(Sha("sheet-1:824424675:123456789")));
        Assert.That(row.Uid, Is.EqualTo("123456789"));
        Assert.That(PurchaseIdentity.FileId(row.MessageId, SheetRow.Dropbox), Is.EqualTo(Sha($"{row.MessageId}:{SheetRow.Dropbox}")));
        Assert.That(PurchaseIdentity.RevisionId(row.SourceId, row.RawJson), Is.EqualTo(Sha($"{row.SourceId}:{row.RawJson}")));
    }

    [Test]
    public void RawJsonIsWrittenLikeJavaScriptJsonStringify()
    {
        Assert.That(JsJson.String("a\"b\\\n\t\u0001é/<>&'"), Is.EqualTo("\"a\\\"b\\\\\\n\\t\\u0001é/<>&'\""));
        Assert.That(JsJson.String("x\ud800y"), Is.EqualTo("\"x\\ud800y\""));
        Assert.That(JsJson.String("😀"), Is.EqualTo("\"😀\""));

        var row = Normalize(SheetRow.Of("1", text: "Рядок 1\nрядок 2"));
        Assert.That(row.RawJson, Does.StartWith("{\"UID\":\"1\",\"ChatName\":\"БРЕВІ команда\",\"ChatId\":\"-1001\""));
        Assert.That(row.RawJson, Does.Contain("\"Text\":\"Рядок 1\\nрядок 2\""));
        Assert.That(row.RawJson, Does.EndWith("\"FromUserStatus\":\"\"}"));
    }

    [Test]
    public void LinksAreSplitTrimmedAndDeduplicatedWithoutRewriting()
    {
        var a = "https://www.dropbox.com/scl/fi/a/1.pdf?dl=0";
        var b = "https://dropbox.com/scl/fi/b/2.pdf?dl=0";

        Assert.That(FileLinkRules.Extract($"{a},{b}; {a};"), Is.EqualTo(new[] { a, b }));
        Assert.That(FileLinkRules.Extract($"{a};{b}"), Is.EqualTo(new[] { a, b }));
        Assert.That(FileLinkRules.Extract("без посилань"), Is.Empty);
    }

    [Test]
    public void ProblemsAreListedWithTheirReason()
    {
        Assert.That(Normalize(SheetRow.Of("1")).Issues, Is.Empty);
        Assert.That(Normalize(SheetRow.Of("1", fileCount: "2")).Issues, Is.EqualTo(new[] { "FileCount = 2, а посилань знайдено 1" }));
        Assert.That(Normalize(SheetRow.Of("1", fileCount: "1,5")).Issues, Is.EqualTo(new[] { "FileCount «1,5» не є цілим числом" }));
        Assert.That(Normalize(SheetRow.Of("1", fileLinks: "http://www.dropbox.com/x")).Issues,
            Is.EqualTo(new[] { "Посилання не з Dropbox або небезпечне: http://www.dropbox.com/x" }));
        Assert.That(Normalize(SheetRow.Of("1", fileLinks: "https://user:pw@www.dropbox.com/x")).Issues, Has.Count.EqualTo(1));
        Assert.That(Normalize(SheetRow.Of("1", fileLinks: "https://drive.google.com/x")).Issues, Has.Count.EqualTo(1));
        Assert.That(Normalize(SheetRow.Of("1", fileLinks: "див. пошту", fileCount: "0")).Issues,
            Is.EqualTo(new[] { "У FileLinks немає розпізнаних посилань", "Немає посилання на оригінал" }));
    }

    [TestCase("СПЛАЧЕНО", PaymentState.MarkedPaid)]
    [TestCase("  сплачено ", PaymentState.MarkedPaid)]
    [TestCase("СПЛАЧЕНО ЧАСТКОВО", PaymentState.Unconfirmed)]
    [TestCase("", PaymentState.Unconfirmed)]
    public void OnlyTheExactPaidMarkCountsAsPaid(string status, PaymentState expected) =>
        Assert.That(Normalize(SheetRow.Of("1", status: status)).Message.PaymentState, Is.EqualTo(expected));

    [Test]
    public void EmptyRowsAreSkippedAndRowsWithoutIdentityAreRefused()
    {
        Assert.That(SourceRowNormalizer.Normalize(["", " ", null], 5, "s", 1).Value, Is.Null);
        var error = SourceRowNormalizer.Normalize(SheetRow.Of("", chatId: "-1"), 7, "s", 1).Error;
        Assert.That(error!.Message, Does.Contain("Рядок 7"));
    }

    [TestCase("05.11.2026 14:22:01", "2026-11-05T14:22:01")]
    [TestCase("5/11/2026", "2026-11-05T00:00:00")]
    [TestCase("2026-11-05T09:30", "2026-11-05T09:30:00")]
    public void MessageDatesAreRecognisedForSorting(string text, string expected) =>
        Assert.That(MessageDates.Parse(text), Is.EqualTo(DateTime.Parse(expected, System.Globalization.CultureInfo.InvariantCulture)));

    [Test]
    public void UnknownDatesSortLast() =>
        Assert.That(MessageDates.Parse("вчора"), Is.Null);

    [Test]
    public void TheSheetIsCheckedAsAWhole()
    {
        var rows = new[] { Normalize(SheetRow.Of("1"), 2), Normalize(SheetRow.Of("1", messageId: "11"), 3) };
        Assert.That(PurchaseSourceSnapshot.Create(rows).Error!.Message,
            Does.Contain("UID 1 повторюється в рядках 2, 3"));

        var marks = new[]
        {
            Normalize(SheetRow.Of("1", status: "СПЛАЧЕНО"), 2),
            Normalize(SheetRow.Of("2", status: ""), 3),
        };
        Assert.That(PurchaseSourceSnapshot.Create(marks).Error!.Message,
            Does.Contain("різні Status"));
    }
}
