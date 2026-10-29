using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Application.Documents.Contracts;
using System.Text.Json;

namespace UnitTests.Crm.Documents;

public sealed class IssuerSettingsRulesTests
{
    private static readonly CrmUserId Admin = new(7);

    [Test]
    public void Issuer_and_template_inputs_reject_unmapped_json_fields()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IssuerSettingsInput>(
            """{"legalName":"Brevi","unexpected":"internal"}""", new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DocumentTemplateInput>(
            """{"layoutKey":"standard","unexpected":"internal"}""", new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }

    [Test]
    public void Verified_issuer_version_requires_legal_and_contact_fields()
    {
        Assert.Throws<DocumentSettingsValidationException>(() => IssuerSettingsVersion.Create(1, "", "35521636",
            "355216316015", "UA633052990000026004041206862", "Полтава", "+380951645855",
            "pp.brevi@gmail.com", "https://brevi.com.ua/", null, null, null, Admin));
        Assert.Throws<DocumentSettingsValidationException>(() => IssuerSettingsVersion.Create(1, "ПП Бреві", "35521636",
            "355216316015", "UA633052990000026004041206862", "Полтава", "+380951645855",
            "invalid", "https://brevi.com.ua/", null, null, null, Admin));
    }

    [Test]
    public void Logo_key_cannot_be_an_external_or_arbitrary_file_path()
    {
        foreach (var key in new[] { "https://example.com/logo.svg", "../secret.svg", "/brand/logo.svg" })
            Assert.Throws<DocumentSettingsValidationException>(() => IssuerSettingsVersion.Create(1, "ПП Бреві",
                "35521636", "355216316015", "UA633052990000026004041206862", "Полтава", "+380951645855",
                "pp.brevi@gmail.com", "https://brevi.com.ua/", null, null, key, Admin));
    }

    [Test]
    public void Template_version_rejects_uncontrolled_layout_and_blank_tax_wording()
    {
        Assert.Throws<DocumentSettingsValidationException>(() => DocumentTemplateVersion.Create(1, "rawHtml",
            "КП", "Усі ціни в грн з ПДВ.", "Вступ", "Заголовок", "Текст", "Контакт", Admin));
        Assert.Throws<DocumentSettingsValidationException>(() => DocumentTemplateVersion.Create(1, "standard",
            "КП", "", "Вступ", "Заголовок", "Текст", "Контакт", Admin));
    }

    [Test]
    public void Active_pointer_requires_revision_and_preserves_selected_version_ids()
    {
        var state = DocumentSettingsState.Create();
        Assert.Throws<DocumentSettingsValidationException>(() =>
            state.Activate(2, new IssuerSettingsVersionId(1), new DocumentTemplateVersionId(1)));
        state.Activate(1, new IssuerSettingsVersionId(11), new DocumentTemplateVersionId(12));
        Assert.Multiple(() =>
        {
            Assert.That(state.Revision, Is.EqualTo(2));
            Assert.That(state.ActiveIssuerVersionId, Is.EqualTo(new IssuerSettingsVersionId(11)));
            Assert.That(state.ActiveTemplateVersionId, Is.EqualTo(new DocumentTemplateVersionId(12)));
        });
    }
}
