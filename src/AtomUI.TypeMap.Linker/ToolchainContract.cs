using System.Reflection;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.TypeMap.Linker;

internal static class ToolchainContract
{
    internal const string Capability = "atomui-typemap-v1-illink-10.0.8";
    internal const string LinkerVersion = "10.0.8-servicing.26229.119+94ea82652cdd4e0f8046b5bd5becbd11461482ca";
    internal static string ActualLinkerVersion => typeof(LinkContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";

    internal const string LinkerIdentity = "illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

    internal static void ValidateIdentity(string identity, string version)
    {
        if (identity != LinkerIdentity || version != LinkerVersion)
        {
            throw BackendDiagnostic.Unsupported($"Unsupported ILLink identity '{identity}' ({version}). Use the tested Microsoft.NET.ILLink.Tasks 10.0.8 toolchain.");
        }
    }

    internal static void Validate(LinkContext context)
    {
        // Assembly metadata is public. No linker implementation fields, private handlers or reflection APIs are consulted.
        ValidateIdentity(typeof(LinkContext).Assembly.FullName!, ActualLinkerVersion);
        if (!context.TryGetCustomData("AtomUITypeMapBackend", out var value) || value != Capability)
        {
            throw BackendDiagnostic.Unsupported($"Missing or incompatible AtomUITypeMapBackend capability '{value}'. Expected '{Capability}'; rebuild the matching build assets and backend.");
        }

        var steps = context.Pipeline.GetSteps();
        if (steps.Any(s => s is MarkStep) || steps.Count(s => s is SweepStep) != 1 ||
            steps.Count(s => s.GetType().FullName == typeof(MaterializeTypeMapsStep).FullName) > 1 ||
            steps.Any(s => s is VerifyTypeMapsStep))
        {
            throw BackendDiagnostic.Unsupported("The TypeMap backend must run exactly once after Mark and before Sweep. Remove duplicate or incorrectly ordered custom-step injection.");
        }
    }
}
