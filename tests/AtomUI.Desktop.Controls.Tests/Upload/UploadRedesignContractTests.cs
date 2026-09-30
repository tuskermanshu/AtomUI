using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Upload;

public class UploadRedesignContractTests
{
    [Fact]
    public void Input_Result_Uses_Stable_Status_And_Exception_Free_Rejections()
    {
        Enum.GetNames<UploadInputBatchStatus>().ShouldBe(["Completed", "Cancelled", "Failed"]);
        Enum.GetNames<UploadInputFailureReason>().ShouldBe(["DataSnapshotFailed", "ProcessingFailed"]);
        Enum.GetNames<UploadRejectionReason>().ShouldBe([
            "UnsupportedStorageItem",
            "DirectoryNotAllowed",
            "DirectoryDepthExceeded",
            "DirectoryCycleDetected",
            "EnumerationLimitExceeded",
            "AccessDenied",
            "StorageReadFailed",
            "FileTypeNotAllowed",
            "AdmissionRejected",
            "AdmissionPolicyFailed",
            "CountLimitExceeded",
            "MultipleSelectionNotAllowed"
        ]);

        typeof(UploadRejectedItem).GetProperty("Exception").ShouldBeNull();
        typeof(UploadInputBatchCompletedEventArgs).GetProperty("IsCancelled").ShouldBeNull();
        typeof(UploadInputBatchCompletedEventArgs)
            .GetProperty(nameof(UploadInputBatchCompletedEventArgs.Status))
            .ShouldNotBeNull();
        typeof(UploadInputBatchCompletedEventArgs)
            .GetProperty(nameof(UploadInputBatchCompletedEventArgs.FailureReason))
            .ShouldNotBeNull();

        typeof(UploadAdmissionDecision).GetConstructors().ShouldBeEmpty();
        UploadAdmissionDecision.Accept().IsAccepted.ShouldBeTrue();
        var rejection = UploadAdmissionDecision.Reject("blocked", "Policy blocked the file.");
        rejection.IsAccepted.ShouldBeFalse();
        rejection.RejectionCode.ShouldBe("blocked");
        rejection.Message.ShouldBe("Policy blocked the file.");
    }

    [Fact]
    public void Upload_Template_Binds_List_To_Effective_File_Source()
    {
        var uploadTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/UploadTheme.axaml");

        uploadTheme.ShouldContain("ItemsSource=\"{TemplateBinding EffectiveFiles}\"");
        uploadTheme.ShouldNotContain("ItemsSource=\"{TemplateBinding Files}\"");
    }

    [Fact]
    public void Picture_Shape_Template_Binds_List_To_Display_Source_With_Append_Slot()
    {
        var uploadSource = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Upload.cs");
        var uploadTheme  = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/Themes/UploadTheme.axaml");
        var listSource   = ReadRepoFile("src/AtomUI.Desktop.Controls/Upload/PictureShapeList/UploadPictureShapeList.cs");

        uploadSource.ShouldContain("EffectivePictureItems");
        uploadSource.ShouldContain("UploadAppendContentItem");
        uploadSource.ShouldContain("SyncEffectivePictureItems()");
        uploadTheme.ShouldContain("ItemsSource=\"{TemplateBinding EffectivePictureItems}\"");
        uploadTheme.ShouldNotContain("Name=\"PART_AppendContent\"");
        listSource.ShouldContain("UploadAppendContentItem");
        listSource.ShouldContain("NeedsContainerOverride");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Unable to locate repository file '{relativePath}'.");
    }
}
