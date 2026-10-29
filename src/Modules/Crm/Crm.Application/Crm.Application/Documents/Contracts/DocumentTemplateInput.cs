namespace Crm.Application.Documents.Contracts;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record DocumentTemplateInput(string LayoutKey, string Title, string VatNotice, string OpeningText,
    string CtaTitle, string CtaText, string CtaContactTemplate);
