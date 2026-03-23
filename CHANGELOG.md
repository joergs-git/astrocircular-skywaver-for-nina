# Changelog

All notable changes to Collimation Helper for SkyWave will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/).

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
