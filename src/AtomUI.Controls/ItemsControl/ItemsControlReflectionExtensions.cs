using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;

namespace AtomUI.Controls;

internal static class ItemsControlReflectionExtensions
{
    #region 反射信息定义

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicProperties, typeof(ItemsControl))]
    private static readonly Lazy<PropertyInfo> WrapFocusPropertyInfo = new Lazy<PropertyInfo>(() => 
        typeof(ItemsControl).GetPropertyInfoOrThrow("WrapFocus",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    #endregion
    
    public static void SetWrapFocus(this ItemsControl itemsControl, bool value)
    {
        WrapFocusPropertyInfo.Value.SetValue(itemsControl, value);
    }
}
