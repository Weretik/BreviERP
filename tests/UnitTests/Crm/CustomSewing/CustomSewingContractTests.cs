using YamlDotNet.RepresentationModel;

namespace UnitTests.Crm.CustomSewing;

public sealed class CustomSewingContractTests
{
    [Test]
    public void Canonical_contract_has_all_custom_operations_and_resolvable_schema_references()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docs", "sdd", "contracts", "openapi.yaml")))
            directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        var file = Path.Combine(directory!.FullName, "docs", "sdd", "contracts", "crm", "custom-sewing.openapi.yaml");
        var yaml = new YamlStream(); using (var reader = File.OpenText(file)) yaml.Load(reader);
        var root = (YamlMappingNode)yaml.Documents[0].RootNode;
        var operations = new List<string>();
        void Walk(YamlNode node)
        {
            if (node is YamlMappingNode map)
                foreach (var pair in map.Children)
                {
                    var key = ((YamlScalarNode)pair.Key).Value;
                    if (key == "operationId") operations.Add(((YamlScalarNode)pair.Value).Value!);
                    if (key == "$ref")
                    {
                        var target = ((YamlScalarNode)pair.Value).Value!;
                        var parts = target.Split('#');
                        var referenced = parts[0].Length == 0 ? file : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, parts[0]));
                        Assert.That(File.Exists(referenced), Is.True, target);
                        var other = new YamlStream(); using (var input = File.OpenText(referenced)) other.Load(input);
                        YamlNode selected = other.Documents[0].RootNode;
                        foreach (var field in parts[1].TrimStart('/').Split('/'))
                            selected = ((YamlMappingNode)selected).Children[new YamlScalarNode(field.Replace("~1", "/").Replace("~0", "~"))];
                        Assert.That(selected, Is.Not.Null);
                    }
                    Walk(pair.Value);
                }
            else if (node is YamlSequenceNode sequence) foreach (var child in sequence.Children) Walk(child);
        }
        Walk(root);
        Assert.That(operations, Is.EquivalentTo(new[] { "createCrmCustomSewingDraft", "getCrmCustomSewingDraft",
            "saveCrmCustomSewingDraft", "calculateCrmCustomSewing", "saveCrmCustomSewingVersion",
            "getCrmCustomSewingVersions", "getCrmCustomSewingVersion" }));
    }
}
