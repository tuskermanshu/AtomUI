# Message Changelog

本文档记录 Message 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-13

- Lifecycle / correctness
  - Serialize entry and exit actor writes through the completion of the previous motion's asynchronous cleanup, with cancellation-aware handoff and six-placement regression coverage.
  - Release expired and already-closing cards from scheduler scratch lists at the end of each wakeup, and reschedule from the time after synchronous close callbacks.
  - Select stable close batches for MaxItems and DestroyAll so immediate removal and reentrant callbacks preserve ordering, new messages and disposal boundaries.
- Cleanup
  - Remove redundant motion guards, unused motion configuration, duplicated projection checks and empty Gallery overrides while preserving animation and template contracts.
- Motion
  - Preserve the latest card's backplate dimensions while expanded and fade/slide the two plates in during card collapse, avoiding a delayed width expansion after every hover exit.
  - Keep transparent backplates outside queue measurement and mirror their edge anchor for bottom placements; cover rapid hover reversal, content width changes, motion/Stack toggles and empty queues.
- Behavior
  - Clarify that Message stack remains opt-in and never changes or pauses a message lifetime unless the pointer is actually hovering the stack.
  - Keep finite messages on their original deadlines and require `Expiration=TimeSpan.Zero` when a caller wants a persistent stacked message.
- Gallery
  - Add an Ant Design-aligned Stack showcase with runtime Enabled and Threshold controls, alternating persistent messages, and a scoped Destroy all action.
  - Isolate the Stack showcase from the default Message examples through separate lazily created managers so stack configuration and destruction cannot leak across examples.
  - Center the Stack configuration labels and controls on one row and mark the showcase with the standard `v6.1.9` RibbonBadge.
- Verification / lifecycle
  - Cover default-stack isolation, live configuration updates, persistent content ordering, scoped destruction, detach cleanup, and finite-message scheduler registration while stack is enabled.
- Internal architecture
  - Name the per-card motion execution owner `FeedbackCardMotionCoordinator` to reflect its coordination of actor state, cancellation and close completion.

## 2026-09-12

- Design
  - Adopt the shared Feedback stack infrastructure with an opt-in Message stack, threshold `3`, whole-stack hover expansion and static AXAML depth plates.
  - Define `TopCenter`, 3-second expiration and unlimited `MaxItems` as the Message defaults.
  - Add the shared `IsStackEnabled`, `StackThreshold`, `IsPauseOnHover` and `DestroyAll()` manager contract.
- Performance / lifecycle
  - Require a stable ItemsSource-backed collection and one lazy nearest-deadline scheduler per manager, with no per-item timers or hot-layout allocations.
  - Define deterministic cleanup for retemplate, detach, rehost, destroy, callback failure and dispose, backed by WeakReference regression coverage.
- Motion
  - Align Message entry and exit with the shared Feedback 64 DIP translate/fade motion, full-duration Ant ease-in-out curve and scale-preserving geometry.
  - Animate existing cards to their new queue positions during add, remove, collapse and expand instead of jumping their layout bounds.

## 2026-09-10

- Semantic Part
  - Publish two owner descriptors aligned with the upstream Message semantic keys: `MessageCard` exposes `wrapper` / `icon` / `title` (plus implicit `root`) for the notice card, and `WindowMessageManager` exposes `listContent` (plus implicit `root`, which maps the upstream list). `Since` is `6.0` for both.
  - Add generated semantic style types `MessageCardWrapperStyle`, `MessageCardIconStyle`, `MessageCardTitleStyle` and `WindowMessageManagerListContentStyle`.
  - Add static `Classes.semantic-*` markers in `MessageCardTheme.axaml` and `WindowMessageManagerTheme.axaml`; the built-in themes do not consume `.semantic-*` for default visuals.
  - Add `docs/controls/desktop/feedback/message/semantic-part.md` as the authoritative Part contract, and document the descriptor/marker mapping in `implementation.md`.
  - Add `tests/AtomUI.Desktop.Controls.Tests/Message/MessageSemanticPartTests.cs` covering descriptor fields, static markers, state-preserved marker identity, generated style hits, queue/close removal and host detach cleanup.
  - Gallery: add two `SemanticPartPreview` sections (one per owner) plus a `Custom Semantic Part styling` example using the generated style classes.
  - Record the pre-existing gap that `WindowMessageManager` never updates `Position` pseudo-classes, so the theme's `:topcenter` alignment branch is unreachable. It is not part of this Semantic Part change and is tracked in `semantic-part.md` §7.1.
- API
  - Add a public parameterless `WindowMessageManager()` constructor alongside `WindowMessageManager(TopLevel? host)`; the host overload now delegates to it. A null or omitted host means the manager is not installed into a TopLevel layer and renders inline where the caller places it. This makes the control declaratively usable from XAML, mirrors `WindowNotificationManager`, and backs the Gallery semantic preview. Covered by `Parameterless_Manager_Renders_Inline_Without_Taking_Over_The_Host_Layer`.

## 2026-08-24

- Motion
  - Reuse the shared internal `AtomUI.MotionScene.MotionExecutionState` for MessageCard exit motion scheduling, playback and final `IsClosed` submission while preserving the public `IsClosing` / `IsClosed` contract.
  - Coalesce property-change and template-reapply close scheduling into one execution flow.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Message`.
  - Align generated output paths with `controls/message/index-cn.md` and `controls/message/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Message desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Message desktop architecture documentation under `docs/controls/desktop/feedback/message/overview.md`.
  - Add Message implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Message control-level changelog.
  - Add Message Token documentation covering MessageToken.
