# Splash Changelog

本文档记录 Splash 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-09-16

- Semantic Part
  - Publish ten Semantic Parts: implicit `root` plus `logo`, `title`, `subtitle`, `content`, `spin`, `progressBar`, `message`, `detail` and `footer`; all `Single` / `Selector` / `RuntimeCreated=false` / `CrossVisualRoot=false` / `Since=6.2.0`.
  - Add `Splash.SemanticParts.cs` with the `[SemanticPart]` declarations and the nine `Classes.semantic-*="True"` static markers in `SplashTheme.axaml`; no existing theme selector changed, so default visuals and Token consumers are unaffected.
  - Keep the progress area as two parts instead of one shared `Multiple` part, because `Spin` and `ProgressBar` are different controls with different tokens and mutually exclusive visibility; merging them would widen `ContractType` to `TemplatedControl` and drop the `x:SetterTargetType` context.
  - Exclude `PART_RootLayout`, `PART_SurfaceLayout`, `PART_ContentLayout` and `PART_ProgressLayout`: the surface properties are already projected from owner `Background` / `CornerRadius` / `Padding` via `TemplateBinding`, and the two remaining nodes are pure layout wrappers whose spacing comes from tokens.
  - Do not publish `SplashWindow` as a semantic owner: the window shell is already public API, the surface shadow and host corner radius are token semantics owned by `SplashWindow.Resources` or a dedicated subclass, and `ShadowsAwareContainer` is internal.
  - Splash is the first control in `AtomUI.Desktop.Controls.Extras` to adopt Semantic Parts, so this package now emits its first semantic descriptor, generated `Splash*Style` types and the `AtomUI.Theme.Styling` XML namespace mapping.
- Gallery
  - Move the dedicated window splash visual from `/template/` + `PART_*` selectors to the generated Semantic Part styles (`SplashTitleStyle`, `SplashSubtitleStyle`, `SplashDetailStyle`, `SplashMessageStyle`), removing the template penetration that the documented boundary forbids.
  - Add a Semantic Parts preview tab covering all ten parts. Because `spin` and `progressBar` are mutually exclusive in visibility and the preview skips invisible targets, the stage hosts two instances pinned to the determinate and indeterminate states.
  - Fix the preview stage layout for those two instances: they must sit in two equal grid columns so the composition can never wrap to a second row. The single-preview tab clamps its content height to the page viewport remainder, so any stacked composition pushes the second instance past `PART_PreviewContentHost` and the clipped part silently disappears (on a real display it looked like a stray white strip under the preview card).
  - Add a "Custom Semantic Part styling" example item that styles the parts through generated styles in AXAML, with no code-behind fallback.
  - Make the example logo templates fill the `logo` presenter instead of carrying their own fixed size: the part style sizes the presenter, so a fixed-size template graphic (48 inside a 36 presenter) overflowed it and the setter had no visible effect. Documented the coordination rule in `semantic-part.md` §2.2 and asserted the visible logo bounds, not just the presenter's `Width`/`Height`.
- Docs
  - Add `semantic-part.md` with the full per-part contract, selector usage, state matrix, sizing baseline audit and customization boundaries.
  - Align the `overview.md` semantic region table with the descriptor: the former coarse `brand` / `status` / `progress` grouping could not support per-part customization because it merged nodes with different tokens.

## 2026-08-03

- Theme
  - Route title and ordinary message foregrounds through `SplashTokenResource`, so Splash Control-level `ColorTextHeading` and `ColorText` overrides are consumed through the existing Effective Global Token mechanism.
  - Bind the Splash surface template to the control's existing `Background`, `Padding` and `CornerRadius` properties, preserving Token-backed defaults while allowing derived themes to customize the surface through normal setters.
  - Keep Success/Error message foregrounds on the existing `SuccessColor` and `ErrorColor` Own Tokens, preserving current state semantics and default rendering.
- Gallery
  - Move the window showcase visual into a dedicated `GallerySplashWindow` and a `GalleryWindowSplash` that owns its AXAML `Styles`.
  - Remove window resource-key overrides and runtime template selectors from the Gallery service; the service now only creates the dedicated window.
  - Establish that pages, parent styles and window themes must not cross the Splash template boundary, while a dedicated Splash child may reuse the standard Theme through `StyleKeyOverride` and own its single template boundary.
- Docs
  - Document the Splash Effective Global Token, Own Token, dedicated-window customization and template ownership boundaries without adding new Splash public API or duplicate foreground Own Tokens.

## 2026-06-28

- Code
  - Add the first desktop Splash implementation in `AtomUI.Desktop.Controls.Extras`.
  - Add `Splash`, `SplashWindow`, `ISplashService`, `SplashService`, `SplashOptions`, `SplashStatus` and Splash theme resources.
  - Add `UseDesktopExtras()` theme registration and generated Splash token resource keys.
  - Add Splash behavior tests for visual state, progress normalization, controller updates, static API delegation and idempotent close.
  - Add the Gallery showcase under `controlgallery/AtomUIGallery/ShowCases/Other/Splash`.
  - Move the Splash window shell, surface shadow and host corner radius to `SplashWindowTheme.axaml` with `ShadowsAwareContainer`.
  - Remove the C# surface host/token bridge so window-level Splash visual overrides resolve from `SplashWindow.Resources`.
  - Move state update methods from `SplashController` into `Splash` and remove the standalone controller type.
  - Refine `SplashWindow` property contracts so `Splash` is a nullable styled content property, timing options are styled properties and close-request state remains direct runtime state.

## 2026-06-27

- Docs
  - Establish Splash desktop architecture documentation under `docs/controls/desktop/feedback/splash/overview.md`.
  - Add Splash implementation documentation covering visual control, window host, service orchestration, static API and AOT boundaries.
  - Add Splash Token documentation covering startup surface, branding, progress, status and host visual variables.
  - Add Splash control-level changelog.
