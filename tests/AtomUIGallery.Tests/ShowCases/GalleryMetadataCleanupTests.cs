using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class GalleryMetadataCleanupTests
{
    [Fact]
    public void ShowCases_Do_Not_Keep_Api_Or_DesignToken_Metadata_Sidecars()
    {
        var showCasesRoot = Path.Combine(GetRepoRoot(), "controlgallery/AtomUIGallery/ShowCases");

        Directory.GetFiles(showCasesRoot, "*ApiDataGrid.axaml", SearchOption.AllDirectories)
                 .Select(ToRepoRelativePath)
                 .Order(StringComparer.Ordinal)
                 .ShouldBeEmpty();
        Directory.GetFiles(showCasesRoot, "*ApiDataGrid.axaml.cs", SearchOption.AllDirectories)
                 .Select(ToRepoRelativePath)
                 .Order(StringComparer.Ordinal)
                 .ShouldBeEmpty();
        Directory.GetFiles(showCasesRoot, "*DesignTokenDataGrid.axaml", SearchOption.AllDirectories)
                 .Select(ToRepoRelativePath)
                 .Order(StringComparer.Ordinal)
                 .ShouldBeEmpty();
        Directory.GetFiles(showCasesRoot, "*DesignTokenDataGrid.axaml.cs", SearchOption.AllDirectories)
                 .Select(ToRepoRelativePath)
                 .Order(StringComparer.Ordinal)
                 .ShouldBeEmpty();

        var viewModelLeaks = Directory.GetFiles(showCasesRoot, "*ViewModel.cs", SearchOption.AllDirectories)
                                      .Where(ContainsApiOrTokenRowMetadata)
                                      .Select(ToRepoRelativePath)
                                      .Order(StringComparer.Ordinal)
                                      .ToArray();
        viewModelLeaks.ShouldBeEmpty();

        var localizationLeaks = Directory.GetFiles(showCasesRoot, "*.cs", SearchOption.AllDirectories)
                                         .Where(path => path.Contains($"{Path.DirectorySeparatorChar}Localization{Path.DirectorySeparatorChar}",
                                                                      StringComparison.Ordinal))
                                         .Where(ContainsApiOrTokenLocalizationMetadata)
                                         .Select(ToRepoRelativePath)
                                         .Order(StringComparer.Ordinal)
                                         .ToArray();
        localizationLeaks.ShouldBeEmpty();
    }

    private static bool ContainsApiOrTokenRowMetadata(string path)
    {
        var source = File.ReadAllText(path);
        return source.Contains("ApiRows", StringComparison.Ordinal) ||
               source.Contains("DesignTokenRows", StringComparison.Ordinal) ||
               source.Contains("EnsureApiRows", StringComparison.Ordinal) ||
               source.Contains("EnsureDesignTokenRows", StringComparison.Ordinal) ||
               source.Contains("ApiRow", StringComparison.Ordinal) ||
               source.Contains("DesignTokenRow", StringComparison.Ordinal);
    }

    private static bool ContainsApiOrTokenLocalizationMetadata(string path)
    {
        var source = File.ReadAllText(path);
        return source.Contains("public const string ScenarioApi", StringComparison.Ordinal) ||
               source.Contains("public const string ScenarioDesignToken", StringComparison.Ordinal) ||
               source.Contains("public const string ApiTitle", StringComparison.Ordinal) ||
               source.Contains("public const string ApiDescription", StringComparison.Ordinal) ||
               source.Contains("public const string ApiColumn", StringComparison.Ordinal) ||
               source.Contains("public const string ApiProperty", StringComparison.Ordinal) ||
               source.Contains("public const string TokenColumn", StringComparison.Ordinal) ||
               source.Contains("public const string TokenName", StringComparison.Ordinal) ||
               source.Contains("public const string TokenScope", StringComparison.Ordinal) ||
               source.Contains("public const string TokenStatus", StringComparison.Ordinal);
    }

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate AtomUI repository root.");
    }

    private static string ToRepoRelativePath(string path)
    {
        return Path.GetRelativePath(GetRepoRoot(), path).Replace(Path.DirectorySeparatorChar, '/');
    }
}
