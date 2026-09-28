using Avalonia;
using Avalonia.Headless.XUnit;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class TypedPackageEntryTests
{
    [AvaloniaTheory]
    [InlineData("DataGrid", true)]
    [InlineData("ColorPicker", true)]
    [InlineData("Extras", false)]
    public void Optional_entry_stages_only_its_package_and_rejects_duplicate_before_completion(string name, bool hasLanguage)
    {
        var builder = new AtomUIBuilder(new Application());
        Register(builder, name);
        var package = builder.ThemeManagerBuilder.ControlPackages.ShouldHaveSingleItem();
        package.Id.ShouldBe("AtomUI.Desktop.Controls." + name);
        package.ControlThemesProvider.Id.ShouldBe(package.Id);
        var catalogs = builder.LocalizationBuilder.Catalogs.Count;
        (catalogs > 0).ShouldBe(hasLanguage);
        builder.ThemeManagerBuilder.InitializerCount.ShouldBe(0);
        Should.Throw<InvalidOperationException>(() => Register(builder, name));
        builder.LocalizationBuilder.Catalogs.Count.ShouldBe(catalogs);
        builder.ThemeManagerBuilder.ControlPackages.ShouldHaveSingleItem().ShouldBeSameAs(package);
    }

    [AvaloniaFact]
    public void Common_dependency_runs_services_and_localization_once_before_desktop_commit()
    {
        var builder = new AtomUIBuilder(new Application());
        AtomUI.Controls.ThemeManagerBuilderExtensions.UseCommonControls(builder);
        var services = builder.OwnedServiceRegistrations.Count;
        var catalogs = builder.LocalizationBuilder.Catalogs.Count;
        services.ShouldBeGreaterThan(0);
        catalogs.ShouldBeGreaterThan(0);
        AtomUI.Controls.ThemeManagerBuilderExtensions.UseCommonControls(builder);
        builder.OwnedServiceRegistrations.Count.ShouldBe(services);
        builder.LocalizationBuilder.Catalogs.Count.ShouldBe(catalogs);
        builder.UseDesktopControls();
        builder.ThemeManagerBuilder.ControlPackages.Select(p => p.Id).ShouldBe(new[] { "AtomUI.Controls.Common", "AtomUI.Desktop.Controls" });
        builder.OwnedServiceRegistrations.Count.ShouldBe(services);
        builder.ThemeManagerBuilder.InitializerCount.ShouldBe(1);
        var desktopCatalogs = builder.LocalizationBuilder.Catalogs.Count;
        Should.Throw<InvalidOperationException>(() => builder.UseDesktopControls());
        builder.LocalizationBuilder.Catalogs.Count.ShouldBe(desktopCatalogs);
        builder.ThemeManagerBuilder.InitializerCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public void All_optional_packages_initialize_and_freeze_with_desktop_enabled_explicitly()
    {
        var app = new Application();
        IAtomUIBuilder? saved = null;
        app.UseAtomUI(builder =>
        {
            saved = builder;
            builder.UseDesktopControls().UseDesktopDataGrid().UseDesktopColorPicker().UseDesktopExtras();
        });
        app.GetThemeManager().ShouldNotBeNull().CurrentTheme.ShouldNotBeNull();
        Should.Throw<InvalidOperationException>(() => saved!.UseDesktopDataGrid()).Message.ShouldContain("frozen");
    }

    private static void Register(IAtomUIBuilder builder, string name)
    {
        switch (name)
        {
            case "DataGrid": builder.UseDesktopDataGrid(); break;
            case "ColorPicker": builder.UseDesktopColorPicker(); break;
            case "Extras": builder.UseDesktopExtras(); break;
            default: throw new ArgumentException(name);
        }
    }
}
