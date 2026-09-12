# Modal Changelog

本文档记录 Modal 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-11

- Semantic Parts
  - Publish the Modal family Semantic Part contract: `Dialog` and `MessageBox` each expose implicit `root` plus `mask`, `wrapper`, `container`, `header`, `title`, `body`, `footer` and `close`, mapped to the upstream Ant Design `Modal` semantic DOM (`components/modal/demo/_semantic.tsx`, 9 slots).
  - Declare every non-root Part as `RuntimeCreated`/`CrossVisualRoot`/`CrossNestedOwners` (`Dialog` has no `ControlTemplate`; nodes live in the `DialogSurface`/`OverlayDialogPresenter`/`OverlayDialogHeader`/`OverlayDialogMask` templates) and mark them with static `Classes.semantic-*="True"`.
  - Route every Part through Dialog-owned `.semantic-scope-*` anchors instead of a broad `>>`: the body permanently hosts a `Skeleton` and user content commonly contains `Card`/`Tooltip`/`Spin`, which publish same-named `semantic-header`/`semantic-title`/`semantic-body`/`semantic-footer`/`semantic-container` classes that a broad descendant would also hit.
  - Cardinality: `mask` and `wrapper` are `Optional` (modeless and the Window host do not materialize them); `footer`/`close` stay `Single` because `IsFooterVisible`/`IsClosable` only toggle `IsVisible`.
- API
  - Add `Dialog.OverlayScope` (default `null`, Overlay host only): when set, the overlay host is injected into that element's scope so the mask, surface sizing and centering are bounded by the scope instead of the owning `TopLevel`, matching upstream Ant Design's inline/`setContainer` modal semantics. The default keeps the existing TopLevel host resolution unchanged.
  - Add `Dialog.IsPinnedOpen` (default `false`, Overlay host only). While true, user-initiated close requests (header close button, mask outside-press, Escape, footer buttons) are ignored; external `IsOpen=false` still closes. This replaces the earlier plan of an example-side `BeforeCloseAsync` veto, which AXAML cannot express.
- Behavior
  - `OverlayDialogPresenter` sets the `Dialog` owner as its logical parent so owner-scoped generated Semantic Styles can reach the presenter subtree; the parent is only set while the owner is attached to a logical tree, and the visual parent remains `DialogOverlayLayer`.
  - `Dialog` implements `ISemanticPartCrossRootProvider` and reports the live Overlay presenter as its cross root.
- Docs
  - Add `semantic-part.md`; replace the previous non-contract `root`/`host`/`surface`/`content`/`motion` overview summary with the real Part table; document the `.semantic-scope-*` route anchors, host boundaries and `IsPinnedOpen` semantics.
- Gallery
  - Add a MessageBox Semantic Part styling demo inside the "Custom Semantic Part styling" example (generated `MessageBox<Part>Style` classes with an explicit owner selector, triggered by an `Open MessageBox` button next to the Dialog one), and switch every Modal example trigger from `ToggleSwitch` to `Button` (MessageBox host selection is now two buttons).
  - Stack a second `SemanticPartPreview` for MessageBox under the Dialog preview in the Semantic Parts tab; both previews use the stage-scoped inline modal pattern (`OverlayScope`), and the stage owners are explicitly sized to the stage so the `root` part can highlight (the Dialog theme defaults to a zero-size owner and the resolver skips zero-size targets).
  - Add `ModalSemanticPartHighlightTests`: hovering each of the 9 part cards in both previews yields exactly one highlight adorner, a mask press does not close the pinned previews, and switching back to Examples clears all highlights.
- Validation
  - Add `DialogSemanticPartTests`: descriptor contract, static marker lists, runtime markers, logical-parent invariant, cross-root reporting, exact single-node style hits, pin gating, modeless/hidden-node `Optional` semantics and MessageBox parity. Desktop Controls 3732/3732; LLMS generate/verify 79 controls / 161 files.
  - Gallery Semantic Parts tab and NativeAOT publish are still pending.

## 2026-08-21

- API
  - Change the `Dialog.HorizontalStartupLocation` and `Dialog.VerticalStartupLocation` metadata defaults from `Custom` to `Center`, matching `DialogOptions` and the static Dialog APIs.
- Behavior
  - Directly instantiated Dialogs now open centered by default; explicit anchors, `Custom` placement and offsets keep their existing semantics.
  - Keep Overlay structural-minimum updates initialization-aware so template layout cannot clamp the unresolved `(0, 0)` Surface position and convert it into startup offsets before centered placement.
- Docs
  - Document the shared centered startup default and the explicit `Custom` offset behavior.
  - Extend the Dialog popup family matrix with shared surface ownership: Direct Popup and specialized Popup-bearing controls inherit the primitive's null surface default, while explicit non-null brushes opt into a host-owned surface.
  - Preserve the Windows/macOS tested and Linux X11/Wayland untested evidence boundary for Popup surface behavior.
- Implementation
  - Remove the obsolete standalone manual regression application and its source-contract assertions; retain the Popup Theme default assertion and automated Dialog Popup coverage.
- Validation
  - Record the Dialog content Popup real-window manual regression as passed on Ubuntu 26.04 GNOME Wayland; Linux X11 and Drawer on Linux remain untested.

## 2026-08-20

- Design
  - Keep Overlay Dialog presentation in the owning Window `TopLevel`: `DialogOverlayLayer` uses Avalonia `OverlayLayer`, while content popups use the higher `PopupOverlayLayer` with the normal `LightDismissOverlayLayer` between them.
  - Reserve `WindowDrawnDecorations` overlay for chrome visuals and manage modal chrome coverage through a Window-owned reference-counted suppression lease shared safely by overlapping Dialog and Drawer presentations.
- API
  - Add `Dialog.IsMaskClosable` (default `true`) controlling whether pressing the Overlay modal mask requests a close; `MessageBox` inherits it and `MessageBoxOptions` exposes the same passthrough.
  - Define the two close-entry switches as orthogonal: `IsClosable` gates the header close button, `IsMaskClosable` gates the mask outside-press entry.
- Behavior
  - With `IsMaskClosable=false`, a mask press is swallowed by the topmost Overlay presenter without producing any close request, and never enters the `Closing`/`BeforeCloseAsync` pipeline.
  - Dialog content popups retain their Window `TopLevel`, open without crashes, remain clickable and preserve normal outside-click light-dismiss behavior under managed/drawn window chrome.
- Docs
  - Add the Modal content popup layering design document and link it from the public design, implementation and compatibility sections.
  - Document the mask close-entry contract in the Dialog contract groups, behavior model, compatibility invariants and maintenance invariants.
  - Expand popup verification from the ComboBox trigger to the complete Popup/Flyout/ToolTip/ContextMenu inventory and control-family matrix.
  - Define the canonical Popup family matrix, with Windows and macOS tested and Linux X11/Wayland explicitly untested.

## 2026-07-22

- Design
  - Define one Surface-body sizing contract for Overlay and native Window hosts, including requested size, structural minimum, host capacity, effective constraints and actual size.
  - Make structural minimum preserve the title, Footer actions and a non-zero content viewport whenever host capacity permits.
  - Define normal resize, capacity degradation, maximize/restore and Window chrome translation as one stable host-sizing model.
- API
  - Keep all existing `Host*` registrations and defaults while defining `HostMin*` as a request that can raise, but cannot lower, the structural minimum.
  - Keep `HostWidth/Height=NaN` as an initial natural-size request without converting natural size into a permanent minimum or writing user resize back to the public properties.
- Token
  - Define `DialogToken.MinWidth/MinHeight` as the content viewport baseline used by structural minimum resolution.
- Docs
  - Add the Modal host sizing and resize design and link it from the public design, implementation and Token documents.
- Gallery
  - Enable resize in the basic Overlay and Window examples, keep the Overlay header on its default close-only capability, demonstrate finite HostMin/Max ranges, and expose the Dialog content viewport minimum Token baselines.
- Behavior
  - Capture Overlay resize pointers and terminate resize state on both release and capture loss so a later drag cannot reuse a stale origin.
  - Preserve a natural axis as actual geometry after constraints clamp it, and resolve final natural sizing and startup placement before native Window show so the first visible frame cannot show a preliminary size or platform-default position.
  - Re-resolve structural minimum after template-subtree measure changes and translate Window chrome according to the active CSD or managed-title-bar template metrics.
  - Disable native Window maximize whenever either HostMax axis is finite, and automatically restore the capability when both axes return to PositiveInfinity.

## 2026-07-20

- Architecture
  - Unify Overlay Dialog host selection across Windows, Linux and macOS by using the drawn decorations Dialog host whenever the current platform exposes it, with TopLevel popup and scoped overlay fallback.
  - Separate complete mask bounds, Window visible-frame Dialog bounds and Dialog BoxShadow extents as independent geometry responsibilities.
  - Replace OS-specific Dialog body bounds with one Window visible-frame calculation shared with the Window frame clip.
  - Derive Dialog body owner bounds from the Window visible frame and the current DPI-rounded drawn-decoration frame thickness, while keeping host dimensions as body dimensions.
- Behavior
  - Define the modal mask as covering and blocking the complete Avalonia drawable window, including managed/drawn title bars, while applying Dialog body owner bounds derived from the Window visible frame to placement, drag, resize and maximize; native system chrome outside the client visual tree remains platform-managed.
  - Let Overlay Dialog placement, drag, resize and maximize use managed/drawn title-bar space on every platform while continuing to exclude transparent frame-shadow buffers.
  - Keep the Dialog body inside the effective Window frame on every platform while allowing its BoxShadow to be clipped naturally at the window edge.
- Performance
  - Move Overlay Dialog drag positioning from layout-affecting Margin writes to one reusable render-only Matrix translation while retaining `OffsetX` / `OffsetY` as persistence state.
- Docs
  - Document capability-driven host selection, visual-layer clipping ownership, lifecycle cleanup, AOT reflection boundary and cross-platform regression requirements.

## 2026-07-18

- Architecture
  - Replace the multi-owner host lifecycle with one `DialogSession`, two async presenters and one shared `DialogSurface` implementation.
  - Make `MessageBox` a direct Dialog specialization without a hidden Dialog or parallel host lifecycle.
- Behavior
  - Make `OpenAsync` represent the complete opening, active and teardown lifetime; reject concurrent instance opens.
  - Unify normal and forced close sources, deterministic post-commit teardown, opening/closing motion, nested focus restoration and owner close behavior.
  - Preserve natural Overlay/Window sizing, runtime size updates, placement, standard/custom buttons, loading and semantic MessageBox styles.
  - Fix real modal mask pointer routing while allowing modeless background input.
  - Construct generic static API views on the UI Dispatcher and remove Gallery dispatcher-yield workarounds from direct click handlers.
- Lifecycle
  - Pair presenter bindings, events, logical/resource parents, content references and button subscriptions with deterministic release paths.
  - Disconnect Surface composition children before host removal so retained Content or CustomButton controls cannot retain a closed Presenter/Surface.
  - Add Overlay and Window WeakReference coverage for Session, Presenter, Surface, user Content and retained custom controls.
- API
  - Remove synchronous/callback display APIs and obsolete host/action-result types.

## 2026-07-06

- API
  - Make `Dialog.IsOpen` default to `BindingMode.TwoWay` so controlled dialog open state updates the bound ViewModel without explicit binding mode.
  - Add `DialogOptions.BeforeCloseAsync`, `DialogClosingContext` and `DialogCloseReason` for static Dialog close-before validation.
- Behavior
  - Route Dialog button, keyboard, host, owner, placement-target and programmatic close requests through one async close pipeline.
- Docs
  - Document `Dialog.IsOpen` as controlled open state and clarify that it is not a Form validation value.
  - Update the close-before validation design from planned behavior to implemented API and pipeline invariants.

## 2026-07-03

- Docs
  - Add the approved design for `DialogOptions.BeforeCloseAsync` as a future close-before-validation API for static Modal dialogs.
  - Document the intended close request pipeline, event order, async validation behavior and compatibility boundaries.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Modal`.
  - Align generated output paths with `controls/modal/index-cn.md` and `controls/modal/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Modal desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Modal desktop architecture documentation under `docs/controls/desktop/feedback/modal/overview.md`.
  - Add Modal implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Modal control-level changelog.
  - Add Modal Token documentation covering DialogToken.
