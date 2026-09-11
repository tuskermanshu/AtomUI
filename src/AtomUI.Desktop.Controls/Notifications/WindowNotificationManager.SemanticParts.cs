using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// manager 的隐式 root 是承载全部通知卡片的容器；listContent 对应
// ReversibleStackPanel#PART_Items 的排列 / 顺序 / 对齐职责。该节点位于 manager 自身 ControlTheme，
// 因此不需要 CrossVisualRoot / RuntimeCreated。
[SemanticPart(
    "listContent",
    SelectorClass = "semantic-list-content",
    ContractType = typeof(ReversibleStackPanel),
    Since = "6.0")]
public partial class WindowNotificationManager
{
}
