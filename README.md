# Collimation Helper for SkyWave

A [N.I.N.A.](https://nighttime-imaging.eu/) plugin that automates telescope collimation data capture for [InnovationForesight's SkyWave](https://www.innovationsforesight.com/aitelescopecollimation/) AI wavefront analysis.

## Contents

- [Background — SkyWave & AI Wavefront Sensing](#background--skywave--ai-wavefront-sensing)
- [Why this plugin exists](#why-this-plugin-exists)
- [What this plugin does differently](#what-this-plugin-does-differently)
- [How it works](#how-it-works)
- [Two tools, one workflow](#two-tools-one-workflow)
  - [NINA Plugin](#nina-plugin--collimation-helper-for-skywave)
  - [Web Tool (HTML)](#web-tool-html--for-non-nina-users)
- [Star presets](#star-presets)
- [Tips](#tips)
- [Requirements](#requirements)
- [License](#license)

## Background — SkyWave & AI Wavefront Sensing

[SkyWave](https://www.innovationsforesight.com/aitelescopecollimation/) is a desktop application by [InnovationForesight](https://www.innovationsforesight.com/) that uses **AI-based wavefront sensing (AIWFS)** to analyze telescope optics. You feed it a defocused star image (FITS), and SkyWave extracts the telescope's full wavefront error — coma, astigmatism, tilt, spacing issues, field curvature, and more — all in seconds, on a standard laptop.

What makes SkyWave unique is that it doesn't just tell you "your collimation is off." It quantifies **how much** and **in which direction**, showing Zernike coefficients, wavefront maps, and actionable correction vectors. For telescopes with adjustable optics (SCTs, RCs, Newtonians, refractors with tilt adjusters), this turns collimation from guesswork into a measured, repeatable process.

SkyWave can work with a single on-axis star, but its real power emerges when it receives defocused star data from **multiple field positions** — that's where this plugin comes in.

## Why this plugin exists

Precise telescope collimation is critical for sharp, aberration-free images — but getting there has always been tedious. The traditional workflow means manually slewing to a star, defocusing, capturing a frame, then repeating at multiple field positions, often juggling between NINA sequences, PixInsight scripts, and manual mount control. Most people give up after a single on-axis star and never detect the field-dependent problems lurking at the edges of their sensor.

## What this plugin does differently

Instead of capturing just one defocused star at the center of the sensor, this plugin captures the star at **multiple positions across the full sensor plane** in a circular ring pattern and integrates them into a single FITS. This gives SkyWave the data it needs to measure **field-dependent aberrations** — the kind you only see off-axis:

- **Sensor tilt** — uneven focus plane across the field
- **Spacing errors** — incorrect backfocus causing off-axis aberrations
- **Off-axis coma & astigmatism** — collimation errors invisible at the center
- **Field curvature** — the natural curve of the focal plane

A single centered star cannot reveal these. Spreading the defocused donut across the entire sensor makes the difference between "collimated on-axis" and "truly collimated across the whole field."

The plugin handles everything automatically — slew, center, filter change, optional autofocus, defocus, circular capture, MAX-stacking, optional crop, and refocus — in one click.

## How it works

1. **Select** an isolated star of appropriate magnitude (from 22 built-in presets or manual RA/Dec — "Find Best" auto-selects based on your optics to target ~60% ADU without overexposure)
2. **Switch to L filter** and **plate-solve & center** on the star (in focus)
3. **Switch to target filter** (e.g. R, G, B) for capture
4. **Optionally run autofocus** — a dialog asks before defocusing (works with NINA's built-in AF or Hocusfocus)
5. **Defocus** by a configurable number of focuser steps (with steps/µm readout for your focuser)
6. **Capture** exposures at N positions around a circular ring pattern (blind slews — no plate-solving while defocused)
7. **MAX-stack** sub-frames — each pixel keeps its maximum value across all frames, so every defocused donut shines through without dilution
8. **Optionally crop** the integrated image to the ring pattern bounding box + 300px safety margin
9. **Save** a 16-bit monochrome FITS with proper headers (FOCALLEN, XPIXSZ, XBINNING, etc.) to your configured output folder
10. **Refocus** — always returns the focuser and restores the original filter, even on failure or cancel

<img width="1182" height="719" alt="Collimation Helper for SkyWave" src="https://github.com/joergs-git/Skywave-Collimation-helper-for-NINA/blob/main/astrocirular-skw-nina-helper.png" />

## Two tools, one workflow

### NINA Plugin — Collimation Helper for SkyWave

A native N.I.N.A. plugin that does everything inside NINA — no external tools required:

- **Dockable tool panel** in NINA's imaging tab — click "Run Collimation" and it does everything
- **Star picker** with 22 presets (mag 2–5, all seasons) and **"Find Best"** auto-selection based on time, location, and optical setup
- **Magnitude advisor** — computes ideal star brightness from your focal length, aperture, exposure, and gain to target ~60% ADU fill
- **Steps/µm readout** — enter your focuser's steps-per-micron and see real defocus distance in µm
- **Live sensor map** showing ring positions with progress (grey=pending, red=active, green=done)
- **Camera preview** of each captured frame with auto-stretch (median + MAD robust statistics)
- **MAX stacking** — each pixel keeps its maximum value, preserving every donut across the field
- **Optional crop** — trims the integrated image to the ring pattern + 300px margin (off by default — SkyWave may need full sensor dimensions)
- **Native FITS output** — always 16-bit unsigned with correct headers, regardless of NINA's default format setting
- **Optional autofocus** before defocusing — works with any AF provider (NINA built-in, Hocusfocus, etc.)
- **Full cancellation support** — focus and filter always restored on cancel or error
- **All settings persist** between NINA sessions

#### Settings overview

| Setting | Default | Description |
|---------|---------|-------------|
| Exposure (s) | 8.0 | Capture duration per position |
| Gain | 100 | Camera gain |
| Offset | 0 | Camera offset/bias |
| Defocus steps | 2442 | Focuser steps to defocus |
| Steps/µm | 3.0 | Focuser calibration (e.g. ZWO EAF = 3.0) |
| Ring positions | 8 | Number of positions on the circle |
| Radius % | 80 | Ring radius as percentage of FOV |
| Include center | On | Add center position as first capture |
| Settle time (s) | 3 | Pause after slew before exposure |
| Filter | L | Capture filter (L, R, G, B, Ha, etc.) |
| Crop | Off | Crop to ring pattern + 300px margin |
| Del subs | Off | Auto-delete individual sub-frames after stacking |
| AF first | Off | Run autofocus before defocusing |

#### Installation

1. Download the zip from [Releases](https://github.com/joergs-git/Skywave-Collimation-helper-for-NINA/releases)
2. Unzip and double-click `install.bat` (close NINA first)
3. Restart N.I.N.A. — find **Collimation Helper for SkyWave** in the tool panels

#### Usage

1. Open the **Collimation Helper for SkyWave** panel (imaging tab, tool windows)
2. Select a star from the presets or click **Find Best** for automatic selection
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

See the full list of 22 presets (mag 2–5) in the [web tool](https://joergs-git.github.io/Skywave-Collimation-helper-for-NINA/).

## Tips

- **Camera rotation:** Set your camera to 0° or 180° rotation to avoid confusion with mirrored orientation in the integrated image. Since we capture in a circle, rotation doesn't affect collimation quality — it just makes visual interpretation easier.
- **Center position first:** The plugin always captures the center star position first (if enabled), then the ring positions. This matches SkyWave's expectation for field-dependent analysis.
- **MAX stacking:** The integration uses pixel-by-pixel MAX stacking — no alignment, no rejection, no normalization. Each frame shows the defocused star at a different field position. MAX mode ensures every donut ring is preserved at full brightness without dilution from empty areas.
- **Output format:** Always 16-bit unsigned FITS with proper headers (FOCALLEN, XPIXSZ, XBINNING, etc.). Never XISF — regardless of NINA's default format setting.
- **Sub-frames:** When "Del subs" is off, individual frames are kept in a `subframes_*` subfolder inside your output directory.
- **Bin 2 pixel size:** If you capture at bin 2, remember that your effective pixel size doubles. Enter your native (bin 1) pixel size in NINA's camera settings — the plugin handles the FITS header math.
- **Focuser calibration:** Use the steps/µm field to verify your defocus amount in real physical units. Check your focuser's specs — e.g. ZWO EAF is ~3 steps/µm, Moonlite is typically ~1 step/µm.
- **Blind slews:** Ring positions are reached via blind SlewToRaDec — no plate-solving while defocused. This is by design: the telescope stays defocused throughout the ring capture, and plate-solving defocused stars is unreliable.

## Requirements

- **N.I.N.A. 3.0+** (.NET 8.0)
- GoTo mount with slew capability
- Electronic focuser
- Camera with FITS output
- Filter wheel (optional — for L filter plate-solving and target filter capture)
- [SkyWave](https://www.innovationsforesight.com/aitelescopecollimation/) by InnovationForesight for wavefront analysis

## License

[GPL-3.0](LICENSE)

---
If you find this useful, consider supporting my work via [Buy Me a Coffee](https://buymeacoffee.com/joergsflow)
