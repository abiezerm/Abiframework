using System.Collections.Immutable;
using AbiFramework.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AbiFramework.Tests.Entities;

public class UnionCompilerTests
{
    private const string NonExhaustiveSwitchDiagnosticId = "CS8509";

    [Fact]
    public void Switch_on_result_with_missing_domain_error_arm_reports_non_exhaustiveness()
    {
        const string source = """
            using System;
            using AbiFramework.Entities;

            public static class Example
            {
                public static string Describe(Result<Guid> result) => result switch
                {
                    Guid value => value.ToString()
                };
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = Compile(source);

        diagnostics.Should().Contain(diagnostic => diagnostic.Id == NonExhaustiveSwitchDiagnosticId);
    }

    [Fact]
    public void Switch_on_result_with_both_arms_has_no_non_exhaustiveness_diagnostic()
    {
        const string source = """
            using System;
            using AbiFramework.Entities;

            public static class Example
            {
                public static string Describe(Result<Guid> result) => result switch
                {
                    Guid value => value.ToString(),
                    DomainError error => error.Code
                };
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = Compile(source);

        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == NonExhaustiveSwitchDiagnosticId);
        diagnostics.Should().NotContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Nullable_result_type_argument_reports_nullability_constraint_diagnostic()
    {
        const string source = """
            #nullable enable
            using AbiFramework.Entities;

            public static class Example
            {
                public static Result<string?> Create() => default;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = Compile(source);

        diagnostics.Should().Contain(diagnostic => diagnostic.Id == "CS8714");
    }

    private static ImmutableArray<Diagnostic> Compile(string source)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Preview);
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        IEnumerable<MetadataReference> references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(Result<>).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: $"CompilerTest_{Guid.NewGuid():N}",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        return compilation.GetDiagnostics();
    }
}
