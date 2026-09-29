using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls;

internal static class VisualReflectionExtensions
{
    #region 反射信息定义

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(Visual))]
    private static readonly Lazy<MethodInfo> SetVisualParentMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(Visual).GetMethodInfoOrThrow("SetVisualParent",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    #endregion
    
    public static void SetVisualParent(this Visual visual, Control? parent)
    {
        SetVisualParentMethodInfo.Value.Invoke(visual, [parent]);
    }
}
