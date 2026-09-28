using AtomUI;
using AtomUI.Theme.Resources;
namespace Fixture.NuGetAuthor;
public class AuthorControl : Avalonia.Controls.Control;
public class TypedKeyHostControl : Avalonia.Controls.Control;
public class TypedKeyDependencyControl : Avalonia.Controls.Control;
public class SharedResourceControl : Avalonia.Controls.Control;
public class UnrelatedResourceControl : Avalonia.Controls.Control;
public static class Registration
{
    public static IAtomUIBuilder UseAuthorControls(this IAtomUIBuilder builder) =>
        AtomUI.Generated.FixtureNuGetAuthor.GeneratedControlPackageRegistration.Register(builder, static () => new Provider());
    private sealed class Provider : ControlThemesProvider { public Provider() { Id = "Fixture.NuGetAuthor"; } }
}
