# Collimation Helper for SkyWave — TODO

## Completed
- [x] Review HTML tool for correctness and NINA format compatibility
- [x] Plan native NINA plugin architecture
- [x] Plugin scaffolding — solution, csproj, AssemblyInfo, manifest
- [x] GitHub Actions CI + release zip with installer
- [x] Core math ported from JS (coordinates, ring calc, star catalog, magnitude)
- [x] Sequence instructions: SkwDefocus, SkwCircularCapture, SkwIntegrateFrames, SkwCollimationRun
- [x] Native FITS integration (FitsAverager, RawFitsWriter, RawFitsReader, FitsHeaderWriter)
- [x] Options page with settings UI
- [x] Installer fixed for NINA 3.x versioned plugin path (Plugins\3.0.0\)
- [x] Plugin loads and displays in NINA
- [x] v0.2.0: Dockable panel (SkyWave Collimator Helper) in imaging tab
- [x] v0.2.0: One-click Run Collimation with direct equipment control
- [x] v0.2.0: Star picker with 16 presets and Find Best
- [x] v0.2.0: Settings persistence via PluginOptionsAccessor
- [x] v0.2.0: Folder browser button for output directory
- [x] v0.2.0: Sensor map with position progress indicators (grey/red/green)
- [x] v0.2.0: Camera preview with auto-stretch
- [x] v0.2.0: Plate-solve centering (L filter) for first slew
- [x] v0.2.0: Target filter switch after defocus
- [x] v0.2.0: ~~Slow mode~~ removed — slew-and-center makes no sense for off-center ring positions
- [x] v0.2.0: Self-contained FITS pipeline (RawFitsWriter/Reader, no NINA format dependency)
- [x] v0.2.0: Rectangular crop (no circular masking)
- [x] v0.2.0: All toggles in dockable panel with visible labels
- [x] v0.2.0: Equipment auto-read from NINA profile
- [x] v0.2.0: WPF geometry icon

## Open — Go Live (v0.3.0)
- [ ] **Publish to NINA plugin repository** — manifest.json is prepared in
  repo root with placeholder SHA256. To go live:
  1. Create stable release tag `v0.2.0`
  2. Download zip, generate SHA256 checksum
  3. Update manifest.json with checksum and stable download URL
  4. Fork github.com/isbeorn/nina.plugin.manifests
  5. Place at `manifests/c/CollimationHelperForSkyWave/manifest.0.2.0.1.json`
  6. Submit PR (validation: `npm install && node gather.js`)
- [ ] **PixInsight Tools integration** — Optional integration path using
  isbeorn's PixInsight Tools plugin for stacking
- [ ] **Verify output files** — Confirm integrated FITS loads correctly in SkyWave
- [x] ~~Test slow mode~~ — removed, not applicable (ring positions are intentionally off-center)

## Open — HTML Tool Fixes
- [ ] Fix TakeExposure.ExposureCount: 0 → 1 in generated NINA JSON
- [ ] Fix PixInsight script: use windowById instead of activeWindow
- [ ] Fix PixInsight script: update XPIXSZ/XBINNING headers after bin2

## Key Learnings
- NINA 3.x plugins must go in `Plugins\3.0.0\` (one-time migration from root)
- ResourceDictionary: use programmatic pack URI loading, NOT x:Class/InitializeComponent
  (but dockable DataTemplates CAN use x:Class — different MEF export path)
- No WPF Hyperlink elements (crash plugin load without RequestNavigate handler)
- FilterInfo constructor needs (string, int, short) — cast position to short
- BinningMode constructor needs (short, short) — cast binning to short
- Filter switch must look up by name from profile's FilterWheelFilters list
- Options page Settings object is disconnected from panel VM — all persisted
  settings should be on the panel VM via PluginOptionsAccessor
- NINA's SaveToDisk may change filenames/formats — use own RawFitsWriter instead
- CheckBox Content text is invisible in NINA's dark theme — use explicit TextBlock

## Results
- v0.1.0 (2026-03-22): Plugin scaffolding, sequence instructions, CI, installer
- v0.2.0 (2026-03-23): Dockable panel, direct equipment control, self-contained FITS pipeline
