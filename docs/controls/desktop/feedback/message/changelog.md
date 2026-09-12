# Message Changelog

本文档记录 Message 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-12

- Design
  - Adopt the shared Feedback stack infrastructure with an opt-in Message stack, threshold `3`, whole-stack hover expansion and static AXAML depth plates.
  - Define `TopCenter`, 3-second expiration and unlimited `MaxItems` as the Message defaults.
  - Add the shared `IsStackEnabled`, `StackThreshold`, `IsPauseOnHover` and `DestroyAll()` manager contract.
- Performance / lifecycle
  - Require a stable ItemsSource-backed collection and one lazy nearest-deadline scheduler per manager, with no per-item timers or hot-layout allocations.
  - Define deterministic cleanup for retemplate, detach, rehost, destroy, callback failure and dispose, backed by WeakReference regression coverage.

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
