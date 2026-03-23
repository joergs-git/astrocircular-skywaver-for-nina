using System.Reflection;
using System.Runtime.InteropServices;

// General Information
[assembly: AssemblyTitle("Collimation Helper for SkyWave")]
[assembly: AssemblyDescription("Automated telescope collimation helper for InnovationForesight's SkyWave — circular defocused star pattern capture across the full sensor plane, native frame integration, and wavefront-ready FITS output.")]
[assembly: AssemblyCompany("joergsflow")]
[assembly: AssemblyProduct("Collimation Helper for SkyWave")]
[assembly: AssemblyCopyright("Copyright © joergsflow 2026")]

// COM visibility
[assembly: ComVisible(false)]

// Unique plugin identifier — NEVER change this after first publish
[assembly: Guid("b7e3f1a2-9c4d-4e8b-a6f5-1d2c3b4a5e6f")]

// Version: MAJOR.MINOR.PATCH.CHANNEL (9=release, 2=beta, 1=nightly)
[assembly: AssemblyVersion("0.2.0.1")]
[assembly: AssemblyFileVersion("0.2.0.1")]

// NINA plugin metadata
[assembly: AssemblyMetadata("MinimumApplicationVersion", "3.0.0.9001")]
[assembly: AssemblyMetadata("License", "GPL-3.0")]
[assembly: AssemblyMetadata("LicenseURL", "https://www.gnu.org/licenses/gpl-3.0.html")]
[assembly: AssemblyMetadata("Repository", "https://github.com/joergs-git/SkyWave-Collimation-Helper-for-NINA")]
[assembly: AssemblyMetadata("Homepage", "https://joergs-git.github.io/SkyWave-Collimation-Helper-for-NINA/")]
[assembly: AssemblyMetadata("Tags", "Collimation,SkyWave,SKW,Wavefront,Defocus,Optics,Tilt")]
[assembly: AssemblyMetadata("ShortDescription", "Automated SkyWave collimation helper — circular defocused star capture & native FITS integration")]
[assembly: AssemblyMetadata("LongDescription",
@"Collimation Helper for SkyWave automates the complete telescope collimation data-capture workflow inside N.I.N.A., producing wavefront-ready FITS images for InnovationForesight's SkyWave AI analysis:

1. Select a bright, isolated star (from 16 presets or manual RA/Dec)
2. Plate-solve and center on the star (in focus)
3. Switch to the target filter and optionally run autofocus
4. Defocus the telescope by a configurable number of focuser steps
5. Capture exposures at N positions around a circular ring pattern covering the full sensor plane
6. Natively integrate sub-frames (average, crop, bin 2x) into a single FITS
7. Save the SkyWave-ready FITS to your configured watch folder
8. Return the focuser to its original position

By spreading the defocused star across the entire sensor, SkyWave can detect not just on-axis collimation errors but also field-dependent aberrations like tilt, spacing issues, and off-axis coma — something a single centered star image cannot reveal.

Provides both individual sequence instructions (SkwDefocus, SkwCircularCapture, SkwIntegrateFrames) for advanced users and a single SkwCollimationRun container for one-click operation.")]
