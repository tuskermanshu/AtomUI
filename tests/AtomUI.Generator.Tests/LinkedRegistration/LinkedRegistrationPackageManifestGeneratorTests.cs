extern alias LinkedPublish;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Shouldly;
using Xunit;
using LinkedRegistrationPackageManifestGenerator = LinkedPublish::AtomUI.Generator.LinkedRegistration.LinkedRegistrationPackageManifestGenerator;

namespace AtomUI.Generator.Tests.LinkedRegistration;

public sealed class LinkedRegistrationPackageManifestGeneratorTests
{
    [Fact]
    public void Package_Core_Attached_Property_Calls_Do_Not_Root_The_Control_Unit()
    {
        var result = Run(
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Avalonia.Controls { public class Control { } }
                namespace Acme.Controls
                {
                    public sealed class ToolTip : Avalonia.Controls.Control
                    {
                        public static object? GetTip(Avalonia.Controls.Control control) => null;
                        public static void SetIsOpen(Avalonia.Controls.Control control, bool value) { }
                    }
                }
                """),
            new TestSource(
                "/repo/PackageCore/ToolTipService.cs",
                """
                namespace Acme.Controls
                {
                    internal sealed class ToolTipService
                    {
                        public object? Read(Avalonia.Controls.Control control) => ToolTip.GetTip(control);
                        public void Open(Avalonia.Controls.Control control) => ToolTip.SetIsOpen(control, true);
                    }
                }
                """));

        result.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "ATOMUILINK002");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.RootUnit.v1");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
    }

    [Theory]
    [InlineData("public object Create() => new Acme.Controls.ToolTip();")]
    [InlineData("public System.Type GetTypeInfo() => typeof(Acme.Controls.ToolTip);")]
    public void Package_Core_Direct_Type_Evidence_Roots_The_Control_Unit(string evidence)
    {
        var result = Run(
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Avalonia.Controls { public class Control { } }
                namespace Acme.Controls { public class ToolTip : Avalonia.Controls.Control { } }
                """),
            new TestSource(
                "/repo/PackageCore/Bootstrap.cs",
                $$"""
                namespace Acme.Controls.PackageCore
                {
                    public class Bootstrap
                    {
                        {{evidence}}
                    }
                }
                """));

        result.GeneratedSource.ShouldContain("AtomUI.Linked.RootUnit.v1");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FTooltip");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
    }

    [Fact]
    public void Package_Core_Control_Returning_Factory_Call_Roots_The_Result_Unit()
    {
        var result = Run(
            new TestSource(
                "/repo/Alert/Alert.cs",
                """
                namespace Avalonia.Controls { public class Control { } }
                namespace Acme.Controls
                {
                    public sealed class Alert : Avalonia.Controls.Control
                    {
                        public static Alert Create() => new Alert();
                    }
                }
                """),
            new TestSource(
                "/repo/PackageCore/Bootstrap.cs",
                """
                namespace Acme.Controls.PackageCore
                {
                    public sealed class Bootstrap
                    {
                        public Acme.Controls.Alert Create() => Acme.Controls.Alert.Create();
                    }
                }
                """));

        result.GeneratedSource.ShouldContain("AtomUI.Linked.RootUnit.v1");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FAlert");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
    }

    [Fact]
    public void Package_Core_Generic_Value_Read_Does_Not_Become_A_Control_Factory_Root()
    {
        var result = Run(
            new TestSource(
                "/repo/Primitives/Control.cs",
                """
                namespace Avalonia.Controls
                {
                    public class Control
                    {
                        public T? GetValue<T>(object property) => default;
                    }
                }
                """),
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class ToolTip : Avalonia.Controls.Control
                    {
                        public static readonly object ToolTipProperty = new object();
                    }
                }
                """),
            new TestSource(
                "/repo/PackageCore/ToolTipService.cs",
                """
                namespace Acme.Controls.PackageCore
                {
                    public sealed class ToolTipService
                    {
                        public Acme.Controls.ToolTip? Read(Avalonia.Controls.Control control) =>
                            control.GetValue<Acme.Controls.ToolTip>(Acme.Controls.ToolTip.ToolTipProperty);
                    }
                }
                """));

        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.RootUnit.v1");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
    }

    [Fact]
    public void Cross_Unit_Call_Still_Emits_A_Direct_Call_Edge()
    {
        var result = Run(
            new TestSource(
                "/repo/Primitives/Control.cs",
                "namespace Avalonia.Controls { public class Control { } }"),
            new TestSource(
                "/repo/Alert/Alert.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class Alert : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Button/Button.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class Button : Avalonia.Controls.Control
                    {
                        public void Run() => Alert.Ping();
                    }
                }
                """));

        result.GeneratedSource.ShouldContain("AtomUI.Linked.UnitEdge.v1");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FButton");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FAlert");
        result.GeneratedSource.ShouldContain("CSharpCall");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
    }

    [Fact]
    public void Interface_Dispatch_Is_Not_A_Registration_Dependency_Or_Fallback()
    {
        var result = Run(
            new TestSource(
                "/repo/Primitives/Contracts.cs",
                """
                namespace Avalonia.Controls { public class Control { } }
                namespace Acme.Controls.Primitives
                {
                    public interface IPart { void Run(); }
                    public sealed class PartControl : Avalonia.Controls.Control, IPart
                    {
                        public void Run() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Button/Button.cs",
                """
                namespace Acme.Controls.Buttons
                {
                    public sealed class Button : Avalonia.Controls.Control
                    {
                        public void Run(Acme.Controls.Primitives.IPart part) => part.Run();
                    }
                }
                """));

        result.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "ATOMUILINK002");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
        var interfaceCallEdges = result.GeneratedSource.Split('\n').Where(static line =>
            line.Contains("AtomUI.Linked.UnitEdge.v1", StringComparison.Ordinal) &&
            line.Contains("CSharpCall", StringComparison.Ordinal)).ToArray();
        interfaceCallEdges.ShouldBeEmpty(string.Join(Environment.NewLine, interfaceCallEdges));
    }

    [Fact]
    public void Cross_Unit_Delegate_Dispatch_Remains_A_Package_Fallback()
    {
        var result = Run(
            new TestSource(
                "/repo/Primitives/Contracts.cs",
                """
                namespace Avalonia.Controls { public class Control { } }
                namespace Acme.Controls.Primitives
                {
                    public delegate void PartAction();
                    public sealed class PartControl : Avalonia.Controls.Control { }
                }
                """),
            new TestSource(
                "/repo/Button/Button.cs",
                """
                namespace Acme.Controls.Buttons
                {
                    public sealed class Button : Avalonia.Controls.Control
                    {
                        public void Run(Acme.Controls.Primitives.PartAction action) => action();
                    }
                }
                """));

        result.Diagnostics.ShouldContain(diagnostic => diagnostic.Id == "ATOMUILINK002");
        result.GeneratedSource.ShouldContain("AtomUI.Linked.Fallback.v1");
    }

    private const string AotTrimUnitApiSource = """
        namespace AtomUI.Registration
        {
            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class AotTrimUnitAttribute : System.Attribute
            {
                public AotTrimUnitAttribute(string unitName)
                {
                    UnitName = unitName;
                }

                public string UnitName { get; }
            }

            public static class AotTrimGeneralUnits
            {
                public const string Core = "Core";
            }
        }
        """;

    private static TestSource AotTrimUnitApi =>
        new("/lib/AotTrimUnitAttribute.cs", AotTrimUnitApiSource);

    [Fact]
    public void AotTrimUnit_Annotated_Service_In_Control_Family_Directory_Does_Not_Attribute_Evidence_To_The_Enclosing_Unit()
    {
        var result = Run(
            AotTrimUnitApi,
            new TestSource(
                "/repo/Primitives/Control.cs",
                "namespace Avalonia.Controls { public class Control { } }"),
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class ToolTip : Avalonia.Controls.Control
                    {
                        public static object? GetTip(Avalonia.Controls.Control control) => null;
                        public static void SetIsOpen(Avalonia.Controls.Control control, bool value) { }
                    }
                }
                """),
            new TestSource(
                "/repo/Alert/Alert.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class Alert : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Tooltip/ToolTipService.cs",
                """
                using AtomUI.Registration;
                namespace Acme.Controls
                {
                    [AotTrimUnit(AotTrimGeneralUnits.Core)]
                    internal sealed class ToolTipService
                    {
                        public object? Read(Avalonia.Controls.Control control) => ToolTip.GetTip(control);
                        public void Open(Avalonia.Controls.Control control) => ToolTip.SetIsOpen(control, true);
                        public void Touch() => Alert.Ping();
                    }
                }
                """));

        result.Diagnostics.ShouldNotContain(diagnostic => diagnostic.Id == "ATOMUILINK002");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.RootUnit.v1");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
        result.GeneratedSource.ShouldNotContain("%2FAlert|CSharp");
    }

    [Theory]
    [InlineData("public object Create() => new Acme.Controls.ToolTip();")]
    [InlineData("public System.Type GetTypeInfo() => typeof(Acme.Controls.ToolTip);")]
    public void AotTrimUnit_Annotated_Direct_Type_Evidence_Roots_The_Control_Unit(string evidence)
    {
        var result = Run(
            AotTrimUnitApi,
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Avalonia.Controls { public class Control { } }
                namespace Acme.Controls { public class ToolTip : Avalonia.Controls.Control { } }
                """),
            new TestSource(
                "/repo/Tooltip/Bootstrap.cs",
                $$"""
                using AtomUI.Registration;
                namespace Acme.Controls
                {
                    [AotTrimUnit(AotTrimGeneralUnits.Core)]
                    public class Bootstrap
                    {
                        {{evidence}}
                    }
                }
                """));

        result.GeneratedSource.ShouldContain("AtomUI.Linked.RootUnit.v1");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FTooltip");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
    }

    [Fact]
    public void AotTrimUnit_Annotated_Control_Factory_Call_Roots_The_Result_Unit()
    {
        var result = Run(
            AotTrimUnitApi,
            new TestSource(
                "/repo/Primitives/Control.cs",
                "namespace Avalonia.Controls { public class Control { } }"),
            new TestSource(
                "/repo/Alert/Alert.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class Alert : Avalonia.Controls.Control
                    {
                        public static Alert Create() => new Alert();
                    }
                }
                """),
            new TestSource(
                "/repo/Dialog/Dialog.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class Dialog : Avalonia.Controls.Control { }
                }
                """),
            new TestSource(
                "/repo/Dialog/Bootstrap.cs",
                """
                using AtomUI.Registration;
                namespace Acme.Controls
                {
                    [AotTrimUnit(AotTrimGeneralUnits.Core)]
                    public sealed class Bootstrap
                    {
                        public Acme.Controls.Alert Create() => Acme.Controls.Alert.Create();
                    }
                }
                """));

        result.GeneratedSource.ShouldContain("AtomUI.Linked.RootUnit.v1");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FAlert");
        result.GeneratedSource.ShouldNotContain("%2FDialog|CSharp");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
    }

    [Fact]
    public void AotTrimUnit_Annotated_File_Does_Not_Attribute_Evidence_To_The_Enclosing_Unit()
    {
        var result = Run(
            AotTrimUnitApi,
            new TestSource(
                "/repo/Primitives/Control.cs",
                "namespace Avalonia.Controls { public class Control { } }"),
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class ToolTip : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Alert/Alert.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class Alert : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Dialog/Dialog.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class Dialog : Avalonia.Controls.Control
                    {
                        public void Run() => Alert.Ping();
                    }
                }
                """),
            new TestSource(
                "/repo/Dialog/DialogService.cs",
                """
                using AtomUI.Registration;
                namespace Acme.Controls
                {
                    [AotTrimUnit(AotTrimGeneralUnits.Core)]
                    internal static class DialogService
                    {
                        public static void Run() => ToolTip.Ping();
                    }
                }
                """));

        result.GeneratedSource.ShouldContain("AtomUI.Linked.UnitEdge.v1");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FDialog");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FAlert");
        result.GeneratedSource.ShouldNotContain("%2FTooltip|CSharp");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.RootUnit.v1");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
    }

    [Fact]
    public void AotTrimUnit_Can_Assign_A_Type_To_A_Known_Control_Unit()
    {
        var result = Run(
            AotTrimUnitApi,
            new TestSource(
                "/repo/Primitives/Control.cs",
                "namespace Avalonia.Controls { public class Control { } }"),
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class ToolTip : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Alert/Alert.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class Alert : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Tooltip/AlertHelper.cs",
                """
                namespace Acme.Controls
                {
                    [AtomUI.Registration.AotTrimUnit("Alert")]
                    internal static class AlertHelper
                    {
                        public static void Run() => ToolTip.Ping();
                    }
                }
                """));

        result.GeneratedSource.ShouldContain("AtomUI.Linked.UnitEdge.v1");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FAlert");
        result.GeneratedSource.ShouldContain("Acme.Controls%2FTooltip");
        result.GeneratedSource.ShouldContain("CSharpCall");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.RootUnit.v1");
        result.GeneratedSource.ShouldNotContain("AtomUI.Linked.Fallback.v1");
    }

    [Fact]
    public void AotTrimUnit_Conflicting_Compile_Metadata_Reports_ATOMUILINK011_And_Attribute_Wins()
    {
        var result = Run(
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
            {
                ["/repo/Tooltip/ToolTipService.cs"] = new Dictionary<string, string>
                {
                    ["build_metadata.Compile.AtomUIRegistrationUnit"] = "Alert"
                }
            },
            strict: false,
            AotTrimUnitApi,
            new TestSource(
                "/repo/Primitives/Control.cs",
                "namespace Avalonia.Controls { public class Control { } }"),
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class ToolTip : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Alert/Alert.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class Alert : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Tooltip/ToolTipService.cs",
                """
                using AtomUI.Registration;
                namespace Acme.Controls
                {
                    [AotTrimUnit(AotTrimGeneralUnits.Core)]
                    internal static class ToolTipService
                    {
                        public static void Run() => ToolTip.Ping();
                    }
                }
                """));

        result.Diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUILINK011" &&
            diagnostic.Severity == DiagnosticSeverity.Warning);
        result.GeneratedSource.ShouldNotContain("%2FTooltip|CSharp");
    }

    [Fact]
    public void AotTrimUnit_Conflict_Is_Error_In_Strict_Mode()
    {
        var result = Run(
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
            {
                ["/repo/Tooltip/ToolTipService.cs"] = new Dictionary<string, string>
                {
                    ["build_metadata.Compile.AtomUIRegistrationUnit"] = "Alert"
                }
            },
            strict: true,
            AotTrimUnitApi,
            new TestSource(
                "/repo/Primitives/Control.cs",
                "namespace Avalonia.Controls { public class Control { } }"),
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class ToolTip : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Tooltip/ToolTipService.cs",
                """
                using AtomUI.Registration;
                namespace Acme.Controls
                {
                    [AotTrimUnit(AotTrimGeneralUnits.Core)]
                    internal static class ToolTipService
                    {
                        public static void Run() => ToolTip.Ping();
                    }
                }
                """));

        result.Diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUILINK011" &&
            diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Multiple_AotTrimUnit_Values_In_One_File_Report_ATOMUILINK012()
    {
        var result = Run(
            AotTrimUnitApi,
            new TestSource(
                "/repo/Primitives/Control.cs",
                "namespace Avalonia.Controls { public class Control { } }"),
            new TestSource(
                "/repo/Tooltip/ToolTip.cs",
                """
                namespace Acme.Controls
                {
                    public sealed class ToolTip : Avalonia.Controls.Control
                    {
                        public static void Ping() { }
                    }
                }
                """),
            new TestSource(
                "/repo/Shared/Bootstrap.cs",
                """
                namespace Acme.Controls
                {
                    [AtomUI.Registration.AotTrimUnit("Core")]
                    internal static class FirstHelper { }

                    [AtomUI.Registration.AotTrimUnit("Other")]
                    internal static class SecondHelper { }
                }
                """));

        result.Diagnostics.ShouldContain(diagnostic =>
            diagnostic.Id == "ATOMUILINK012" &&
            diagnostic.Severity == DiagnosticSeverity.Warning);
    }

    private static PackageManifestGeneratorExecution Run(params TestSource[] sources)
    {
        return Run(null, false, sources);
    }

    private static PackageManifestGeneratorExecution Run(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? perTreeOptions,
        bool strict,
        params TestSource[] sources)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "Acme.Controls",
            sources.Select(source => CSharpSyntaxTree.ParseText(
                source.Content,
                parseOptions,
                source.Path)),
            UsageGeneratorTestHost.PlatformReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var globalValues = new Dictionary<string, string>
        {
            ["build_property.AtomUIRegistrationPackageId"] = "Acme.Controls",
            ["build_property.AtomUIRegistrationGranularity"] = "Directory",
            ["build_property.AtomUILinkedPublish"] = "true",
            ["build_property.ProjectDir"] = "/repo/"
        };
        if (strict)
        {
            globalValues["build_property.AtomUIRegistrationStrict"] = "true";
        }
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new LinkedRegistrationPackageManifestGenerator().AsSourceGenerator()],
            parseOptions: parseOptions,
            optionsProvider: new PackageOptionsProvider(globalValues, perTreeOptions));
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);
        var runResult = driver.GetRunResult().Results.ShouldHaveSingleItem();
        return new PackageManifestGeneratorExecution(
            runResult.GeneratedSources.ShouldHaveSingleItem().SourceText.ToString(),
            runResult.Diagnostics);
    }

    private sealed class PackageOptionsProvider(
        IReadOnlyDictionary<string, string> globalValues,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? perTreeValues)
        : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _globalOptions = new PackageOptions(globalValues);
        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            if (perTreeValues is not null &&
                perTreeValues.TryGetValue(tree.FilePath, out var treeValues))
            {
                var merged = new Dictionary<string, string>(globalValues, StringComparer.Ordinal);
                foreach (var pair in treeValues)
                {
                    merged[pair.Key] = pair.Value;
                }
                return new PackageOptions(merged);
            }
            return _globalOptions;
        }
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _globalOptions;
    }

    private sealed class PackageOptions(IReadOnlyDictionary<string, string> values)
        : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value) =>
            values.TryGetValue(key, out value!);
    }

    private sealed record PackageManifestGeneratorExecution(
        string GeneratedSource,
        ImmutableArray<Diagnostic> Diagnostics);
}
