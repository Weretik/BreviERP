using System.Text.Json;
using Crm.Domain.Quotes.Introductions;
using YamlDotNet.RepresentationModel;

namespace UnitTests.Crm.Introductions;

public sealed class IntroductionOpenApiContractTests
{
    private static string Repository()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docs/sdd/contracts/openapi.yaml")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository not found.");
    }

    private static YamlMappingNode Load(string file)
    {
        var yaml = new YamlStream();
        using var input = File.OpenText(file);
        yaml.Load(input);
        return (YamlMappingNode)yaml.Documents[0].RootNode;
    }

    [Test]
    public void Introduction_contract_operations_and_references_match_current_transport()
    {
        var repo = Repository();
        var file = Path.Combine(repo, "docs/sdd/contracts/crm/introduction-templates.openapi.yaml");
        var root = Load(file);
        var ids = new List<string>();
        void Walk(YamlNode node)
        {
            if (node is YamlMappingNode map)
                foreach (var pair in map.Children)
                {
                    var key = ((YamlScalarNode)pair.Key).Value;
                    if (key == "operationId") ids.Add(((YamlScalarNode)pair.Value).Value!);
                    if (key == "$ref")
                    {
                        var target = ((YamlScalarNode)pair.Value).Value!;
                        var parts = target.Split('#');
                        var referenced = parts[0].Length == 0 ? file : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, parts[0]));
                        YamlNode selected = Load(referenced);
                        foreach (var field in parts[1].TrimStart('/').Split('/'))
                            selected = ((YamlMappingNode)selected).Children[new YamlScalarNode(field.Replace("~1", "/").Replace("~0", "~"))];
                        Assert.That(selected, Is.Not.Null, target);
                    }
                    Walk(pair.Value);
                }
            else if (node is YamlSequenceNode sequence)
                foreach (var child in sequence.Children) Walk(child);
        }
        Walk(root);
        Assert.That(ids, Is.EquivalentTo(new[] { "getCrmIntroductionTemplates", "createCrmIntroductionTemplate",
            "getCrmIntroductionTemplate", "reviseCrmIntroductionTemplate", "setCrmIntroductionTemplateArchived",
            "checkCrmIntroductionTemplate", "applyCrmIntroductionTemplate", "getCrmIntroductionFeaturedMapping",
            "activateCrmIntroductionFeaturedMapping" }));
        var controllers = File.ReadAllText(Path.Combine(repo, "src/Modules/Crm/Crm.Api/Crm.Api/Templates/IntroductionTemplatesController.cs")) +
            File.ReadAllText(Path.Combine(repo, "src/Modules/Crm/Crm.Api/Crm.Api/Templates/IntroductionFeaturedMappingController.cs"));
        foreach (var id in ids) Assert.That(controllers, Does.Contain($"Name = \"{id}\""));
    }

    [Test]
    public void Contract_keeps_strict_write_shapes_and_existing_pagination()
    {
        var root = Load(Path.Combine(Repository(), "docs/sdd/contracts/crm/introduction-templates.openapi.yaml"));
        var schemas = (YamlMappingNode)((YamlMappingNode)root["components"])["schemas"];
        foreach (var name in new[] { "CrmCreateIntroductionTemplateRequest", "CrmReviseIntroductionTemplateRequest",
                     "CrmArchiveIntroductionTemplateRequest", "CrmApplyIntroductionTemplateRequest",
                     "CrmIntroductionFeaturedMappingRequest", "CrmIntroductionSlotInput" })
            Assert.That(((YamlScalarNode)((YamlMappingNode)schemas[name])["additionalProperties"]).Value, Is.EqualTo("false"));
        var pageProperties = (YamlMappingNode)((YamlMappingNode)schemas["CrmIntroductionTemplatePage"])["properties"];
        Assert.That(pageProperties.Children.Keys.Select(x => ((YamlScalarNode)x).Value), Is.EquivalentTo(new[] { "value", "pagedInfo" }));
        Assert.That(((YamlScalarNode)((YamlMappingNode)pageProperties["pagedInfo"])["$ref"]).Value,
            Is.EqualTo("./clients-quotes.openapi.yaml#/components/schemas/CrmPagedInfo"));
    }

    [Test]
    public void Current_web_json_uses_numeric_response_enums_and_computed_slot_uid()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new { layout = IntroductionLayout.Winter,
            slot = new IntroductionSlot(IntroductionSeason.Winter, 2) }, options));
        Assert.That(json.RootElement.GetProperty("layout").GetInt32(), Is.EqualTo(2));
        Assert.That(json.RootElement.GetProperty("slot").GetProperty("season").GetInt32(), Is.EqualTo(1));
        Assert.That(json.RootElement.GetProperty("slot").GetProperty("uid").GetInt32(), Is.EqualTo(202));
    }
}
