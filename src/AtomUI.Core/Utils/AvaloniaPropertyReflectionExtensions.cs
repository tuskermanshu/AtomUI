using System.Reflection;
using AtomUI.Reflection;
using Avalonia;

namespace AtomUI.Utils;

// 反射扩展类
internal static class AvaloniaPropertyReflectionExtensions
{
    private static readonly Lazy<PropertyInfo> NotifyingPropertyInfo = new(() =>
        typeof(AvaloniaProperty).GetPropertyInfoOrThrow("Notifying",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));

    public static void InvokeNotifying(this AvaloniaProperty property, AvaloniaObject target, bool status)
    {
        var notifyingDelegate = NotifyingPropertyInfo.Value.GetValue(property)
            as Action<AvaloniaObject, bool>;
        if (notifyingDelegate == null)
        {
            return;
        }
        notifyingDelegate.Invoke(target, status);
    }
}
