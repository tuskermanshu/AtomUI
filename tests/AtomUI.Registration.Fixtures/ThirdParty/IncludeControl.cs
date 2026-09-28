using AtomUI;
using AtomUI.Theme.Resources;
using Avalonia.Controls;

namespace Fixture.ThirdParty;
public class IncludeControl : ContentControl;
public class UnusedThirdPartyControl : ContentControl;
public static class Registration
{
    public static IAtomUIBuilder UseFixtureControls(this IAtomUIBuilder builder) =>
        AtomUI.Generated.AtomUIRegistrationFixturesThirdParty.GeneratedControlPackageRegistration.Register(builder, static () => new Provider());
    private sealed class Provider : ControlThemesProvider { public Provider() { Id = "Fixture.ThirdParty"; } }
}
