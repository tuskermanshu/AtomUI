using AtomUI.Registration;
using Avalonia;
#if !REAL_DESKTOP
using Avalonia.Headless;
#endif
using Avalonia.Threading;
using AtomUI.Registration.Fixtures;

#if REAL_DESKTOP
AppBuilder.Configure<ProductApp>().UsePlatformDetect().SetupWithoutStarting();
#else
AppBuilder.Configure<ProductApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithoutStarting();
#endif
var app = (ProductApp)Application.Current!;
var window = new Avalonia.Controls.Window { Content = app.CreateContent(), Width = 600, Height = 500 };
window.Show();
Dispatcher.UIThread.RunJobs();
var verification = app.VerifyAsync();
while (!verification.IsCompleted) Dispatcher.UIThread.RunJobs();
verification.GetAwaiter().GetResult();
window.Close();
Console.WriteLine("PRODUCT_PASS " + (ControlRegistrationRuntime.IsTrimmed ? "selected" : "full"));
