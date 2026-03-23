@echo off
title Collimation Helper for SkyWave — Uninstaller
echo.
echo  ============================================
echo   Collimation Helper for SkyWave — Uninstaller
echo  ============================================
echo.

set "FOUND="

:: Check new name (versioned path, NINA 3.x)
if exist "%localappdata%\NINA\Plugins\3.0.0\CollimationHelper.SkyWave" (
    echo  Removing: %localappdata%\NINA\Plugins\3.0.0\CollimationHelper.SkyWave
    rmdir /S /Q "%localappdata%\NINA\Plugins\3.0.0\CollimationHelper.SkyWave"
    set "FOUND=1"
)

:: Check new name (root path)
if exist "%localappdata%\NINA\Plugins\CollimationHelper.SkyWave" (
    echo  Removing: %localappdata%\NINA\Plugins\CollimationHelper.SkyWave
    rmdir /S /Q "%localappdata%\NINA\Plugins\CollimationHelper.SkyWave"
    set "FOUND=1"
)

:: Also clean up old AstroCircular.SkyWaver name if still present
if exist "%localappdata%\NINA\Plugins\3.0.0\AstroCircular.SkyWaver" (
    echo  Removing old installation: %localappdata%\NINA\Plugins\3.0.0\AstroCircular.SkyWaver
    rmdir /S /Q "%localappdata%\NINA\Plugins\3.0.0\AstroCircular.SkyWaver"
    set "FOUND=1"
)
if exist "%localappdata%\NINA\Plugins\AstroCircular.SkyWaver" (
    echo  Removing old installation: %localappdata%\NINA\Plugins\AstroCircular.SkyWaver
    rmdir /S /Q "%localappdata%\NINA\Plugins\AstroCircular.SkyWave"
    set "FOUND=1"
)

if not defined FOUND (
    echo  Plugin not found. Nothing to remove.
) else (
    echo.
    echo  [OK] Collimation Helper for SkyWave uninstalled.
)
echo.
pause
