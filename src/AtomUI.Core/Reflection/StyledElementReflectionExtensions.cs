using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;

namespace AtomUI.Reflection;

internal static class StyledElementReflectionExtensions
{
    #region 反射信息定义
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicProperties, typeof(StyledElement))]
    private static readonly Lazy<PropertyInfo> TemplatedParentPropertyInfo = new Lazy<PropertyInfo>(() =>
        typeof(StyledElement).GetPropertyInfoOrThrow("TemplatedParent",
            BindingFlags.Instance | BindingFlags.Public));
    
    #endregion
    
    public static void SetTemplatedParent(this StyledElement styledElement, AvaloniaObject? templateParent)
    {
        TemplatedParentPropertyInfo.Value.SetValue(styledElement, templateParent);
    }
}
