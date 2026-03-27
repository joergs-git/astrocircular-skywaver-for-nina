# Changelog

All notable changes to Collimation Helper for SkyWave will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.0.1] - 2026-03-27

### Fixed
- Plugin not loading after install from NINA plugin manager — ZIP archive now contains the DLL at the root level instead of inside a subfolder

### Changed
- CI now uses NINA's official `CreateManifest.ps1` script for manifest and archive generation
- All plugin metadata (descriptions, tags, URLs) moved into AssemblyInfo.cs assembly attributes as single source of truth

## [1.0.0] - 2026-03-27

First stable release for the official N.I.N.A. plugin repository.

### Added
- MAX stacking (each pixel keeps its maximum value across all frames)
- 22 star presets (mag 2–5, all seasons, mid-northern latitudes)
- "Find Best" star auto-selection based on time, location, and optical setup
- Magnitude advisor targeting ~60% ADU fill
- Steps/µm readout for focuser calibration
- Optional crop to ring pattern + 300px safety margin
- Optional autofocus before defocusing
- Camera preview with robust auto-stretch (median + MAD)
- Live sensor map with ring position progress
- Full cancellation support with focus/filter restoration
- Browser-based collimation helper for non-NINA users
- Plugin screenshots in README

### Changed
- Stacking mode changed from average to MAX — preserves every donut at full brightness
- Crop safety margin tripled from 100px to 300px for defocused stars on long sensor sides
- Star catalog expanded from 16 to 22 presets with brighter stars (down to mag 2)
- Renamed "Web Tool" / "Live demo" to "Browser-based Collimation Helper"

## [0.1.0] - 2026-03-22

### Added
- **HTML Tool (v5):** Web-based SKW Collimation Coordinator
  - Generates N.I.N.A. Advanced Sequencer JSON files for circular defocused star capture
  - Generates PixInsight integration scripts for combining sub-frames
  - 16 embedded star presets for all seasons (mid-northern latitudes)
  - Star finder with LST/altitude calculation and nautical dusk timing
  - Magnitude advisor based on optical setup
  - Sensor preview visualization with ring positions
  - Position table with RA/Dec coordinates

- **NINA Plugin (v0.1.0):** Native N.I.N.A. plugin scaffolding
  - `SkwDefocus` — Move focuser by configurable steps (extra/intra-focal)
  - `SkwCircularCapture` — Slew to N ring positions and capture defocused exposures
  - `SkwIntegrateFrames` — Native FITS averaging, crop-to-circle, bin 2x, 16-bit output
  - `SkwCollimationRun` — Full orchestrated workflow with try/finally refocus
  - Plugin options page with telescope, sensor, location, and imaging defaults
  - Star catalog with 16 embedded presets and FindBestStar algorithm
  - Coordinate utilities (HMS/DMS, LST, altitude, nautical dusk)
  - SVG icon with SkyWave-style wavefront rings and defocused star donuts
  - GitHub Actions CI for Windows builds

- **Project setup**
  - .gitignore, CHANGELOG, tasks/lessons.md, tasks/todo.md
  - GitHub Pages deployment for HTML tool
  - GPL-3.0 license
