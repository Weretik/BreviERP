namespace Crm.Domain.Documents;

public sealed class DocumentSettingsValidationException(string code) : Exception(code);
