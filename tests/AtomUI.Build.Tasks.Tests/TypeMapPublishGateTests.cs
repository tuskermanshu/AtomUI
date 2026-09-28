using AtomUI.Build.Tasks.Isolation;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class TypeMapPublishGateTests
{
    [Fact]
    public void Incorrect_actual_linker_binary_is_rejected_even_with_supported_sdk()
    {
        var result = TaskProcessHost.Execute(new TaskRequest
        {
            TaskName = "ValidateRegistrationToolchainTask",
            Properties = { ["SdkVersion"] = "10.0.300", ["LinkerAssembly"] = typeof(TaskProcessHost).Assembly.Location }
        });
        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Code == "ATOMUIREG006");
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
