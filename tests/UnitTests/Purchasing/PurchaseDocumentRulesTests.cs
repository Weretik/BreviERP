using System.Text.Json;
using Purchasing.Domain.Documents;

namespace UnitTests.Purchasing;

/// <summary>PUR-001 stage 3: facts of an invoice and their kopeck arithmetic.</summary>
public sealed class PurchaseDocumentRulesTests
{
    private const string Uid = "391813863";
    private const string Link = "https://www.dropbox.com/scl/fi/abc/file_749.pdf?rlkey=x&dl=0";

    internal const string WithoutVat = """
        {"documentType":"Рахунок-фактура","documentNumber":"110776","documentDate":"2026-10-21",
         "supplierName":"ФОП БИСТРОВ ДМИТРО АНДРІЙОВИЧ","vat":"without_vat","pricesIncludeVat":false,
         "items":[{"name":"Розміщення реклами","quantity":"1","unit":"посл.","price":"8000","sum":"8000.00"}],
         "vatAmount":null,"totalPayable":"8000.00"}
        """;

    internal const string VatOnTop = """
        {"documentType":"Рахунок на оплату","documentNumber":"СФ-15","documentDate":"2026-11-03","supplierName":"ТОВ Текстиль-Контакт",
         "vat":"with_vat","pricesIncludeVat":false,
         "items":[{"name":"Тканина саржа","quantity":"12.5","unit":"м","price":"100.10","sum":"1251.25"},
                  {"name":"Нитки","quantity":"3","unit":"шт","price":"16.25","sum":"48.75"}],
         "vatAmount":"260.00","totalPayable":"1560.00"}
        """;

    private static (PurchaseDocument? Document, IReadOnlyList<string> Problems) Read(string json, bool identity = false)
    {
        using var parsed = JsonDocument.Parse(json);
        return PurchaseDocumentRules.Read(parsed.RootElement, Uid, Link, identity);
    }

    private static string With(string json, Action<System.Text.Json.Nodes.JsonObject> change)
    {
        var node = System.Text.Json.Nodes.JsonNode.Parse(json)!.AsObject();
        change(node);
        return node.ToJsonString();
    }

    [Test]
    public void A_document_without_vat_is_read_and_written_in_a_fixed_order()
    {
        var (document, problems) = Read(WithoutVat);

        Assert.That(problems, Is.Empty);
        Assert.That(document!.Vat, Is.EqualTo(PurchaseVatMode.WithoutVat));
        Assert.That(document.Items.Single().Position, Is.EqualTo(1));
        var json = PurchaseDocumentJson.Write(document);
        Assert.That(json, Does.StartWith($$"""{"sourceUID":"{{Uid}}","originalFileLink":"{{Link}}","documentType":"Рахунок-фактура","""));
        Assert.That(json, Does.Contain("\"vatAmount\":null,\"totalPayable\":\"8000.00\"}"));
        Assert.That(PurchaseDocumentJson.Read(json), Is.EqualTo(document).Using<PurchaseDocument>((a, b) =>
            PurchaseDocumentJson.Write(a) == PurchaseDocumentJson.Write(b)));
    }

    [Test]
    public void Vat_on_top_of_the_positions_adds_up_in_kopecks()
    {
        var (document, problems) = Read(VatOnTop);
        Assert.That(problems, Is.Empty);
        Assert.That(document!.Vat, Is.EqualTo(PurchaseVatMode.WithVat));
    }

    [Test]
    public void Prices_with_vat_must_add_up_to_the_total_alone()
    {
        var json = With(VatOnTop, x => x["pricesIncludeVat"] = true);
        Assert.That(Read(json).Problems, Is.EqualTo(new[] { "Сума позицій 1300.00, а до оплати 1560.00." }));
        Assert.That(Read(With(json, x => x["totalPayable"] = "1300.00")).Problems, Is.Empty);
    }

    [Test]
    public void One_kopeck_off_is_a_problem()
    {
        var (document, problems) = Read(With(VatOnTop, x => x["totalPayable"] = "1560.01"));
        Assert.That(document, Is.Null);
        Assert.That(problems, Is.EqualTo(new[] { "Сума позицій 1300.00 + ПДВ 260.00 = 1560.00, а до оплати 1560.01." }));
    }

    [Test]
    public void A_misread_line_is_reported_but_a_rounded_price_is_not()
    {
        var misread = With(WithoutVat, x => x["items"]![0]!["quantity"] = "2");
        Assert.That(Read(misread).Problems, Has.Some.Contains("кількість × ціна = 16000.00"));

        var rounded = With(WithoutVat, x =>
        {
            x["items"]![0]!["quantity"] = "3";
            x["items"]![0]!["price"] = "2666.67";
        });
        Assert.That(Read(rounded).Problems, Is.Empty);
    }

    [Test]
    public void Without_vat_cannot_carry_a_vat_amount_and_with_vat_needs_one()
    {
        Assert.That(Read(With(WithoutVat, x => x["vatAmount"] = "10.00")).Problems,
            Is.EqualTo(new[] { "Рахунок без ПДВ не може мати суму ПДВ." }));
        Assert.That(Read(With(WithoutVat, x => x["vatAmount"] = "0.00")).Document!.VatAmount, Is.Null);
        Assert.That(Read(With(VatOnTop, x => x["vatAmount"] = null)).Problems,
            Has.Some.EqualTo("Для рахунку з ПДВ потрібна сума ПДВ (число з крапкою)."));
        Assert.That(Read(With(WithoutVat, x => x["vat"] = "unknown")).Problems,
            Has.Some.EqualTo("vat має бути with_vat або without_vat."));
    }

    [Test]
    public void Numbers_with_commas_or_spaces_and_missing_facts_are_problems()
    {
        var json = With(WithoutVat, x =>
        {
            x["totalPayable"] = "8 000,00";
            x["documentNumber"] = " ";
            x["documentDate"] = "21.10.2026";
        });
        Assert.That(Read(json).Problems, Is.EquivalentTo(new[]
        {
            "Номер рахунку: не знайдено.",
            "Дата рахунку не знайдена або не у форматі РРРР-ММ-ДД.",
            "Сума до оплати не знайдена або не число з крапкою (до 2 знаків).",
        }));
    }

    [Test]
    public void A_manual_file_must_name_the_same_uid_and_link()
    {
        var json = With(WithoutVat, x =>
        {
            x["sourceUID"] = "111111111";
            x["originalFileLink"] = Link + "&x=1";
        });
        Assert.That(Read(json, identity: true).Problems, Is.EqualTo(new[]
        {
            $"sourceUID має бути {Uid}.", "originalFileLink не збігається з посиланням цього файлу.",
        }));
    }

    [Test]
    public void Kopecks_are_exact()
    {
        Assert.That(PurchaseDocumentRules.Kopecks("0.1"), Is.EqualTo(10));
        Assert.That(PurchaseDocumentRules.Kopecks("123456789012.99"), Is.EqualTo(12345678901299));
        Assert.That(PurchaseDocumentRules.Kopecks("1.999"), Is.Null);
    }
}
