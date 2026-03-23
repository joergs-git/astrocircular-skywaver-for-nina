# Collimation Helper for SkyWave

A [N.I.N.A.](https://nighttime-imaging.eu/) plugin that automates telescope collimation data capture for [InnovationForesight's SkyWave](https://www.innovationsforesight.com/aitelescopecollimation/) AI wavefront analysis.

## Why this exists

Precise telescope collimation is critical for sharp, aberration-free images — but getting there has always been tedious. The traditional workflow means manually slewing to a star, defocusing, capturing a frame, then repeating at multiple field positions, often juggling between NINA sequences, PixInsight scripts, and manual mount control. Most people give up after a single on-axis star and never detect the field-dependent problems lurking at the edges of their sensor.

[SkyWave](https://www.innovationsforesight.com/aitelescopecollimation/) by InnovationForesight changed the game: it uses AI-based wavefront sensing (AIWFS) to analyze a defocused star image and extract the telescope's full wavefront error — coma, astigmatism, tilt, spacing issues, field curvature, and more — all from a single FITS file, in seconds, on a standard laptop.

## What this plugin does differently

Instead of capturing just one defocused star at the center of the sensor, this plugin captures the star at **multiple positions across the full sensor plane** in a circular ring pattern and integrates them into a single FITS. This gives SkyWave the data it needs to measure **field-dependent aberrations** — the kind you only see off-axis:

- **Sensor tilt** — uneven focus plane across the field
- **Spacing errors** — incorrect backfocus causing off-axis aberrations
- **Off-axis coma & astigmatism** — collimation errors invisible at the center
- **Field curvature** — the natural curve of the focal plane

A single centered star cannot reveal these. Spreading the defocused donut across the entire sensor makes the difference between "collimated on-axis" and "truly collimated across the whole field."

The plugin handles everything automatically — slew, center, filter change, optional autofocus, defocus, circular capture, integration, and refocus — in one click.

## How it works

1. **Select** a bright, isolated star (from 16 built-in presets or manual RA/Dec)
2. **Switch to L filter** and **plate-solve & center** on the star (in focus)
3. **Switch to target filter** (e.g. R, G, B) for capture
4. **Optionally run autofocus** — a dialog asks before defocusing (works with NINA's built-in AF or Hocusfocus)
5. **Defocus** by a configurable number of focuser steps
6. **Capture** exposures at N positions around a circular ring pattern
7. **Integrate** sub-frames natively (simple average, optional square crop, optional bin 2x)
8. **Save** a 16-bit monochrome FITS to your configured output folder
9. **Refocus** — always returns the focuser, even on failure

<img width="1182" height="719" alt="Collimation Helper for SkyWave" src="https://github.com/joergs-git/Skywave-Collimation-helper-for-NINA/blob/main/astrocirular-skw-nina-helper.png" />

## Two tools, one workflow

### NINA Plugin — Collimation Helper for SkyWave

A native N.I.N.A. plugin that does everything inside NINA — no external tools required:

- **Dockable tool panel** in NINA's imaging tab — click "Run Collimation" and it does everything
- **Star picker** with 16 presets and "Find Best" auto-selection based on time and location
- **Live sensor map** showing ring positions with progress (grey=pending, red=active, green=done)
- **Camera preview** of each captured frame with auto-stretch
- **Native FITS integration** — simple pixel average, no alignment, no rejection, no normalization
- **Optional autofocus** before defocusing — works with any AF provider
- **All settings persist** between NINA sessions
- **Slow mode** for inaccurate mounts — plate-solves every position

#### Installation

1. Download the zip from [Releases](https://github.com/joergs-git/Skywave-Collimation-helper-for-NINA/releases)
2. Unzip and double-click `install.bat` (close NINA first)
3. Restart N.I.N.A. — find **Collimation Helper for SkyWave** in the tool panels

#### Usage

1. Open the **Collimation Helper for SkyWave** panel (imaging tab, tool windows)
2. Select a star from the presets or enter RA/Dec manually
3. Set defocus steps, exposure time, filter, gain, positions, radius
4. Set the output folder via the `...` browse button
5. Click **Run Collimation**
6. The integrated FITS appears in your output folder, ready for SkyWave

### Web Tool (HTML) — for non-NINA users

A standalone browser-based tool that generates N.I.N.A. sequence files and PixInsight scripts. This tool remains available for users who prefer to build their own sequences or don't use the plugin:

- **[Live demo](https://joergs-git.github.io/Skywave-Collimation-helper-for-NINA/)** — runs entirely in your browser
- Generates downloadable `.json` for N.I.N.A. Advanced Sequencer
- Generates downloadable `.js` PixInsight integration script
- Star finder with altitude/LST calculator
- Magnitude advisor based on your optical setup

## Star presets

| Star | RA | Dec | Mag | Season | Notes |
|------|-----|-----|-----|--------|-------|
| θ Boo | 14:25:11.8 | +51:51:03 | 4.05 | Spring | Very isolated, ideal near zenith 52°N |
| κ Dra | 12:33:28.9 | +69:47:18 | 3.87 | Spring | Extremely clean field |
| 42 Dra | 18:25:59.1 | +65:33:49 | 4.82 | Summer | Recommended! Perfect mag, extremely isolated |
| χ Dra | 18:21:03.4 | +72:43:58 | 3.57 | Summer | Circumpolar, clean field |
| ξ Cep | 22:03:47.5 | +64:37:41 | 4.29 | Fall | Away from Milky Way |
| α Cam | 04:54:03.0 | +66:20:34 | 4.29 | Winter | Recommended! Sparsest field in the sky |

See the full list of 16 presets in the [web tool](https://joergs-git.github.io/Skywave-Collimation-helper-for-NINA/).

## Tips

- **Camera rotation:** Set your camera to 0° or 180° rotation to avoid confusion with mirrored orientation in the integrated image. Since we capture in a circle, rotation doesn't affect collimation quality — it just makes visual interpretation easier.
- **Center position first:** The plugin always captures the center star position first (if enabled), then the ring positions. This matches SkyWave's expectation for field-dependent analysis.
- **Slow mode:** Enable this if your mount is not accurate enough for blind slewing. In slow mode, the plugin refocuses and plate-solves at every single ring position — much slower but ensures precise positioning. The workflow per position is: refocus → L filter → slew & center (plate-solve) → defocus → target filter → expose. Default is off (blind slew after initial centering).
- **Integration:** The integration is a simple pixel-by-pixel average — no alignment, no rejection, no normalization, no weighting. This is by design: each frame shows the defocused star at a different field position, and SkyWave needs the raw combined pattern.
- **Output format:** Always 16-bit unsigned FITS with proper headers (FOCALLEN, XPIXSZ, XBINNING, etc.). Never XISF — regardless of NINA's default format setting.
- **Sub-frames:** When "Auto-delete subs" is off, individual frames are kept in a `subframes_*` subfolder inside your output directory.

## Requirements

- **N.I.N.A. 3.0+** (.NET 8.0)
- GoTo mount with slew capability
- Electronic focuser
- Camera with FITS output
- Filter wheel (optional — for L filter plate-solving and target filter capture)
- [SkyWave Collimator](https://www.innovationsforesight.com/aitelescopecollimation/) for wavefront analysis

## License

[GPL-3.0](LICENSE)

---
If you find this useful, consider supporting my work via [Buy Me a Coffee](https://buymeacoffee.com/joergsflow)
