using System.Text.Json.Nodes;
using Reference.Infrastructure.Integrations.KeepinCrm;

namespace UnitTests.Reference.Suppliers;

public sealed class KeepinCrmSupplierParserTests
{
    [Test]
    public void Reads_the_same_fields_as_the_sheet_script()
    {
        var page = JsonNode.Parse("""
            {
              "items": [
                {
                  "id": 104551,
                  "name": " ТК-Домашній текстиль ",
                  "phones": [{ "phone": "38 (093) 269 13 29" }, "0532671109", "0532671109"],
                  "emails": ["tpidhaiets@tk-domtex.com.ua"],
                  "tag_names": ["тканина", "опт"],
                  "custom_fields": {
                    "shcho_zakupovuiemo_323": "тканина",
                    "sait_324": { "value": "https://tk-domtex.com.ua" },
                    "kontaktnaya_osoba_297": "Тетяна"
                  }
                }
              ],
              "pagination": { "page": 1 }
            }
            """);

        var supplier = KeepinCrmSupplierParser.ParsePage(page).Single();

        Assert.Multiple(() =>
        {
            Assert.That(supplier.KeepinCrmId, Is.EqualTo("104551"));
            Assert.That(supplier.Name, Is.EqualTo("ТК-Домашній текстиль"));
            Assert.That(supplier.PurchaseCategories, Is.EqualTo("тканина"));
            Assert.That(supplier.Link, Is.EqualTo("https://tk-domtex.com.ua"));
            Assert.That(supplier.ContactPerson, Is.EqualTo("Тетяна"));
            Assert.That(supplier.PhoneNumber, Is.EqualTo("38 (093) 269 13 29, 0532671109"));
            Assert.That(supplier.Email, Is.EqualTo("tpidhaiets@tk-domtex.com.ua"));
            Assert.That(supplier.Tags, Is.EqualTo("тканина, опт"));
        });
    }

    [Test]
    public void Finds_custom_fields_in_an_array_by_name_and_the_list_in_nested_data()
    {
        var page = JsonNode.Parse("""
            {
              "data": {
                "suppliers": [
                  {
                    "id": "39475",
                    "title": "Вишивка Киев Volia Branding",
                    "contact": { "full_name": "Ігор" },
                    "custom_fields": [
                      { "name": "Що закуповуємо", "value": ["послуги"] },
                      { "code": "sait_324", "value": "https://voliabranding.com.ua/" }
                    ]
                  }
                ]
              }
            }
            """);

        var supplier = KeepinCrmSupplierParser.ParsePage(page).Single();

        Assert.Multiple(() =>
        {
            Assert.That(supplier.Name, Is.EqualTo("Вишивка Киев Volia Branding"));
            Assert.That(supplier.PurchaseCategories, Is.EqualTo("послуги"));
            Assert.That(supplier.Link, Is.EqualTo("https://voliabranding.com.ua/"));
            Assert.That(supplier.ContactPerson, Is.EqualTo("Ігор"));
            Assert.That(supplier.Email, Is.Null);
        });
    }

    [Test]
    public void Empty_or_unrelated_payload_has_no_rows()
    {
        Assert.Multiple(() =>
        {
            Assert.That(KeepinCrmSupplierParser.CountRows(JsonNode.Parse("[]")), Is.Zero);
            Assert.That(KeepinCrmSupplierParser.ParsePage(JsonNode.Parse("""{ "items": [] }""")), Is.Empty);
            Assert.That(KeepinCrmSupplierParser.ParsePage(JsonNode.Parse("""{ "meta": { "total": 0 } }""")), Is.Empty);
        });
    }
}
