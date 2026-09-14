using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaMenuItem = Avalonia.Controls.MenuItem;

[PseudoClasses(MenuItemPseudoClass.TopLevel)]
public class MenuItem : AvaloniaMenuItem, IMenuItemData, IScrollAwareControl
{
    #region 公共属性定义

    public new static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<MenuItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<MenuItem>();

    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<MenuItem>();

    public static readonly StyledProperty<int> DisplayPageSizeProperty =
        Menu.DisplayPageSizeProperty.AddOwner<MenuItem>();

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public new PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }

    #endregion

    IEnumerable<IMenuItemData> ITreeNode<IMenuItemData>.Children => EnumerateChildren();
    public ITreeNode<IMenuItemData>? ParentNode => Parent as ITreeNode<IMenuItemData>;
    public EntityKey? ItemKey { get; set; }

    private Popup? _popup;
    private bool _isSyncingSubMenuPopupState;
    private bool _isUsingDetachedTitleBarPopupPlacement;
    private IDisposable? _detachedTitleBarPopupPlacementTracker;

    private IEnumerable<IMenuItemData> EnumerateChildren()
    {
        foreach (var item in Items)
        {
            if (item is IMenuItemData menuItem)
            {
                yield return menuItem;
            }
        }
    }

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> IsCheckStateChangedEvent =
        RoutedEvent.Register<MenuItem, RoutedEventArgs>(nameof(IsCheckStateChanged), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? IsCheckStateChanged
    {
        add => AddHandler(IsCheckStateChangedEvent, value);
        remove => RemoveHandler(IsCheckStateChangedEvent, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<MenuItem>();

    internal static readonly StyledProperty<double> MaxPopupHeightProperty =
        AvaloniaProperty.Register<MenuItem, double>(nameof(MaxPopupHeight));

    internal static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<MenuItem, double>(nameof(ItemHeight));

    internal static readonly StyledProperty<Thickness> PopupPaddingProperty =
        AvaloniaProperty.Register<MenuItem, Thickness>(nameof(PopupPadding));

    internal static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<MenuItem, bool>(nameof(ShouldUseOverlayPopup));

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<MenuItem>();

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal double MaxPopupHeight
    {
        get => GetValue(MaxPopupHeightProperty);
        set => SetValue(MaxPopupHeightProperty, value);
    }

    internal double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    internal Thickness PopupPadding
    {
        get => GetValue(PopupPaddingProperty);
        set => SetValue(PopupPaddingProperty, value);
    }

    internal bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    internal bool IsPointerOverSubMenu => _popup?.IsPointerOverPopup ?? false;

    private IDisposable? _popupPinnedOpenBinding;

    #endregion

    static MenuItem()
    {
        AffectsRender<MenuItem>(BackgroundProperty);
        AffectsMeasure<MenuItem>(IconProperty);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<MenuItem>(false);
        ClickEvent.AddClassHandler<MenuItem>(
            (x, e) => x.CloseOwningMenuBeforeClickHandler(e),
            RoutingStrategies.Bubble,
            handledEventsToo: true);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsSubMenuOpenProperty)
        {
            SyncSubMenuPopupOpenState();
            if (change.GetNewValue<bool>())
            {
                ConfigureDetachedTitleBarPopupPlacement();
            }
            else
            {
                DetachedTitleBarPopupSupport.ClearPopupPlacementTracker(
                    ref _detachedTitleBarPopupPlacementTracker);
            }
        }

        base.OnPropertyChanged(change);
        if (change.Property == ParentProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == IconProperty)
        {
            if (change.OldValue is Icon oldIcon)
            {
                oldIcon.SetTemplatedParent(null);
            }

            if (change.NewValue is Icon newIcon)
            {
                LogicalChildren.Remove(newIcon);
                newIcon.SetTemplatedParent(this);
            }
        }
        else if (change.Property == IsCheckedProperty)
        {
            RaiseEvent(new RoutedEventArgs(IsCheckStateChangedEvent, this));
        }
        else if (change.Property == DisplayPageSizeProperty ||
                 change.Property == ItemHeightProperty ||
                 change.Property == PopupPaddingProperty ||
                 change.Property == IsScrollEnabledProperty)
        {
            ConfigureMaxPopupHeight();
        }
        else if (((change.Property == IsPopupPinnedOpenProperty && change.GetNewValue<bool>()) ||
                  (change.Property == IsSubMenuOpenProperty && !change.GetNewValue<bool>() && IsPopupPinnedOpen)) &&
                 HasSubMenu &&
                 !IsSubMenuOpen)
        {
            SetCurrentValue(IsSubMenuOpenProperty, true);
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(MenuItemPseudoClass.TopLevel, IsTopLevel);
    }

    // 由 owner 在容器 prepare 时下发。None 表示该 MenuItem 不属于 plain Menu 语义作用域（MenuFlyout /
    // ContextMenu / DropdownButton 弹层创建并复用同一容器类型），此时保持既有 .semantic-item marker。
    internal MenuSemanticLevel SemanticLevel { get; set; }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is MenuSeparatorData)
        {
            return new MenuSeparator();
        }

        if (item is MenuItemGroupData)
        {
            return new MenuItemGroup();
        }

        var menuItem = new MenuItem();
        menuItem.Classes.Add(DropdownButtonSemanticParts.ItemClass);
        return menuItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is MenuItem or MenuSeparator or MenuItemGroup)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MenuItem menuItem)
        {
            var childLevel = ResolveChildSemanticLevel();
            menuItem.Classes.Add(DropdownButtonSemanticParts.ItemClass);
            if (childLevel != MenuSemanticLevel.None)
            {
                menuItem.SemanticLevel = childLevel;
                MenuSemanticLevelScope.ApplyItemLevel(menuItem, childLevel);
            }

            if (item != null && item is not Visual)
            {
                if (!menuItem.IsSet(HeaderProperty))
                {
                    menuItem.SetCurrentValue(HeaderProperty, item);
                }

                if (item is IMenuItemData menuItemData)
                {
                    if (!menuItem.IsSet(IconProperty))
                    {
                        menuItem.SetCurrentValue(IconProperty, menuItemData.Icon);
                    }

                    if (menuItem.ItemKey == null)
                    {
                        menuItem.ItemKey = menuItemData.ItemKey;
                    }

                    if (!menuItem.IsSet(IsEnabledProperty))
                    {
                        menuItem.SetCurrentValue(IsEnabledProperty, menuItemData.IsEnabled);
                    }

                    if (!menuItem.IsSet(InputGestureProperty))
                    {
                        menuItem.SetCurrentValue(InputGestureProperty, menuItemData.InputGesture);
                    }
                }
            }

            if (ItemTemplate != null)
            {
                menuItem[!HeaderTemplateProperty] = this[!ItemTemplateProperty];
            }

            menuItem[!ItemTemplateProperty]          = this[!ItemTemplateProperty];
            menuItem[!SizeTypeProperty]              = this[!SizeTypeProperty];
            menuItem[!IsMotionEnabledProperty]       = this[!IsMotionEnabledProperty];
            menuItem[!ShouldUseOverlayPopupProperty] = this[!ShouldUseOverlayPopupProperty];
            PrepareMenuItem(menuItem, item, index);
        }
        else if (container is MenuSeparator menuSeparator)
        {
            menuSeparator.Orientation = Orientation.Horizontal;
        }
        else if (container is MenuItemGroup menuItemGroup)
        {
            // 分组标题与子项的样式由 MenuItemGroup 自身的模板与容器逻辑处理。
            var childLevel = ResolveChildSemanticLevel();
            if (childLevel != MenuSemanticLevel.None)
            {
                menuItemGroup.SemanticLevel = childLevel;
                MenuSemanticLevelScope.ApplyGroupLevel(menuItemGroup, childLevel);
            }
        }
        else if (container is not MenuSeparator)
        {
            throw new ArgumentOutOfRangeException(nameof(container),
                "The container type is incorrect, it must be type MenuItem or MenuSeparator.");
        }
    }

    protected virtual void PrepareMenuItem(MenuItem menuItem, object? item, int index)
    {
    }

    // 自身层级决定子容器层级：plain Menu 顶层项的子项属于子菜单层，子菜单项的子项仍在子菜单层。
    // None 表示不在 plain Menu 语义作用域内，子容器保持复用方既有的 marker 行为。
    private MenuSemanticLevel ResolveChildSemanticLevel()
    {
        return SemanticLevel == MenuSemanticLevel.None
            ? MenuSemanticLevel.None
            : MenuSemanticLevel.SubMenu;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ClearDetachedTitleBarPopupPlacement();
        _popupPinnedOpenBinding?.Dispose();
        _popupPinnedOpenBinding = null;
        if (_popup is not null)
        {
            _popup.Opened -= HandleSubMenuPopupOpened;
            _popup.Closed -= HandleSubMenuPopupClosed;
        }

        base.OnApplyTemplate(e);
        // icon / content 的层级 marker 由模板静态声明（同 NavMenu），不在这里按 Name 动态注入。
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_popup != null)
        {
            _popup.Opened += HandleSubMenuPopupOpened;
            _popup.Closed += HandleSubMenuPopupClosed;
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                _popup,
                Popup.IsPopupPinnedOpenProperty);
            if (IsSubMenuOpen)
            {
                DeferSubMenuPopupOpen(_popup);
            }
        }
        ConfigureDetachedTitleBarPopupPlacement();
        UpdatePseudoClasses();
        ConfigureMaxPopupHeight();
    }

    private void SyncSubMenuPopupOpenState()
    {
        // 模板尚未应用时（声明式 IsSubMenuOpen="True"）由 OnApplyTemplate 延迟同步：
        // 子菜单弹层不能在模板应用 / 父弹层的强制布局期间同步 Open()。
        if (_popup is null)
        {
            return;
        }

        // 业务打开状态与弹层呈现之间是双向传播：这里写 Popup.IsOpen 会同步派发 Opened / Closed，
        // 事件处理器又会把 IsSubMenuOpen 写回并回到本方法。弹层无法呈现时（放置目标跑出 TopLevel
        // 可视矩形，例如菜单在长页面折叠线以下）该写回不会收敛，会形成无界同步递归直到栈溢出。
        // 正在驱动弹层时不再回写驱动：弹层已经处于目标状态，重入同步没有语义。
        if (_isSyncingSubMenuPopupState)
        {
            return;
        }

        _isSyncingSubMenuPopupState = true;
        try
        {
            _popup.IsOpen = IsSubMenuOpen;
        }
        finally
        {
            _isSyncingSubMenuPopupState = false;
        }
    }

    private void DeferSubMenuPopupOpen(Popup popup)
    {
        // 声明式打开（IsSubMenuOpen="True"，对应上游 defaultOpenKeys）在模板应用时物化。
        // 此刻通常处于父弹层 OverlayPopupHost.Show 的强制布局 / 模板应用期间，
        // 同步 Open() 会在 PopupOverlayLayer.MeasureOverride 枚举 Children 时修改集合
        // （Collection was modified），因此延迟到下一个调度帧再打开。
        Dispatcher.Post(() =>
        {
            if (ReferenceEquals(popup, _popup) && IsSubMenuOpen && !popup.IsOpen)
            {
                popup.IsOpen = true;
            }
        });
    }

    private void HandleSubMenuPopupOpened(object? sender, EventArgs e)
    {
        if (!IsSubMenuOpen)
        {
            SetCurrentValue(IsSubMenuOpenProperty, true);
        }
    }

    private void HandleSubMenuPopupClosed(object? sender, EventArgs e)
    {
        if (IsSubMenuOpen)
        {
            SetCurrentValue(IsSubMenuOpenProperty, false);
        }
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureDetachedTitleBarPopupPlacement();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ClearDetachedTitleBarPopupPlacement();
        base.OnDetachedFromLogicalTree(e);
    }

    public async Task CloseItemAsync(CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is MenuItem childMenuItem)
            {
                await childMenuItem.CloseItemAsync(cancellationToken);
            }
        }

        IsSubMenuOpen = false;
    }

    internal void CloseForLifecycle()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is MenuItem childMenuItem)
            {
                childMenuItem.CloseForLifecycle();
            }
        }

        _popup?.CloseForLifecycle();
        SetCurrentValue(IsSubMenuOpenProperty, false);
    }

    private void CloseOwningMenuBeforeClickHandler(RoutedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this) || HasSubMenu || StaysOpenOnClick)
        {
            return;
        }

        CloseOwningMenuImmediately();
    }

    private void CloseOwningMenuImmediately()
    {
        StyledElement? current = Parent;
        while (current != null)
        {
            if (current is Menu menu)
            {
                menu.CloseImmediately();
                return;
            }

            if (current is ContextMenu contextMenu)
            {
                contextMenu.Close();
                return;
            }

            if (current is MenuItem menuItem)
            {
                menuItem.Close();
            }

            current = current.Parent;
        }
    }

    private void ConfigureMaxPopupHeight()
    {
        var maxPopupHeight = IsScrollEnabled
            ? ItemHeight * DisplayPageSize + PopupPadding.Top + PopupPadding.Bottom
            : double.PositiveInfinity;
        SetCurrentValue(MaxPopupHeightProperty, maxPopupHeight);
    }

    private void ConfigureDetachedTitleBarPopupPlacement()
    {
        DetachedTitleBarPopupSupport.ConfigurePopupPlacement(
            this,
            _popup,
            ref _isUsingDetachedTitleBarPopupPlacement,
            IsTopLevel);
        _detachedTitleBarPopupPlacementTracker =
            DetachedTitleBarPopupSupport.UpdatePopupPlacementTracker(
                this,
                _popup,
                _detachedTitleBarPopupPlacementTracker,
                () => IsSubMenuOpen,
                IsTopLevel && IsSubMenuOpen);
    }

    private void ClearDetachedTitleBarPopupPlacement()
    {
        DetachedTitleBarPopupSupport.ClearPopupPlacementTracker(
            ref _detachedTitleBarPopupPlacementTracker);
        DetachedTitleBarPopupSupport.ClearPopupPlacement(
            _popup,
            ref _isUsingDetachedTitleBarPopupPlacement);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.DisableTransitions();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.EnableTransitions();
    }
}
