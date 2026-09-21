using System.Diagnostics;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Localization;
using AtomUI.Icons.AntDesign;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum PaginationAlign
{
    Start,
    Center,
    End
}

public partial class Pagination : AbstractPagination
{
    #region 公共属性定义
    
    public static readonly StyledProperty<bool> IsShowSizeChangerProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(IsShowSizeChanger));

    public static readonly StyledProperty<IReadOnlyList<int>?> PageSizeOptionsProperty =
        AvaloniaProperty.Register<Pagination, IReadOnlyList<int>?>(
            nameof(PageSizeOptions),
            validate: ValidatePageSizeOptions);
    
    public static readonly StyledProperty<bool> IsShowQuickJumperProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(IsShowQuickJumper));
    
    public static readonly StyledProperty<bool> IsShowTotalInfoProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(IsShowTotalInfo));

    public static readonly StyledProperty<bool> IsShowLessItemsProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(IsShowLessItems));

    public static readonly StyledProperty<bool> IsShowPrevNextJumpersProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(IsShowPrevNextJumpers), true);

    public static readonly StyledProperty<string?> TotalInfoTemplateProperty =
        AvaloniaProperty.Register<Pagination, string?>(nameof(TotalInfoTemplate));
    
    public bool IsShowSizeChanger
    {
        get => GetValue(IsShowSizeChangerProperty);
        set => SetValue(IsShowSizeChangerProperty, value);
    }

    public IReadOnlyList<int>? PageSizeOptions
    {
        get => GetValue(PageSizeOptionsProperty);
        set => SetValue(PageSizeOptionsProperty, value);
    }
    
    public bool IsShowQuickJumper
    {
        get => GetValue(IsShowQuickJumperProperty);
        set => SetValue(IsShowQuickJumperProperty, value);
    }
    
    public bool IsShowTotalInfo
    {
        get => GetValue(IsShowTotalInfoProperty);
        set => SetValue(IsShowTotalInfoProperty, value);
    }

    public bool IsShowLessItems
    {
        get => GetValue(IsShowLessItemsProperty);
        set => SetValue(IsShowLessItemsProperty, value);
    }

    public bool IsShowPrevNextJumpers
    {
        get => GetValue(IsShowPrevNextJumpersProperty);
        set => SetValue(IsShowPrevNextJumpersProperty, value);
    }
    
    public string? TotalInfoTemplate
    {
        get => GetValue(TotalInfoTemplateProperty);
        set => SetValue(TotalInfoTemplateProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Pagination, ComboBox?> SizeChangerProperty =
        AvaloniaProperty.RegisterDirect<Pagination, ComboBox?>(nameof(SizeChanger),
            o => o.SizeChanger,
            (o, v) => o.SizeChanger = v);

    internal static readonly DirectProperty<Pagination, QuickJumperBar?> QuickJumperBarProperty =
        AvaloniaProperty.RegisterDirect<Pagination, QuickJumperBar?>(nameof(QuickJumperBar),
            o => o.QuickJumperBar,
            (o, v) => o.QuickJumperBar = v);

    internal static readonly DirectProperty<Pagination, string?> PageTextProperty =
        AvaloniaProperty.RegisterDirect<Pagination, string?>(nameof(PageText),
            o => o.PageText,
            (o, v) => o.PageText = v);

    internal static readonly DirectProperty<Pagination, string?> TotalInfoTextProperty =
        AvaloniaProperty.RegisterDirect<Pagination, string?>(nameof(TotalInfoText),
            o => o.TotalInfoText,
            (o, v) => o.TotalInfoText = v);

    private ComboBox? _sizeChanger;

    internal ComboBox? SizeChanger
    {
        get => _sizeChanger;
        set => SetAndRaise(SizeChangerProperty, ref _sizeChanger, value);
    }

    private QuickJumperBar? _quickJumperBar;

    internal QuickJumperBar? QuickJumperBar
    {
        get => _quickJumperBar;
        set => SetAndRaise(QuickJumperBarProperty, ref _quickJumperBar, value);
    }

    private string? _pageText;

    public string? PageText
    {
        get => _pageText;
        set => SetAndRaise(PageTextProperty, ref _pageText, value);
    }

    private string? _totalInfoText;

    public string? TotalInfoText
    {
        get => _totalInfoText;
        set => SetAndRaise(TotalInfoTextProperty, ref _totalInfoText, value);
    }

    #endregion

    #region 内部协作 API

    internal const int MaxNavItemCount = 9;

    #endregion

    private static readonly int[] DefaultPageSizeOptions = [10, 20, 50, 100];

    private PaginationNav? _paginationNav;
    private PaginationNavItem? _previousPageItem;
    private PaginationNavItem? _nextPageItem;
    private int _nextPushItemIndex = 1;
    private int _selectedNavItemIndex = -1;
    private IDisposable? _sizeChangerDisposable;
    private IDisposable? _quickJumperDisposable;
    private ILanguageManager? _subscribedLanguageManager;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachLanguageListener();
        RefreshAutomationNames();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachLanguageListener();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_paginationNav is not null)
        {
            _paginationNav.ContainerPrepared   -= HandleContainerPrepared;
            _paginationNav.PageNavigateRequest -= HandlePageNavRequest;
        }

        _paginationNav = e.NameScope.Find<PaginationNav>("PART_Nav");
        Debug.Assert(_paginationNav is not null);
        _paginationNav.ContainerPrepared   -= HandleContainerPrepared;
        _paginationNav.PageNavigateRequest -= HandlePageNavRequest;
        _paginationNav.ContainerPrepared   += HandleContainerPrepared;
        _paginationNav.PageNavigateRequest += HandlePageNavRequest;
        if (IsShowQuickJumper)
        {
            SetupQuickJumper();
        }

        if (IsShowSizeChanger)
        {
            SetupSizeChanger();
        }
    }

    private void HandleContainerPrepared(object? sender, ContainerPreparedEventArgs args)
    {
        Debug.Assert(_paginationNav is not null);
        var count = _paginationNav.ItemCount;
        if (args.Container is PaginationNavItem navItem)
        {
            if (0 == args.Index)
            {
                navItem.PaginationItemType = PaginationItemType.Previous;
                _previousPageItem          = navItem;
                _previousPageItem.Icon = FlowDirection == FlowDirection.RightToLeft
                    ? new RightOutlined()
                    : new LeftOutlined();
                AutomationProperties.SetName(_previousPageItem,
                    GetLocalizedText(PaginationLangResourceKind.PreviousPageText, "Previous Page"));
            }
            else if (count - 1 == args.Index)
            {
                navItem.PaginationItemType = PaginationItemType.Next;
                _nextPageItem              = navItem;
                _nextPageItem.Icon = FlowDirection == FlowDirection.RightToLeft
                    ? new LeftOutlined()
                    : new RightOutlined();
                AutomationProperties.SetName(_nextPageItem,
                    GetLocalizedText(PaginationLangResourceKind.NextPageText, "Next Page"));
            }
            else
            {
                navItem.PaginationItemType = PaginationItemType.PageIndicator;
            }
        }
        TemplateConfigured = true;
        if (HasRealizedContainerCount(_paginationNav, count))
        {
            HandlePageConditionChanged();
        }
    }

    private static bool HasRealizedContainerCount(ItemsControl itemsControl, int expectedCount)
    {
        var realizedCount = 0;
        foreach (var _ in itemsControl.GetRealizedContainers())
        {
            ++realizedCount;
            if (realizedCount > expectedCount)
            {
                return false;
            }
        }

        return realizedCount == expectedCount;
    }

    protected override void NotifyPageConditionChanged(int currentPage, int pageCount, int pageSize, long total)
    {
        ConfigureNavigationItems(currentPage, pageCount);
        base.NotifyPageConditionChanged(currentPage, pageCount, pageSize, total);
    }

    private void ConfigureNavigationItems(int currentPage, int pageCount)
    {
        if (TemplateConfigured)
        {
            Debug.Assert(_paginationNav != null);
            Debug.Assert(_previousPageItem != null);
            Debug.Assert(_nextPageItem != null);
            var count = _paginationNav.ItemCount;
            // 清空状态 clear state
            _paginationNav.SelectedIndex = -1;
            _selectedNavItemIndex        = -1;
            for (int i = 1; i < count - 1; i++)
            {
                var container = _paginationNav.ContainerFromIndex(i);
                if (container is PaginationNavItem navItem)
                {
                    navItem.PaginationItemType = PaginationItemType.PageIndicator;
                    navItem.IsVisible          = false;
                    navItem.Content            = null;
                    navItem.Icon               = null;
                    navItem.JumpIcon           = null;
                    navItem.ClearValue(AutomationProperties.NameProperty);
                }
            }

            _previousPageItem.IsEnabled  = currentPage > 1;
            _previousPageItem.PageNumber = Math.Max(1, CurrentPage - 1);
            _nextPageItem.IsEnabled      = currentPage < pageCount;
            _nextPageItem.PageNumber     = Math.Min(pageCount, CurrentPage + 1);
            _nextPushItemIndex           = 1;

            foreach (var item in CreateNavigationItems(currentPage, pageCount))
            {
                SetupNextNavigationItem(item);
            }

            _paginationNav.SelectedIndex = _selectedNavItemIndex;
            SetupTotalInfoText();
        }
    }

    internal IReadOnlyList<PaginationNavigationItem> CreateNavigationItems(int currentPage, int pageCount)
    {
        return PaginationNavigationModel.Build(
            currentPage,
            pageCount,
            IsShowLessItems,
            IsShowPrevNextJumpers);
    }

    private void HandlePageNavRequest(object? sender, PageNavRequestArgs args)
    {
        if (args.PageNumber != CurrentPage)
        {
            SetCurrentValue(CurrentPageProperty, args.PageNumber);
        }
    }

    private void SetupNextNavigationItem(PaginationNavigationItem item)
    {
        if (_nextPushItemIndex <= 0 || _nextPushItemIndex >= MaxNavItemCount - 1)
        {
            throw new ArgumentException("Invalid next push item index");
        }

        Debug.Assert(_paginationNav != null);
        var navItem = _paginationNav.ContainerFromIndex(_nextPushItemIndex++) as PaginationNavItem;

        if (item.IsActive)
        {
            _selectedNavItemIndex = _nextPushItemIndex - 1;
        }

        Debug.Assert(navItem != null);
        navItem.PaginationItemType = item.ItemType;
        navItem.PageNumber         = item.PageNumber;
        navItem.Content = item.ItemType == PaginationItemType.PageIndicator
            ? $"{item.PageNumber}"
            : null;
        navItem.Icon = item.ItemType is PaginationItemType.JumpPrevious or PaginationItemType.JumpNext
            ? new EllipsisOutlined()
            : null;
        navItem.JumpIcon = item.ItemType switch
        {
            PaginationItemType.JumpPrevious when FlowDirection == FlowDirection.RightToLeft => new DoubleRightOutlined(),
            PaginationItemType.JumpPrevious => new DoubleLeftOutlined(),
            PaginationItemType.JumpNext when FlowDirection == FlowDirection.RightToLeft => new DoubleLeftOutlined(),
            PaginationItemType.JumpNext => new DoubleRightOutlined(),
            _ => null
        };
        navItem.SetValue(AutomationProperties.NameProperty, GetAutomationName(item));
        navItem.IsVisible = true;
    }

    private void RefreshNavigationItems()
    {
        if (TemplateConfigured)
        {
            ConfigureNavigationItems(CurrentPage, PageCount);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsShowSizeChangerProperty)
            {
                SetupSizeChanger();
            }
            else if (change.Property == IsShowQuickJumperProperty)
            {
                SetupQuickJumper();
            }
        }

        if (change.Property == IsShowLessItemsProperty ||
            change.Property == IsShowPrevNextJumpersProperty)
        {
            RefreshNavigationItems();
        }
        else if (change.Property == FlowDirectionProperty)
        {
            UpdatePreviousAndNextIcons();
            RefreshNavigationItems();
        }

        if (change.Property == PageTextProperty ||
            change.Property == PageSizeOptionsProperty)
        {
            SyncSizeChangerItems();
        }
        else if (change.Property == PageSizeProperty)
        {
            SyncSizeChangerSelection();
        }
    }

    private void UpdatePreviousAndNextIcons()
    {
        if (_previousPageItem != null)
        {
            _previousPageItem.Icon = FlowDirection == FlowDirection.RightToLeft
                ? new RightOutlined()
                : new LeftOutlined();
        }

        if (_nextPageItem != null)
        {
            _nextPageItem.Icon = FlowDirection == FlowDirection.RightToLeft
                ? new LeftOutlined()
                : new RightOutlined();
        }
    }

    private string? GetAutomationName(PaginationNavigationItem item)
    {
        return item.ItemType switch
        {
            PaginationItemType.PageIndicator => $"{PageText} {item.PageNumber}",
            PaginationItemType.JumpPrevious => GetLocalizedText(
                PaginationLangResourceKind.PreviousPagesTextFormat,
                $"Previous {(IsShowLessItems ? 3 : 5)} Pages",
                IsShowLessItems ? 3 : 5),
            PaginationItemType.JumpNext => GetLocalizedText(
                PaginationLangResourceKind.NextPagesTextFormat,
                $"Next {(IsShowLessItems ? 3 : 5)} Pages",
                IsShowLessItems ? 3 : 5),
            _ => null
        };
    }

    private static string GetLocalizedText(
        PaginationLangResourceKind resourceKind,
        string fallback,
        params object?[] arguments)
    {
        var localizer = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)
            : null;
        if (localizer is null)
        {
            return fallback;
        }

        return arguments.Length == 0
            ? localizer.Get(resourceKind)
            : localizer.Format(resourceKind, arguments);
    }

    private void AttachLanguageListener()
    {
        if (_subscribedLanguageManager is not null)
        {
            return;
        }

        var languageManager = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)
            : null;
        if (languageManager is null)
        {
            return;
        }

        languageManager.LanguageChanged += HandleLanguageChanged;
        _subscribedLanguageManager = languageManager;
    }

    private void DetachLanguageListener()
    {
        if (_subscribedLanguageManager is null)
        {
            return;
        }

        _subscribedLanguageManager.LanguageChanged -= HandleLanguageChanged;
        _subscribedLanguageManager = null;
    }

    private void HandleLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        RefreshAutomationNames();
    }

    private void RefreshAutomationNames()
    {
        if (_previousPageItem != null)
        {
            AutomationProperties.SetName(_previousPageItem,
                GetLocalizedText(PaginationLangResourceKind.PreviousPageText, "Previous Page"));
        }

        if (_nextPageItem != null)
        {
            AutomationProperties.SetName(_nextPageItem,
                GetLocalizedText(PaginationLangResourceKind.NextPageText, "Next Page"));
        }

        RefreshNavigationItems();
    }

    private void SyncSizeChangerItems()
    {
        if (SizeChanger != null)
        {
            SizeChanger.SelectionChanged -= HandlePageSizeChanged;
            try
            {
                SizeChanger.SelectedIndex = -1;
                SizeChanger.Items.Clear();
                foreach (var pageSize in GetEffectivePageSizeOptions())
                {
                    SizeChanger.Items.Add(new PageSizeComboBoxItem
                    {
                        Content  = $"{pageSize} / {PageText}",
                        PageSize = pageSize
                    });
                }

                var selectedPageSize = PageSize <= 0 ? DefaultPageSize : PageSize;
                SizeChanger.SelectedIndex = -1;
                if (TryFindSizeChangerItemIndex(selectedPageSize, out var index))
                {
                    SizeChanger.SelectedIndex = index;
                }
            }
            finally
            {
                SizeChanger.SelectionChanged += HandlePageSizeChanged;
            }
        }
    }

    private void SyncSizeChangerSelection()
    {
        if (SizeChanger == null)
        {
            return;
        }

        var selectedPageSize = PageSize <= 0 ? DefaultPageSize : PageSize;
        if (TryFindSizeChangerItemIndex(selectedPageSize, out var index))
        {
            SetSizeChangerSelectedIndex(index);
        }
        else
        {
            SyncSizeChangerItems();
        }
    }

    private bool TryFindSizeChangerItemIndex(int pageSize, out int index)
    {
        index = -1;
        if (SizeChanger == null)
        {
            return false;
        }

        for (var i = 0; i < SizeChanger.Items.Count; i++)
        {
            if (SizeChanger.Items.GetAt(i) is PageSizeComboBoxItem pageSizeItem &&
                pageSizeItem.PageSize == pageSize)
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    private void SetSizeChangerSelectedIndex(int index)
    {
        Debug.Assert(SizeChanger != null);
        if (SizeChanger.SelectedIndex == index)
        {
            return;
        }

        SizeChanger.SelectionChanged -= HandlePageSizeChanged;
        try
        {
            SizeChanger.SelectedIndex = index;
        }
        finally
        {
            SizeChanger.SelectionChanged += HandlePageSizeChanged;
        }
    }

    private IEnumerable<int> GetEffectivePageSizeOptions()
    {
        var selectedPageSize = PageSize <= 0 ? DefaultPageSize : PageSize;
        var pageSizeOptions  = PageSizeOptions ?? DefaultPageSizeOptions;
        var emittedPageSizes = new HashSet<int>();

        if (selectedPageSize > 0 && !pageSizeOptions.Contains(selectedPageSize))
        {
            emittedPageSizes.Add(selectedPageSize);
            yield return selectedPageSize;
        }

        foreach (var pageSize in pageSizeOptions)
        {
            if (emittedPageSizes.Add(pageSize))
            {
                yield return pageSize;
            }
        }
    }

    private void SetupTotalInfoText()
    {
        if (IsShowTotalInfo && TotalInfoTemplate != null)
        {
            TotalInfoText = TotalInfoTemplate.Replace("${Total}", $"{Total}")
                                             .Replace("${RangeStart}", $"{(CurrentPage - 1) * PageSize}")
                                             .Replace("${RangeEnd}", $"{Math.Min(CurrentPage * PageSize, Total)}");
        }
    }

    private void SetupSizeChanger()
    {
        if (!IsShowSizeChanger)
        {
            ClearSizeChanger();
            return;
        }

        if (SizeChanger == null)
        {
            var sizeChanger = new ComboBox();
            sizeChanger.VerticalAlignment = VerticalAlignment.Center;
            _sizeChangerDisposable?.Dispose();
            _sizeChangerDisposable = BindUtils.RelayBind(this, SizeTypeProperty, sizeChanger, ComboBox.SizeTypeProperty);
            SizeChanger                  =  sizeChanger;
            SyncSizeChangerItems();
        }
    }

    private void ClearSizeChanger()
    {
        if (SizeChanger is not null)
        {
            SizeChanger.SelectionChanged -= HandlePageSizeChanged;
            SizeChanger = null;
        }

        _sizeChangerDisposable?.Dispose();
        _sizeChangerDisposable = null;
    }

    private void SetupQuickJumper()
    {
        if (!IsShowQuickJumper)
        {
            ClearQuickJumper();
            return;
        }

        if (QuickJumperBar == null)
        {
            QuickJumperBar = new QuickJumperBar();
            QuickJumperBar.JumpRequest += HandleQuickJumpRequested;
            _quickJumperDisposable?.Dispose();
            _quickJumperDisposable = BindUtils.RelayBind(this, SizeTypeProperty, QuickJumperBar, QuickJumperBar.SizeTypeProperty);
        }
    }

    private void ClearQuickJumper()
    {
        if (QuickJumperBar is not null)
        {
            QuickJumperBar.JumpRequest -= HandleQuickJumpRequested;
            QuickJumperBar = null;
        }

        _quickJumperDisposable?.Dispose();
        _quickJumperDisposable = null;
    }

    private void HandleQuickJumpRequested(object? sender, QuickJumpArgs args)
    {
        var total     = Math.Max(0, Total);
        var pageSize  = PageSize <= 0 ? DefaultPageSize : PageSize;
        var pageCount = (int)Math.Ceiling(total / (double)pageSize);
        SetCurrentValue(CurrentPageProperty, Math.Max(1, Math.Min(pageCount, args.PageNumber)));
    }

    private void HandlePageSizeChanged(object? sender, SelectionChangedEventArgs? args)
    {
        if (args?.AddedItems.Count >= 1 && args.AddedItems[0] is PageSizeComboBoxItem comboBoxItem)
        {
            SetCurrentValue(PageSizeProperty, Math.Max(comboBoxItem.PageSize, 1));
        }
    }

    private static bool ValidatePageSizeOptions(IReadOnlyList<int>? pageSizeOptions)
    {
        return pageSizeOptions is null || pageSizeOptions.All(pageSize => pageSize > 0);
    }
}
