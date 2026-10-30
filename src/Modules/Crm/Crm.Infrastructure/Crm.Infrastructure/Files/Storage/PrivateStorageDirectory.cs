using System.Security.AccessControl;
using System.Security.Principal;

namespace Crm.Infrastructure.Files.Storage;

internal static class PrivateStorageDirectory
{
    internal static string Open(PrivateFileStorageOptions options)
    {
        if (!Path.IsPathFullyQualified(options.RootPath) || !Path.IsPathFullyQualified(options.PublicWebRootPath))
            throw new InvalidOperationException("Private storage and public web roots must be explicitly configured absolute paths.");
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.RootPath));
        var publicRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.PublicWebRootPath));
        if (root == Path.GetPathRoot(root) || IsWithin(root, publicRoot) || IsWithin(publicRoot, root) || root.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(x => x.Equals("wwwroot", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Private storage requires a dedicated directory outside the public web root.");
        RejectLinks(root);
        if (!Directory.Exists(root))
        {
            if (OperatingSystem.IsWindows())
            {
                Directory.CreateDirectory(root);
                var security = new DirectorySecurity();
                security.SetAccessRuleProtection(true, false);
                var owner = WindowsIdentity.GetCurrent().User ?? throw new InvalidOperationException("Cannot identify the storage service identity.");
                foreach (var sid in new[] { owner, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) })
                    security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                new DirectoryInfo(root).SetAccessControl(security);
            }
            else Directory.CreateDirectory(root, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
        EnsurePrivate(root);
        return root;
    }

    internal static void EnsurePrivate(string path)
    {
        RejectLinks(path);
        if (OperatingSystem.IsWindows())
        {
            var allowed = new[] { WindowsIdentity.GetCurrent().User!.Value, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null).Value, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Value };
            var security = new DirectoryInfo(path).GetAccessControl();
            if (!security.AreAccessRulesProtected)
                throw new InvalidOperationException("Private storage ACL must not inherit permissions.");
            foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier)))
                if (rule.AccessControlType == AccessControlType.Allow && !allowed.Contains(rule.IdentityReference.Value))
                    throw new InvalidOperationException("Private storage ACL permits another identity.");
        }
        else if ((File.GetUnixFileMode(path) & (UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute)) != 0)
            throw new InvalidOperationException("Private storage permissions must be restricted to the service user.");
    }

    internal static void RejectLinks(string path)
    {
        for (var current = new DirectoryInfo(path); current != null; current = current.Parent)
            if (current.LinkTarget is not null || (current.Exists && (current.Attributes & FileAttributes.ReparsePoint) != 0))
                throw new InvalidOperationException("Private storage cannot traverse symbolic links or reparse points.");
        if (new FileInfo(path).LinkTarget is not null || (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0))
            throw new InvalidOperationException("Private storage cannot read symbolic links or reparse points.");
    }

    private static bool IsWithin(string path, string parent) => path.Equals(parent, StringComparison.OrdinalIgnoreCase) || path.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
}
