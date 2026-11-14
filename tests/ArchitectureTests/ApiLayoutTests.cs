namespace ArchitectureTests;

/// <summary>
/// Api projects keep HTTP code in <c>Endpoints/&lt;Feature&gt;/&lt;UseCase&gt;/</c> (module rules «Folders»), next to
/// <c>Access/</c> with policy names, <c>DependencyInjection/</c> with the registration and an optional <c>Middleware/</c>.
/// No folder is named after a kind of class, and there are no MVC controllers.
/// </summary>
public sealed class ApiLayoutTests
{
    private static readonly string[] KindFolders =
        ["Controllers", "Contracts", "Requests", "Responses", "Mapping", "Mappers", "Models", "Dtos", "DTOs", "Options", "Results", "Filters", "Services", "Helpers", "Extensions"];

    [Test]
    public void Http_endpoints_are_minimal_api_never_mvc_controllers()
    {
        var offending = Directory.GetFiles(Path.Combine(Root(), "src"), "*.cs", SearchOption.AllDirectories)
            .Where(NotBuildOutput)
            .Where(file =>
            {
                var text = File.ReadAllText(file);
                return text.Contains("ControllerBase") || text.Contains("[ApiController]")
                       || text.Contains("AddControllers(") || text.Contains("MapControllers(");
            })
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Api_projects_have_no_folders_named_after_kinds_of_classes()
    {
        var offending = ApiProjects()
            .SelectMany(project => Directory.GetDirectories(project, "*", SearchOption.AllDirectories))
            .Where(NotBuildOutput)
            .Where(dir => KindFolders.Contains(Path.GetFileName(dir)))
            .Where(dir => Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories).Any())
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Api_projects_keep_code_in_folders()
    {
        var offending = ApiProjects()
            .SelectMany(project => Directory.GetFiles(project, "*.cs"))
            .Where(file => !Path.GetFileName(file).EndsWith("ApiAssemblyMarker.cs", StringComparison.Ordinal) && Path.GetFileName(file) != "GlobalUsing.cs")
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    /// <summary>
    /// An Api project holds only <c>Access/</c> (who may do what), <c>DependencyInjection/</c> (registration and
    /// <c>Map&lt;Module&gt;Endpoints</c>), <c>Endpoints/</c> and, when the module has one, <c>Middleware/</c>.
    /// </summary>
    [Test]
    public void Api_projects_have_only_access_registration_endpoints_and_middleware_folders()
    {
        string[] allowed = ["Access", "DependencyInjection", "Endpoints", "Middleware"];
        var offending = ApiProjects()
            .SelectMany(project => Directory.GetDirectories(project))
            .Where(NotBuildOutput)
            .Where(dir => !allowed.Contains(Path.GetFileName(dir)))
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    /// <summary>
    /// One use case = one folder: <c>Endpoints/&lt;Feature&gt;/</c> holds only its route groups (<c>*Endpoints.cs</c>, usually
    /// one) and folders. A use-case folder <c>&lt;UseCase&gt;/</c> holds <c>&lt;UseCase&gt;Endpoint.cs</c> with the request and
    /// response types only it uses; types several use cases share sit in a folder with no endpoint (<c>Shared/</c> or a
    /// named group such as <c>Detail/</c>), and types several features share in <c>Endpoints/Shared/</c>.
    /// </summary>
    [Test]
    public void Endpoints_have_one_folder_per_use_case()
    {
        var offending = new List<string>();
        foreach (var endpoints in ApiProjects().Select(project => Path.Combine(project, "Endpoints")).Where(Directory.Exists))
        {
            offending.AddRange(Directory.GetFiles(endpoints, "*.cs"));
            foreach (var feature in Directory.GetDirectories(endpoints))
            {
                var name = Path.GetFileName(feature);
                offending.AddRange(Directory.GetFiles(feature, "*.cs")
                    .Where(file => name == "Shared" ? file.EndsWith("Endpoint.cs", StringComparison.Ordinal) : !file.EndsWith("Endpoints.cs", StringComparison.Ordinal)));
                if (name == "Shared")
                    continue;
                foreach (var folder in Directory.GetDirectories(feature))
                {
                    var endpointFiles = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories)
                        .Where(file => file.EndsWith("Endpoint.cs", StringComparison.Ordinal) || file.EndsWith("Endpoints.cs", StringComparison.Ordinal))
                        .ToArray();
                    var useCase = Path.GetFileName(folder) + "Endpoint.cs";
                    var isUseCase = endpointFiles.Length == 1 && Path.GetFileName(endpointFiles[0]) == useCase
                                                              && Path.GetDirectoryName(endpointFiles[0]) == folder
                                                              && Directory.GetDirectories(folder).Length == 0;
                    if (endpointFiles.Length > 0 && !isUseCase)
                        offending.Add(folder);
                }
            }
        }

        Assert.That(offending.Select(Relative), Is.Empty);
    }

    private static IEnumerable<string> ApiProjects() =>
        Directory.GetFiles(Path.Combine(Root(), "src", "Modules"), "*.Api.csproj", SearchOption.AllDirectories)
            .Select(project => Path.GetDirectoryName(project)!);

    private static bool NotBuildOutput(string path) =>
        !path.Contains($"{Path.DirectorySeparatorChar}bin") && !path.Contains($"{Path.DirectorySeparatorChar}obj");

    private static string Relative(string path) => Path.GetRelativePath(Root(), path);

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}
