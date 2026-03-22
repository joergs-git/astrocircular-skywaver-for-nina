using System.Reflection;
using System.Runtime.InteropServices;

// General Information
[assembly: AssemblyTitle("AstroCircular SkyWaver")]
[assembly: AssemblyDescription("Automated SkyWave (SKW) telescope collimation for N.I.N.A. — circular defocused star pattern capture, native frame integration, and SkyWave-ready FITS output.")]
[assembly: AssemblyCompany("joergsflow")]
[assembly: AssemblyProduct("AstroCircular SkyWaver for N.I.N.A.")]
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
[assembly: AssemblyMetadata("Repository", "https://github.com/joergs-git/astrocircular-skywaver-for-nina")]
[assembly: AssemblyMetadata("Homepage", "https://joergs-git.github.io/astrocircular-skywaver-for-nina/")]
[assembly: AssemblyMetadata("Tags", "Collimation,SkyWave,SKW,Wavefront,Defocus,Optics")]
[assembly: AssemblyMetadata("ShortDescription", "Automated SkyWave collimation — circular pattern capture & native FITS integration")]
[assembly: AssemblyMetadata("LongDescription",
@"AstroCircular SkyWaver automates the complete SkyWave (SKW) telescope collimation workflow inside N.I.N.A.:

1. Select a bright, isolated star (from presets or manual RA/Dec)
2. Plate-solve and center on the star (in focus)
3. Defocus the telescope by a configurable number of focuser steps
4. Capture exposures at N positions around a circular pattern
5. Natively integrate sub-frames (average, crop, bin 2×) into a single FITS
6. Save the SkyWave-ready FITS to your configured watch folder
7. Return the focuser to its original position

No external tools required — replaces the manual workflow of generating NINA sequences and running PixInsight scripts.

Provides both individual sequence instructions (SkwDefocus, SkwCircularCapture, SkwIntegrateFrames) for advanced users and a single SkwCollimationRun container for one-click operation.")]
