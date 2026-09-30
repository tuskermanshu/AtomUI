using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Registration;

public class RegistrationSurfaceTests
{
    [Fact]
    public void Registration_Exposes_Only_Typed_Contracts()
    {
        var assembly = typeof(ControlPackageRegistration).Assembly;
        foreach (var retired in new[] { "AotTrimRegistration", "AotTrimRegistrationPlanRegistry", "AotTrimControlPackageRegistrationBuilder", "AotTrimUnitAttribute", "ControlPackageRegistrationEntryAttribute" })
            assembly.GetType("AtomUI.Registration." + retired).ShouldBeNull();
        typeof(ControlThemeAssetDescriptor).GetConstructors().ShouldHaveSingleItem()
            .GetParameters().First().ParameterType.ShouldBe(typeof(string));
        typeof(ControlThemeSemanticPartDescriptor).GetConstructors().ShouldHaveSingleItem()
            .GetParameters()[1].ParameterType.ShouldBe(typeof(Type));
        typeof(ControlPackageRegistration).GetConstructors().ShouldHaveSingleItem()
            .GetParameters().Length.ShouldBe(6);
    }
}
