# DateViewer Changelog

本文记录 DateViewer 家族的设计、API、主题与文档变化，不替代发布说明。

## 2026-10-04

- Behavior
  - 统一有效语言的周首、首周与跨年周号，恢复周模式的整行 Hover，并保持周号半透明文字。
  - 同端点范围预览只显示一个有效端点；空值周期浏览不产生主色选中态。
  - 范围 Preview 改为覆盖 committed Cell 视觉状态，反向编辑时只显示规范化后的两个预览端点，不再同时显示旧端点。
  - 使用单一视觉端点状态连接 selected 与 Preview，避免 Hover 时主色端点闪烁。
- Theme
  - 与 Ant Design 6 对齐：日期、月、季度、年的值框背景和 Week 整行背景采用 MotionDurationMid；周行通过透明色与状态色过渡，禁用动效时移除过渡。
  - 月、季度、年使用固定 PeriodCellWidth 和 PeriodCellPadding，Compact 值框在行内垂直居中。
  - 范围起点仅保留左侧圆角，终点仅保留右侧圆角，并保持浅色中段连续衔接。

## 2026-10-03

- API
  - 同步已存在的 DateViewer/RangeDateViewer 属性、原子 DateViewerRange、强类型事件与 Cell/Header 上下文。
  - Header 上下文包含父面板、Page/Period 参数与周期导航状态；范围双面板提供两个真实 Header。
- Theme
  - 记录五个真实叶子主题、池化 Cell 内容结构和已生成的 owner-relative Semantic Style route。
  - 区分 DateViewer.header Optional 与 RangeDateViewer.header Multiple，明确 Cell 的 TemplatedControl 契约。
- Token
  - 同步 DateViewerToken 当前九个资源、默认计算和真实主题消费者。
- Gallery
  - 新增独立 DateViewer 导航页，覆盖单值/原子范围、五种单位、空值浏览、Compact/Content、强类型模板和专用 Style。
  - 页头与示例使用 XLIFF Catalog，源片段使用现有生成入口，保持 Examples/Semantic Parts 页面范式。
- Docs
  - 主文档从批准目标职责同步为现有公共面板源码事实，保留完整视觉等价的人工验收边界。
- Validation
  - 通过 osx-arm64 NativeAOT 发布、产物验证与可执行文件启动检查；通用 ReactiveUI trimming/AOT 与本机链接器提示仍按构建日志记录。

## 2026-10-02

- Docs
  - 建立 DateViewer/RangeDateViewer 的批准目标架构、实现职责、Token 与 Semantic Part 文档，明确源码尚未迁移。
  - 将完整日期家族共享设计集中到 shared-panel-design.md，作为 DatePicker 与 Calendar 的共同设计入口。
- Architecture
  - 确定一套日期内核、统一 TemplatedControl Cell、单值/范围公共面板与明确宿主 ownership。
  - 解除旧实现兼容限制，要求 DatePicker 家族视觉、交互、绑定与 Form 结果逐步等价验证。
