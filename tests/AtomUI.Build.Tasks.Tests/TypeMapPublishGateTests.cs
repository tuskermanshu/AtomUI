using AtomUI.Build.Tasks.Isolation;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class TypeMapPublishGateTests
{
    [Fact]
    public void Incorrect_actual_linker_binary_is_rejected()
    {
        var result = TaskProcessHost.Execute(new TaskRequest
        {
            TaskName = "ValidateRegistrationToolchainTask",
            Properties = { ["LinkerAssembly"] = typeof(TaskProcessHost).Assembly.Location }
        });
        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG006");
    }

    [Theory]
    [InlineData("10.0.8-servicing.26229.119+94ea82652cdd4e0f8046b5bd5becbd11461482ca", true)]
    [InlineData("10.0.9-servicing.26270.113+901ca941248413c79832d2fdbd709da0c4386353", true)]
    [InlineData("11.0.0-preview.3.26201.1+0123456789abcdef", true)]
    [InlineData("10.0.7-servicing.26170.101+0123456789abcdef", false)]
    [InlineData("8.0.28+46295af5828b062bbbf93a9cef50fd8cb9fbcb09", false)]
    [InlineData("not-a-version", false)]
    public void Desktop_native_compiler_accepts_the_verified_servicing_floor_and_later(string reportedVersion, bool accepted)
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "The fake compiler is a POSIX shell script.");
        var directory = Directory.CreateTempSubdirectory("atomui-fake-ilc-");
        try
        {
            var compiler = Path.Combine(directory.FullName, "ilc");
            File.WriteAllText(compiler, "#!/bin/sh\necho '" + reportedVersion + "'\n");
            File.SetUnixFileMode(compiler, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

            var result = TaskProcessHost.Execute(new TaskRequest
            {
                TaskName = "ValidateRegistrationToolchainTask",
                Properties = { ["NativeCompiler"] = compiler }
            });

            result.Success.ShouldBe(accepted);
            if (!accepted) result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG006");
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.9-servicing.26270.113+901ca941", true)]
    [InlineData("illink, Version=11.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "11.0.0-preview.3.26201.1+01234567", true)]
    [InlineData("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.7-servicing.26170.101+01234567", false)]
    [InlineData("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef", "10.0.9-servicing.26270.113+901ca941", false)]
    [InlineData("AtomUI.Build.Tasks, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "10.0.9", false)]
    public void Desktop_linker_requires_official_identity_at_or_after_the_verified_floor(
        string identity, string informationalVersion, bool accepted)
    {
        TypeMapBuildContract.IsSupportedDesktopLinker(identity, informationalVersion).ShouldBe(accepted);
    }

    [Fact]
    public void Missing_receipt_is_a_publish_error_even_with_no_reachable_slots()
    {
        var result = TaskProcessHost.Execute(new TaskRequest
        {
            TaskName = "VerifyTypeMapReceiptTask",
            Properties = { ["ReceiptPath"] = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"), ["SignaturePath"] = "missing" }
        });
        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG007");
    }

    [Fact]
    public void Unsupported_actual_sdk_fails_before_accepting_a_receipt()
    {
        var result = TaskProcessHost.Execute(new TaskRequest
        {
            TaskName = "PrepareTypeMapLinkTask",
            Properties = { ["SdkVersion"] = "10.0.999", ["ReceiptPath"] = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json") }
        });
        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG006");
    }
}
