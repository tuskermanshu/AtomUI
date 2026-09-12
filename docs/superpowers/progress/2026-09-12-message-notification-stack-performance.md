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

This blocks using the aggregate control runner as-is. The implementation therefore adds a focused, persisted Feedback entry point to
`AtomUI.GalleryPerformance`; it realizes the manager, presenter, cards and layout and validates collapsed-card counts and post-destroy
visual cleanup. It is invoked with `--feedback-stack` and is retained for future regressions.

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

Qualification requires all of the following post-change evidence:

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

## Post-change Gallery comparison

The post-change Gallery XAML source shape is unchanged: Message remains 198 visuals and Notification remains 346 visuals. The same
commands, readiness predicates and sample counts were used. Percentage change uses:

```text
change % = (optimized - baseline) / baseline × 100
```

Negative time/allocation values are improvements.

| Control / set | Baseline allocated KB | Optimized allocated KB | Change | Visuals | Conclusion |
| --- | ---: | ---: | ---: | ---: | --- |
| Message cold | 4,968.40 | 4,951.39 | -0.34% | 198 -> 198 | no allocation or shape regression |
| Message repeated | 4,669.94 | 4,669.59 | -0.01% | 198 -> 198 | equivalent |
| Notification cold | 8,628.84 | 8,624.63 | -0.05% | 346 -> 346 | equivalent |
| Notification repeated | 7,841.37 | 7,838.76 | -0.03% | 346 -> 346 | equivalent |

Wall-clock navigation samples were not accepted as a regression signal in the final run. During the run, the host concurrently had an
unrelated Roslyn compiler process above 250% CPU, an unrelated test process around 55% CPU and an unrelated frontend build around 25%
CPU. The untouched Gallery route paths consequently showed large timing drift while their deterministic allocation and shape metrics
remained stable. No unrelated process was stopped for this measurement. Raw post-change reports are:

- `/tmp/atomui-message-gallery-optimized.md`
- `/tmp/atomui-notification-gallery-optimized-current-shape.md`

## Focused Feedback stack probe

The persisted probe command is:

```text
dotnet run --project tools/performances/AtomUI.GalleryPerformance/AtomUI.GalleryPerformance.csproj \
  -c Debug --framework net10.0 --no-build -- \
  --feedback-stack --label optimized-final-cwt --iterations 10 --warmup 3 \
  --markdown /tmp/atomui-feedback-stack-optimized-final-cwt.md
```

Each sample realizes the real manager template and 24 real cards. The toggle phase executes 20 complete expanded/collapsed layout
cycles. Motion is disabled so the probe measures manager, template and layout work rather than compositor wall time. It also fails the
run unless Message collapses to one hit-testable card, Notification collapses to three, and `DestroyAll()` removes every card visual.

| Control | Operation | Mean ms | Median ms | P95 ms | Mean allocated KB |
| --- | --- | ---: | ---: | ---: | ---: |
| Message | Show 24 | 24.60 | 25.30 | 33.60 | 3,947.23 |
| Message | Toggle 20x | 6.80 | 5.55 | 19.40 | 366.94 |
| Message | Destroy 24 | 4.95 | 5.51 | 7.58 | 528.04 |
| Notification | Show 24 | 47.54 | 45.35 | 62.88 | 6,623.80 |
| Notification | Toggle 20x | 5.17 | 4.18 | 8.19 | 324.84 |
| Notification | Destroy 24 | 7.13 | 6.76 | 12.15 | 758.52 |

The absolute wall-clock values are recorded for future same-host comparisons, not compared to the earlier implementation because Stack
did not exist there. Replacing the panel-owned strong transform dictionary with a `ConditionalWeakTable` changed mean allocation by less
than 1 KB for every aggregate operation compared with the preceding optimized run; it removes the removed-card retention edge without a
measurable allocation regression. The deterministic implementation changes reduce timer resources independently of timing noise:

- Message changes from one one-shot timer per finite card to one lazy nearest-deadline scheduler per manager.
- Notification removes two manager polling timers and uses zero scheduler/timer objects for permanent-only items, otherwise one lazy
  nearest-deadline scheduler.
- layout reuses one static full transform plus cached transforms for the two visible scaled Notification layers, and uses no LINQ or
  temporary collection in measure/arrange.
- hidden Notification cards are retained for Ant-compatible expansion but skip hit testing and progress refresh.

## Lifecycle qualification

Automated tests cover the remaining resource contract:

- a controlled monotonic clock proves nearest-deadline scheduling and exact remaining-time pause/resume;
- permanent-only managers prove that no scheduler is created;
- `DestroyAll()` drains all scheduler entries and removes every visual;
- idempotent `Dispose()` cancels the single wakeup and removes its tick delegate;
- WeakReference tests prove manager, presenter, card and user callback owners are collectible both before and after visual attachment;
- a removed card remains collectible while its panel and the panel's weak transform cache stay alive;
- repeated collapsed layout proves the 0.94 and 0.88 transforms are reused by reference.

The complexity burden is three internal shared types (`FeedbackStackPresenter`, `FeedbackStackPanel`, and
`FeedbackLifetimeScheduler`) plus a narrow item interface. There is no public shared base class, runtime reflection, global cache or
static event subscription. This keeps the behavior reusable by both controls while bounding ownership to the manager instance.

## Final verification

- `AtomUI.Desktop.Controls.Tests`: 2,986 passed, 0 failed, including 23 focused Feedback stack tests.
- `AtomUIGallery.Tests`: 436 passed, 0 failed.
- Gallery `osx-arm64` NativeAOT publish: linked registration, restore assets and output validation passed.
- `git diff --check`: passed.

One full-suite run executed concurrently with NativeAOT produced a single failure in the unchanged
`PopupConfirm_In_Dialog_Confirms_And_Closes_Its_Flyout` input test. The test passed three consecutive isolated processes, then the entire
2,986-test suite passed when rerun without the competing NativeAOT build. No production or test change was made for that transient.
