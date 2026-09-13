# Notification Changelog

本文档记录 Notification 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-13

- Lifecycle / correctness
  - Serialize entry and exit actor writes through the completion of the previous motion's asynchronous cleanup, with cancellation-aware handoff and six-placement regression coverage.
  - Release expired and already-closing cards from scheduler scratch lists at the end of each wakeup, and reschedule from the time after synchronous close callbacks.
  - Select stable close batches for MaxItems and DestroyAll so immediate removal and reentrant callbacks preserve ordering, new messages and disposal boundaries.
- Cleanup
  - Remove redundant motion guards, unused motion configuration, duplicated projection checks and empty Gallery overrides while preserving animation and template contracts.
- Design / correctness
  - Define measured variable-height stack projection, `Min(3, StackThreshold)` visible layers, placement-aware back-layer clipping and immediate sibling reflow while a closing card retains its last projection.
  - Make list hover an independent state so count and threshold changes cannot desynchronize expansion or whole-stack lifetime pause.
  - Revalidate the last real pointer screen position after window resize or placement layout movement, so a moved stack cannot retain stale hover expansion or lifetime pause state.
- Motion / performance
  - Require Notification enter/exit to use a render-only actor so translate/fade does not invalidate layout on every animation frame.
  - Separate the transparent translated prepare state from the active transition with one real TopLevel animation frame, including the first card, and reuse immutable directional transforms.
  - Project threshold-hidden cards to the capped deepest scale and placement-aware half clip in the same collapse commit as opacity, preventing their full content and shadow from flashing during hover reset without adding timers or per-frame callbacks.
  - Freeze at most three visible card-content surfaces into one-shot `RenderTargetBitmap` snapshots during expanded-to-collapsed transitions; restore live content and deterministically dispose snapshots on completion, interruption, close, retemplate, detach, motion disable and manager disposal.
- API behavior
  - Disable Notification Stack by default; explicit `IsStackEnabled=true` retains threshold collapse and whole-stack hover behavior.
- Gallery
  - Add a dedicated `v6.1.9` Notification Stack showcase with isolated configuration, variable-height permanent notices and scoped destroy/dispose behavior.
- Internal architecture
  - Name the per-card motion execution owner `FeedbackCardMotionCoordinator` to reflect its coordination of actor state, cancellation and close completion.

## 2026-09-12

- Design
  - Adopt the shared Feedback stack infrastructure with the Notification stack enabled by default, threshold `3`, whole-stack hover expansion and three visible collapsed cards.
  - Define scale `1` / `0.94` / `0.88`, 8 DIP collapsed offsets, position-aware expansion and unlimited `MaxItems` as the Notification contract.
  - Add the shared `IsStackEnabled`, `StackThreshold` and `DestroyAll()` manager contract.
- Performance / lifecycle
  - Replace fixed expiration and cleanup polling with one lazy nearest-deadline scheduler that refreshes progress only for visible active cards.
  - Require stable ItemsSource-backed cards and deterministic cleanup for retemplate, detach, rehost, destroy, callback failure and dispose.
- Motion
  - Replace placement-specific oversized and scale-distorting entry motions with the shared Feedback 64 DIP translate/fade motion and full-duration Ant ease-in-out curve.
  - Animate existing cards to their new queue positions during add, remove, collapse and expand instead of jumping their layout bounds.

## 2026-08-24

- Motion
  - Reuse the shared internal `AtomUI.MotionScene.MotionExecutionState` for NotificationCard exit motion scheduling, playback and final `IsClosed` submission while preserving the public `IsClosing` / `IsClosed` contract.
  - Coalesce property-change and template-reapply close scheduling into one execution flow.

## 2026-07-20

- Fix
  - Add `NotificationType.Default` for plain notifications so the default notification path renders without a type icon.
  - Align the close button size, hover background and pressed background with shared text/icon state tokens.
  - Derive `NotificationProgressBg` from primary border hover color to primary color.
  - Align the default auto-close expiration with the documented 4.5 second duration.
  - Reduce the default internal card spacing tokens by one third while preserving the external stack spacing.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Notification`.
  - Align generated output paths with `controls/notification/index-cn.md` and `controls/notification/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Notification desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Notification desktop architecture documentation under `docs/controls/desktop/feedback/notification/overview.md`.
  - Add Notification implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Notification control-level changelog.
  - Add Notification Token documentation covering NotificationToken.
