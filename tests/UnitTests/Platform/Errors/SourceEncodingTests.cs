using System.Text.RegularExpressions;

namespace UnitTests.Platform.Errors;

/// <summary>
/// Ukrainian messages must reach the admin as they were written. A file saved through the wrong code page turns each
/// Cyrillic letter into «Р» or «С» and a second sign («Р’РєР°Р¶С–С‚СЊ» instead of «Вкажіть»).
/// </summary>
public sealed class SourceEncodingTests
{
    // UTF-8 Cyrillic letters start with byte D0 or D1, which code page 1251 shows as «Р» and «С»; the second byte
    // (80–BF) becomes one of these signs. Three such pairs in a row never occur in real Ukrainian or Russian text.
    private static readonly Regex Mojibake = new(
        "(?:[РС][ЂЃ‚ѓ„…†‡€‰Љ‹ЊЌЋЏ" +
        "ђ‘’“”•–—™љ›њќћџ Ў" +
        "ўЈ¤Ґ¦§Ё©Є«¬­®Ї°±І" +
        "іґµ¶·ё№є»јЅѕї]){3,}",
        RegexOptions.Compiled);

    [Test]
    public void Source_files_have_no_broken_cyrillic()
    {
        var root = RepositoryRoot();
        var broken = new List<string>();
        foreach (var folder in new[] { "src", "tests" })
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, folder), "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                continue;
            // This file holds a broken sample on purpose.
            if (Path.GetFileName(file) == $"{nameof(SourceEncodingTests)}.cs") continue;
            var lines = File.ReadAllLines(file);
            for (var index = 0; index < lines.Length; index++)
                if (Mojibake.IsMatch(lines[index]))
                    broken.Add($"{Path.GetRelativePath(root, file)}:{index + 1}");
        }

        Assert.That(broken, Is.Empty, "Re-save these files as UTF-8 and retype the Ukrainian text.");
    }

    [Test]
    public void The_check_finds_text_saved_through_the_wrong_code_page()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Mojibake.IsMatch("Р’РєР°Р¶С–С‚СЊ РЅР°Р·РІСѓ"), Is.True);
            Assert.That(Mojibake.IsMatch("Вкажіть назву. Січень, Різдво, СРСР."), Is.False);
        });
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("BreviERP.sln was not found.");
    }
}
