using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using Host.Api.DependencyInjection.ServiceRegistration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Platform.Validation;

/// <summary>
/// Server validation is mandatory (module rules «Validation»): every command and query that takes client input has a
/// validator registered in the API container, and every message a validator can show is Ukrainian, without a field name.
/// Needs no database.
/// </summary>
public sealed class ValidationCoverageTests
{
    private static readonly Regex Cyrillic = new("[А-Яа-яІіЇїЄєҐґ]", RegexOptions.Compiled);

    [Test]
    public void Every_command_and_query_with_input_has_a_validator()
    {
        using var services = Services();
        var missing = Requests()
            .Where(TakesInput)
            .Where(type => services.GetService(typeof(IValidator<>).MakeGenericType(type)) is null)
            .Select(type => type.FullName)
            .Order()
            .ToArray();

        TestContext.Out.WriteLine(string.Join(Environment.NewLine, missing));
        Assert.That(missing, Is.Empty);
    }

    [Test]
    public void Validator_messages_are_ukrainian_and_name_no_field()
    {
        using var services = Services();
        var wrong = new List<string>();
        foreach (var type in Requests())
        {
            if (services.GetService(typeof(IValidator<>).MakeGenericType(type)) is not IValidator validator) continue;
            foreach (var message in Messages(validator.CreateDescriptor()))
            {
                if (!Cyrillic.IsMatch(message) || message.Contains("{PropertyName}", StringComparison.Ordinal))
                    wrong.Add($"{type.Name}: {message}");
            }
        }

        TestContext.Out.WriteLine(string.Join(Environment.NewLine, wrong.Distinct()));
        Assert.That(wrong, Is.Empty);
    }

    [Test]
    public void Built_in_messages_are_ukrainian_whatever_the_request_culture()
    {
        using var services = Services();
        var previous = Thread.CurrentThread.CurrentUICulture;
        try
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-RU");
            var validator = new InlineValidator<Sample>();
            validator.RuleFor(x => x.FabricName).NotEmpty();
            var message = validator.Validate(new Sample(null)).Errors.Single().ErrorMessage;
            Assert.That(message, Is.EqualTo("Заповніть це поле."));
        }
        finally
        {
            Thread.CurrentThread.CurrentUICulture = previous;
        }
    }

    public sealed record Sample(string? FabricName);

    private static IEnumerable<string> Messages(IValidatorDescriptor descriptor)
    {
        foreach (var rule in descriptor.Rules)
        foreach (var component in rule.Components)
        {
            // Child validators and Custom() rules have no message of their own; a child validator is checked below.
            var message = component.GetUnformattedErrorMessage();
            if (message != "No default error message has been specified") yield return message;
            if (component.Validator is IChildValidatorAdaptor child && child.ValidatorType is { } childType &&
                childType.GetConstructor(Type.EmptyTypes) is not null &&
                Activator.CreateInstance(childType) is IValidator inner)
            {
                foreach (var innerMessage in Messages(inner.CreateDescriptor())) yield return innerMessage;
            }
        }
    }

    /// <summary>Client input: anything but flags and the signed-in actor that the endpoint fills in.</summary>
    private static bool TakesInput(Type request) =>
        request.GetConstructors().SelectMany(c => c.GetParameters()).Any(p =>
            p.ParameterType != typeof(bool) && p.ParameterType != typeof(bool?) && !p.ParameterType.Name.EndsWith("Actor", StringComparison.Ordinal));

    private static Type[] Requests() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name is { } name && name.EndsWith(".Application", StringComparison.Ordinal) && !name.StartsWith("BuildingBlocks", StringComparison.Ordinal))
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false } && t.GetInterfaces().Any(i =>
                i.Namespace == "Mediator" && i.Name is "ICommand" or "ICommand`1" or "IQuery`1"))
            .ToArray();

    private static ServiceProvider Services()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=brevierp_validation;Username=none;Password=none"
        });
        builder.Services.AddHostServices(builder.Configuration);
        return builder.Services.BuildServiceProvider();
    }
}
