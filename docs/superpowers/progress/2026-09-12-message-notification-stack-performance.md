# Message / Notification Stack Performance Evidence

## Qualification

- Date: 2026-09-12 (Asia/Shanghai)
- Commit under test: `87a73377d` plus the two GalleryPerformance route corrections recorded with this report; no control production code changed.
- Runtime: .NET 10.0.300, Debug, Avalonia headless, 1300 × 900 window.
- Machine: the same local macOS host is required for baseline and optimized samples.
- Primary product constraint: the new Stack behavior must not introduce a measurable regression in the same Message and Notification scenarios.
- Memory constraint: no manager, presenter, card, timer, user callback or old Gallery root may remain reachable after its documented terminal lifecycle.

## Functional baseline

The isolated worktree initially had no restore assets. After a normal restore, the focused pre-change control baseline passed:

```text
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj \
  --framework net10.0 --no-build \
  --filter "FullyQualifiedName~CloseMotionExecutionTests|FullyQualifiedName~NotificationCardThemeTests" \
  --blame-hang-timeout 90s

Passed: 11, Failed: 0, Skipped: 0, Duration: 1s
```

The first whole-project attempt entered testhost but produced no completion or test output for more than two minutes and was cancelled.
The final verification must rerun the whole project with an explicit hang timeout and preserve any dump instead of assuming success.

## Existing control runner blocker

`tools/performances/AtomUI.Performance` does not build at the baseline commit. The build reports nine unrelated stale-API errors in
Calendar, Card and Steps suites (`CalendarSelectionMode`, internal Calendar button types, `Avatar`, `StepsStyle` and
`StepsItemIndicatorType`). No Message or Notification source caused these failures.

This blocks using the aggregate control runner as-is. The feature must add a focused, persisted Feedback performance entry point or make
the existing runner select suites at compile time, then use that identical entry point before/after the control change. A throwaway script
or a benchmark that omits layout, templates, timers or cards does not qualify.

## Gallery baseline policy

The Gallery runner had stale route type/path metadata for the two moved ShowCases. The correction only updates the runtime type and XAML
source path; the actual route, readiness predicate, layout and sample counts stay unchanged.

Both commands use 10 independent cold child processes, 3 warmups and 20 measured in-process navigations:

```text
dotnet run --project tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --showcase message --label stack-baseline --cold-iterations 10 \
  --iterations 20 --warmup 3 --timeout-ms 30000 \
  --markdown /tmp/atomui-message-gallery-baseline.md

dotnet run --project tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --showcase notification --label stack-baseline --cold-iterations 10 \
  --iterations 20 --warmup 3 --timeout-ms 30000 \
  --markdown /tmp/atomui-notification-gallery-baseline.md
```

The trigger is Gallery `NavigateToCommand`; measurement ends when the real route visual tree and layout are stable. This is headless and
does not include mouse input, a visible Stack interaction, platform compositor presentation or GPU scanout.

| Control / set | Mean ms | Median ms | P95 ms | Min ms | Max ms | Mean allocated KB | Visuals |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Message cold first navigation | 146.90 | 137.43 | 186.87 | 123.12 | 206.87 | 4,968.40 | 198 |
| Message repeated navigation | 51.56 | 52.16 | 69.57 | 29.75 | 121.23 | 4,669.94 | 198 |
| Notification cold first navigation | 203.60 | 200.44 | 227.16 | 180.46 | 233.39 | 8,628.84 | 346 |
| Notification repeated navigation | 63.28 | 56.49 | 99.51 | 46.94 | 102.01 | 7,841.37 | 346 |

Raw reports:

- `/tmp/atomui-message-gallery-baseline.md`
- `/tmp/atomui-notification-gallery-baseline.md`

## Qualification interpretation

The dominant measured cost is full Gallery route materialization and layout: the page creates 198 Message or 346 Notification visuals,
while the ShowCase source itself contains only buttons and does not open a feedback Stack. Therefore these figures qualify navigation and
static Gallery-shape regressions, but cannot by themselves qualify manager show/close/toggle allocations or timer behavior.

The implementation is not performance-qualified yet. Qualification requires all of the following post-change evidence:

1. The same Gallery command, source shape, readiness predicate and sample counts.
2. A persisted focused Feedback benchmark that realizes manager templates and measures empty, single, many, collapsed and toggle paths.
3. Scheduler operation-count/state equivalence and WeakReference lifecycle tests.
4. No measurable primary metric regression after considering baseline variance; any increase must be traced to an observable feature and
   reduced without deleting Stack behavior, content, items or motion.

## Falsification hypothesis

The design is invalid if replacing per-item / fixed polling timers with one deadline scheduler and stable ItemsSource-backed layout does
not offset the added presenter/panel work. Specifically, it is falsified if an identical many-item show/toggle sample has higher median or
P95 time and allocation without a separately measured first-use-only cost, or if repeated destroy/retemplate/detach leaves any manager,
presenter, card or scheduler reachable. The post-change run must report that result even if visual behavior is correct.

## Post-change comparison

To be completed after implementation. Percentage change uses:

```text
change % = (optimized - baseline) / baseline × 100
```

Negative time/allocation values are improvements. The final table must include baseline, optimized value, formula result, noise judgment,
conclusion and the implementation complexity burden.
