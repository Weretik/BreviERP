using Media.Domain.Folders;

namespace Media.Application.Features.Folders.Shared;

internal static class FolderMessages
{
    public const string NotFound = "Папку не знайдено.";
    public const string ParentNotFound = "Батьківську папку не знайдено.";
    public const string DuplicateName = "У цій папці вже є папка з такою назвою.";
    public static string TooDeep => $"Можна вкладати не більше {MediaFolderTree.MaxDepth} рівнів папок.";
    public const string MoveIntoItself = "Папку не можна перемістити в неї саму або в її підпапку.";
    public const string NotEmpty = "Папка не порожня: спершу перемістіть або видаліть її фото й підпапки.";
}
