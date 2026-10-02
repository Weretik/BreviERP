using System.Reflection;
using System.Text.Json;
using Identity.Api.Contracts.Auth;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using YamlDotNet.RepresentationModel;

namespace UnitTests;

public sealed class SessionOpenApiContractTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string AggregateContractPath = Path.Combine(
        RepositoryRoot,
        "docs",
        "sdd",
        "contracts",
        "openapi.yaml");
    private static readonly string SessionContractPath = Path.Combine(
        RepositoryRoot,
        "docs",
        "sdd",
        "contracts",
        "identity",
        "session.openapi.yaml");

    [Test]
    public async Task Aggregate_contract_is_valid_and_all_references_resolve()
    {
        var settings = new OpenApiReaderSettings
        {
            LoadExternalRefs = true
        };
        settings.AddYamlReader();

        var (_, diagnostic) = await OpenApiDocument.LoadAsync(
            AggregateContractPath,
            settings,
            CancellationToken.None);

        Assert.That(diagnostic, Is.Not.Null);
        Assert.That(diagnostic!.Errors, Is.Empty,
            () => string.Join(Environment.NewLine, diagnostic.Errors.Select(error => error.Message)));

        var aggregate = LoadYaml(AggregateContractPath);
        var visitedReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        ResolveAllReferences(aggregate.Documents[0].RootNode, AggregateContractPath, visitedReferences);
        Assert.That(visitedReferences, Is.Not.Empty);
    }

    [Test]
    public void Aggregate_operation_ids_are_unique_and_session_operations_are_exposed()
    {
        var aggregate = LoadYaml(AggregateContractPath);
        var operationIds = GetAggregateOperations(aggregate)
            .Select(operation => RequiredScalar(operation.Value, "operationId"))
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(operationIds, Is.Unique);
            Assert.That(operationIds, Does.Contain("loginSession"));
            Assert.That(operationIds, Does.Contain("refreshSession"));
            Assert.That(operationIds, Does.Contain("logoutSession"));
            Assert.That(operationIds, Does.Contain("getCurrentSession"));
        });
    }

    [TestCase(typeof(SessionLoginRequest), "SessionLoginRequest")]
    [TestCase(typeof(SessionTokenResponse), "SessionTokenResponse")]
    [TestCase(typeof(SessionMeResponse), "SessionMeResponse")]
    public void Session_schema_matches_serializable_dto(Type dtoType, string schemaName)
    {
        var contract = LoadYaml(SessionContractPath);
        var schemas = RequiredMapping(
            RequiredMapping(RequiredMapping(contract.Documents[0].RootNode, "components"), "schemas"),
            schemaName);
        var properties = RequiredMapping(schemas, "properties");
        var required = RequiredSequence(schemas, "required")
            .Children
            .Cast<YamlScalarNode>()
            .Select(node => node.Value!)
            .ToHashSet(StringComparer.Ordinal);

        var dtoProperties = dtoType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var expectedNames = dtoProperties
            .Select(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name))
            .ToHashSet(StringComparer.Ordinal);

        Assert.Multiple(() =>
        {
            Assert.That(properties.Children.Keys.Cast<YamlScalarNode>().Select(node => node.Value),
                Is.EquivalentTo(expectedNames));
            Assert.That(required, Is.EquivalentTo(expectedNames));

            foreach (var property in dtoProperties)
            {
                var jsonName = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
                var propertySchema = RequiredMapping(properties, jsonName);
                Assert.That(RequiredScalar(propertySchema, "type"),
                    Is.EqualTo(ExpectedOpenApiType(property.PropertyType)),
                    $"Schema type mismatch for {dtoType.Name}.{property.Name}");
                Assert.That(propertySchema.Children.ContainsKey(new YamlScalarNode("nullable")), Is.False,
                    $"{dtoType.Name}.{property.Name} is non-nullable in the backend DTO");

                if (property.PropertyType == typeof(IReadOnlyList<string>))
                {
                    Assert.That(
                        RequiredScalar(RequiredMapping(propertySchema, "items"), "type"),
                        Is.EqualTo("string"));
                }
            }
        });
    }

    private static IEnumerable<KeyValuePair<string, YamlMappingNode>> GetAggregateOperations(YamlStream aggregate)
    {
        var paths = RequiredMapping(aggregate.Documents[0].RootNode, "paths");
        foreach (var pathEntry in paths.Children)
        {
            var path = ((YamlScalarNode)pathEntry.Key).Value!;
            var pathItem = (YamlMappingNode)pathEntry.Value;
            if (pathItem.Children.TryGetValue(new YamlScalarNode("$ref"), out var referenceNode))
            {
                pathItem = ResolveReference(
                    ((YamlScalarNode)referenceNode).Value!,
                    AggregateContractPath).Node;
            }

            foreach (var method in new[] { "get", "put", "post", "delete", "options", "head", "patch", "trace" })
            {
                if (pathItem.Children.TryGetValue(new YamlScalarNode(method), out var operation))
                {
                    yield return new KeyValuePair<string, YamlMappingNode>(
                        $"{method.ToUpperInvariant()} {path}",
                        (YamlMappingNode)operation);
                }
            }
        }
    }

    private static void ResolveAllReferences(YamlNode node, string sourcePath, ISet<string> visited)
    {
        if (node is YamlMappingNode mapping)
        {
            foreach (var child in mapping.Children)
            {
                if (((YamlScalarNode)child.Key).Value == "$ref")
                {
                    var reference = ((YamlScalarNode)child.Value).Value!;
                    var resolved = ResolveReference(reference, sourcePath);
                    var key = $"{resolved.SourcePath}#{reference.Split('#', 2).ElementAtOrDefault(1)}";
                    if (visited.Add(key))
                    {
                        ResolveAllReferences(resolved.Node, resolved.SourcePath, visited);
                    }
                }
                else
                {
                    ResolveAllReferences(child.Value, sourcePath, visited);
                }
            }
        }
        else if (node is YamlSequenceNode sequence)
        {
            foreach (var child in sequence.Children)
            {
                ResolveAllReferences(child, sourcePath, visited);
            }
        }
    }

    private static (YamlMappingNode Node, string SourcePath) ResolveReference(string reference, string sourcePath)
    {
        var parts = reference.Split('#', 2);
        var targetPath = string.IsNullOrEmpty(parts[0])
            ? sourcePath
            : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath)!, parts[0]));
        Assert.That(File.Exists(targetPath), Is.True, $"Referenced file does not exist: {targetPath}");

        YamlNode current = LoadYaml(targetPath).Documents[0].RootNode;
        if (parts.Length == 2 && !string.IsNullOrEmpty(parts[1]))
        {
            foreach (var encodedSegment in parts[1].TrimStart('/').Split('/'))
            {
                var segment = Uri.UnescapeDataString(encodedSegment)
                    .Replace("~1", "/", StringComparison.Ordinal)
                    .Replace("~0", "~", StringComparison.Ordinal);
                Assert.That(current, Is.TypeOf<YamlMappingNode>(), $"Invalid JSON pointer in {reference}");
                var currentMapping = (YamlMappingNode)current;
                Assert.That(currentMapping.Children.TryGetValue(new YamlScalarNode(segment), out current),
                    Is.True,
                    $"Unresolved JSON pointer segment '{segment}' in {reference}");
            }
        }

        Assert.That(current, Is.TypeOf<YamlMappingNode>(), $"Reference does not resolve to an object: {reference}");
        return ((YamlMappingNode)current!, targetPath);
    }

    private static string ExpectedOpenApiType(Type type)
        => type == typeof(string)
            ? "string"
            : type == typeof(int)
                ? "integer"
                : type == typeof(IReadOnlyList<string>)
                    ? "array"
                    : throw new AssertionException($"No OpenAPI type mapping for {type}");

    private static YamlStream LoadYaml(string path)
    {
        using var reader = File.OpenText(path);
        var yaml = new YamlStream();
        yaml.Load(reader);
        return yaml;
    }

    private static YamlMappingNode RequiredMapping(YamlNode parent, string key)
    {
        Assert.That(parent, Is.TypeOf<YamlMappingNode>());
        var mapping = (YamlMappingNode)parent;
        Assert.That(mapping.Children.TryGetValue(new YamlScalarNode(key), out var child), Is.True,
            $"Missing mapping '{key}'");
        Assert.That(child, Is.TypeOf<YamlMappingNode>(), $"'{key}' must be a mapping");
        return (YamlMappingNode)child!;
    }

    private static YamlSequenceNode RequiredSequence(YamlNode parent, string key)
    {
        Assert.That(parent, Is.TypeOf<YamlMappingNode>());
        var mapping = (YamlMappingNode)parent;
        Assert.That(mapping.Children.TryGetValue(new YamlScalarNode(key), out var child), Is.True,
            $"Missing sequence '{key}'");
        Assert.That(child, Is.TypeOf<YamlSequenceNode>(), $"'{key}' must be a sequence");
        return (YamlSequenceNode)child!;
    }

    private static string RequiredScalar(YamlNode parent, string key)
    {
        Assert.That(parent, Is.TypeOf<YamlMappingNode>());
        var mapping = (YamlMappingNode)parent;
        Assert.That(mapping.Children.TryGetValue(new YamlScalarNode(key), out var child), Is.True,
            $"Missing scalar '{key}'");
        Assert.That(child, Is.TypeOf<YamlScalarNode>(), $"'{key}' must be a scalar");
        return ((YamlScalarNode)child!).Value!;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate the BreviERP repository root.");
    }
}
