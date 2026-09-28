using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
#if COMPLEX
using Fixture.ThirdParty;
using Fixture.ThirdParty.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
#endif
using TextBlock = Avalonia.Controls.TextBlock;
using Button = AtomUI.Desktop.Controls.Button;
using Expander = AtomUI.Desktop.Controls.Expander;
using NumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using ButtonSpinner = AtomUI.Desktop.Controls.ButtonSpinner;

namespace AtomUI.Registration.Fixtures;

public class ProductApp : Application
{
    private Button _button = null!;
    private IReadOnlyList<ControlPackageRegistration> _packages = null!;
    private IAtomUIBuilder _builder = null!;
#if COMPLEX
    private Expander _expander = null!;
    private IncludeControl _include = null!;
    private NumericUpDown _numeric = null!;
    private ThemeConfigProvider _scope = null!;
#endif
    public override void Initialize()
    {
        this.UseAtomUI(builder =>
        {
            _builder = builder;
            builder.UseDesktopControls();
#if COMPLEX
            GC.KeepAlive(new TokenOnlyToken());
            GC.KeepAlive(IdentityOnlyTokens.Identity);
            GC.KeepAlive(EnumOnlyTokenKind.FixtureValue);
            GC.KeepAlive(TokenKeyOnlyTokenKey.FixtureValue);
            GC.KeepAlive(new ExtensionOnlyTokenResourceExtension());
            Styles.Add(new SemanticOnlyLeafStyle());
            builder.UseFixtureControls();
#endif
            _packages = ((ThemeManagerBuilder)builder.Theme).ControlPackages;
        });
#if COMPLEX
        if (!this.TryFindResource(typeof(ButtonSpinner), out var spinnerBase) || spinnerBase is not Avalonia.Styling.ControlTheme original)
            throw new InvalidOperationException("PRODUCT_FAIL ButtonSpinner base theme missing");
        Resources[typeof(ButtonSpinner)] = new Avalonia.Styling.ControlTheme(typeof(ButtonSpinner))
        {
            BasedOn = original,
            Setters = { new Avalonia.Styling.Setter(Control.TagProperty, "application-spinner-theme") }
        };
#endif
    }

    public Control CreateContent()
    {
        _button = new Button { Content = "TypeMap product", IsMotionEnabled = false };
        var stack = new StackPanel();
        stack.Children.Add(_button);
#if COMPLEX
        _expander = new Expander { Header = "Published Expander", Content = "string content", IsExpanded = true, IsMotionEnabled = false };
        _include = new IncludeControl { Width = 100, Height = 25 };
        stack.Children.Add(_expander);
        stack.Children.Add(_include);
        _numeric = new NumericUpDown { Value = 42, Width = 220, Mode = NumericUpDownMode.Spinner, IsMotionEnabled = false, CornerRadius = new CornerRadius(11) };
        stack.Children.Add(_numeric);
#endif
#if COMPLEX
        _scope = new ThemeConfigProvider { Child = stack, Config = new ThemeConfigBuilder().WithToken(nameof(DesignToken.FontSize), "19").Build() };
        return _scope;
#else
        return stack;
#endif
    }

    public async Task VerifyAsync()
    {
        Require(_button.Template is not null && _button.GetVisualDescendants().Any(), "Button template executes");
        var manager = this.GetThemeManager()!;
        Require(manager.CurrentTheme is not null, "ThemeManager initialized");
        Require(_packages[0].Id == "AtomUI.Controls.Common" && _packages[1].Id == "AtomUI.Desktop.Controls", "Common commits before Desktop");
#if COMPLEX
        _button.Resources[ButtonTokenKind.ContentFontSize] = 23d;
        _button.UpdateLayout();
        Require(_button.FontSize == 23d, "local boxed-enum own Token override wins");
        _button.Resources[ButtonTokenKind.ContentFontSize] = 25d;
        _button.UpdateLayout();
        Require(_button.FontSize == 25d, "local boxed-enum own Token update republishes");
        _button.Resources.Remove(ButtonTokenKind.ContentFontSize);
        _button.UpdateLayout();
        Require(_button.FontSize == 19d, "removing local own Token restores scoped value");
        Require(_expander.Template is not null && _expander.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "string content"), "Expander string template executes");
        _expander.Content = new TextBlock { Text = "nonstring content" };
        _expander.UpdateLayout();
        Require(_expander.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "nonstring content"), "Expander nonstring content executes");
        Require(_include.Background is ISolidColorBrush brush && brush.Color == Color.Parse("#FF123456"), "cross-package ResourceInclude executes");
        var spinner = _numeric.GetVisualDescendants().OfType<ButtonSpinner>().Single();
        Require(_numeric.Template is not null && spinner.GetVisualDescendants().Any() &&
                spinner.TryFindResource(spinner.GetType(), out var spinnerTheme) &&
                spinnerTheme is Avalonia.Styling.ControlTheme { BasedOn.TargetType: { } baseTarget } && baseTarget == typeof(ButtonSpinner),
                "NumericUpDown typed ButtonSpinner base theme executes");
        Require(Equals(spinner.Tag, "application-spinner-theme"), "NumericUpDown inherits application ButtonSpinner theme replacement");
        Require(_numeric.CornerRadius == new CornerRadius(11) && !spinner.IsMotionEnabled,
                "NumericUpDown local customization survives inherited theme");
        var thirdParty = _packages.Single(p => p.Id == "Fixture.ThirdParty");
        Require(thirdParty.Controls.Any(c => c.Identity.Id == "TokenOnly") && thirdParty.Controls.Any(c => c.Identity.Id == "IdentityOnly"), "token-only and typed-identity-only registration");
        Require(thirdParty.Controls.Any(c => c.Identity.Id == "EnumOnly") &&
                thirdParty.Controls.Any(c => c.Identity.Id == "TokenKeyOnly") &&
                thirdParty.Controls.Any(c => c.Identity.Id == "ExtensionOnly"), "own-enum-only, TokenKey-only and extension-only registration");
        Require(_button.TryFindResource(EnumOnlyTokenKind.FixtureValue, out var enumValue) && Equals(enumValue, 31d), "boxed-enum-only own resource materializes");
        Require(_button.TryFindResource(ControlTokenResourceKey.Own(new ControlTokenIdentity("Fixture.ThirdParty", "TokenOnly"), TokenOnlyTokenKind.FixtureValue), out var tokenValue) && Equals(tokenValue, 17d), "token-only own resource materializes");
        var semantic = manager.SemanticParts.Controls.Single(c => c.Identity.Id == "SemanticOnly").Parts.Single(p => p.Name == "leaf");
        Require(semantic.Path == "leaf" && semantic.SelectorClass == "semantic-fixture-leaf" && semantic.SelectorRoute == ">> .semantic-fixture-leaf" && semantic.ContractType == typeof(Border) &&
                semantic.Cardinality == SemanticPartCardinality.Optional && semantic.Customization == SemanticPartCustomization.Selector && semantic.Theme is null && !semantic.CrossVisualRoot &&
                semantic.Since == "1.0.0" && semantic.RuntimeCreated && semantic.CrossNestedOwners && semantic.RestHidden && semantic.StyleType == typeof(SemanticOnlyLeafStyle), "semantic-only metadata preserved");
        var dark = await manager.ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID,
            new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build(), ThemeTransitionReason.UserRequest));
        Require(dark.Status == ThemeTransitionStatus.Committed && manager.CurrentTheme!.Appearance == ThemeAppearance.Dark, "dark theme commits");
        _scope.Config = new ThemeConfigBuilder().WithToken(nameof(DesignToken.FontSize), "29").Build();
        _button.UpdateLayout();
        Require(_button.FontSize == 29d, "scoped token update reaches template");
#endif
        await Task.CompletedTask;
        try { _builder.UseDesktopControls(); throw new Exception("Frozen registry accepted late entry"); }
        catch (InvalidOperationException e) { Require(e.Message.Contains("frozen", StringComparison.Ordinal), "Registry frozen after startup"); }
    }
    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException("PRODUCT_FAIL " + message);
        Console.WriteLine("PRODUCT_CHECK " + message);
    }
}
