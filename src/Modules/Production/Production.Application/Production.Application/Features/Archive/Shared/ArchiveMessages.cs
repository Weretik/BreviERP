namespace Production.Application.Features.Archive.Shared;

internal static class ArchiveMessages
{
    public const string CannotImport = "Переносити архів може лише адміністратор.";
    public const string Kind = "Невідомий розділ архіву.";
    public const string Period = "Дата «від» має бути не пізніше дати «до».";
    public const int MaxPageSize = 100;
    public const int MaxImportRows = 5000;
}
