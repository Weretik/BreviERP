using System.Reflection;
using System.Text.RegularExpressions;
using Ardalis.Result;
using BuildingBlocks.Application.Errors;
using BuildingBlocks.Domain.Errors;

namespace UnitTests.Platform.Errors;

/// <summary>The one error type (module rules «Errors»): typed, Ukrainian, mapped to a result in one place.</summary>
public sealed class ErrorTests
{
    private static readonly Regex Cyrillic = new("[А-Яа-яІіЇїЄєҐґ]", RegexOptions.Compiled);

    [TestCase(ErrorType.Validation, ResultStatus.Invalid)]
    [TestCase(ErrorType.NotFound, ResultStatus.NotFound)]
    [TestCase(ErrorType.Conflict, ResultStatus.Conflict)]
    [TestCase(ErrorType.Forbidden, ResultStatus.Forbidden)]
    [TestCase(ErrorType.Unexpected, ResultStatus.Error)]
    public void Each_error_type_becomes_its_result_status(ErrorType type, ResultStatus status)
    {
        var error = new Error("Test.Code", "Повідомлення.", type);

        Assert.Multiple(() =>
        {
            Assert.That(error.ToResult().Status, Is.EqualTo(status));
            Assert.That(error.ToResult<int>().Status, Is.EqualTo(status));
        });
    }

    [Test]
    public void Validation_error_keeps_its_code_as_the_field_key()
    {
        var result = Error.Validation("Catalog.Product.Name.Required", "Вкажіть назву товару.").ToResult();

        var error = result.ValidationErrors.Single();
        Assert.That((error.Identifier, error.ErrorMessage),
            Is.EqualTo(("Catalog.Product.Name.Required", "Вкажіть назву товару.")));
    }

    [Test]
    public void Every_module_error_is_ukrainian_and_has_a_module_code()
    {
        var wrong = new List<string>();
        foreach (var method in ErrorFactories())
        {
            var error = (Error)method.Invoke(null, method.GetParameters().Select(Sample).ToArray())!;
            var module = method.DeclaringType!.Namespace!.Split('.')[0];
            if (!Cyrillic.IsMatch(error.Message) || !error.Code.StartsWith(module + ".", StringComparison.Ordinal))
                wrong.Add($"{method.DeclaringType.Name}.{method.Name}: {error.Code} «{error.Message}»");
        }

        TestContext.Out.WriteLine(string.Join(Environment.NewLine, wrong));
        Assert.That(wrong, Is.Empty);
    }

    private static IEnumerable<MethodInfo> ErrorFactories() =>
        new[] { typeof(global::Catalog.Domain.Products.Errors.ProductErrors).Assembly, typeof(global::Reference.Domain.GarmentAccessories.Entities.Fabric).Assembly }
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.Name.EndsWith("Errors", StringComparison.Ordinal) && type.IsAbstract && type.IsSealed)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(method => method.ReturnType == typeof(Error));

    private static object? Sample(ParameterInfo parameter) => parameter.ParameterType switch
    {
        var t when t == typeof(string) => parameter.Name == "message" ? "Повідомлення." : "Quantity",
        var t when t == typeof(int) => 1,
        var t when t == typeof(long) => 1L,
        var t when t == typeof(decimal) => 1m,
        var t when t == typeof(Guid) => Guid.Empty,
        var t when t.IsValueType => Activator.CreateInstance(t),
        _ => null
    };
}
