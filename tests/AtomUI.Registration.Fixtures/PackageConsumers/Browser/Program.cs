using AtomUI;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

await AppBuilder.Configure<ConsumerApp>().StartBrowserAppAsync("out");
public sealed partial class ConsumerApp : Application
{
    public override void Initialize() => this.UseAtomUI(builder => builder.UseDesktopControls());
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            var control = Fixture.BinaryAdapter.Adapter.CreateControl();
            single.MainView = control;
            control.Loaded += (_, _) =>
            {
                if (control.GetType().Name != "SearchEdit" || control is not TemplatedControl { Template: not null } || !control.GetVisualDescendants().Any())
                    throw new Exception("COLD_BROWSER_FAIL ordinary binary adapter template");
                Console.WriteLine("COLD_BROWSER_PASS ordinary binary adapter template");
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
