using AtomUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Threading;
using Fixture.NuGetAuthor;

AppBuilder.Configure<ResourceApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithoutStarting();
var control = new AuthorControl();
var typed = new TypedKeyHostControl();
var window = new Window { Content = new StackPanel { Children = { control, typed } } };
window.Show();
Dispatcher.UIThread.RunJobs();
if (control.Tag is not ControlTheme theme || theme.TargetType?.FullName != "Fixture.NuGetAuthor.SharedResourceControl")
    throw new Exception("STATIC_RESOURCE_FAIL explicit include did not supply the named theme");
if (typed.Tag is not ControlTheme typedTheme || typedTheme.TargetType?.FullName != "Fixture.NuGetAuthor.TypedKeyDependencyControl")
    throw new Exception("TYPED_RESOURCE_FAIL typed default provider was not selected");
window.Close();
Console.WriteLine("TYPED_RESOURCE_PASS condition comes only from AXAML type key");
Console.WriteLine("STATIC_RESOURCE_PASS trimmed=" + AtomUI.Registration.ControlRegistrationRuntime.IsTrimmed);

public partial class ResourceApp : Application
{
    public override void Initialize() => this.UseAtomUI(builder => builder.UseAuthorControls());
}
