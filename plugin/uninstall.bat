@echo off
title AstroCircular SkyWaver — Uninstaller
echo.
echo  ============================================
echo   AstroCircular SkyWaver — Uninstaller
echo  ============================================
echo.

set "FOUND="

:: Check versioned path first (NINA 3.x)
if exist "%localappdata%\NINA\Plugins\3.0.0\AstroCircular.SkyWaver" (
    echo  Removing: %localappdata%\NINA\Plugins\3.0.0\AstroCircular.SkyWaver
    rmdir /S /Q "%localappdata%\NINA\Plugins\3.0.0\AstroCircular.SkyWaver"
    set "FOUND=1"
)

:: Also check old root path
if exist "%localappdata%\NINA\Plugins\AstroCircular.SkyWaver" (
    echo  Removing: %localappdata%\NINA\Plugins\AstroCircular.SkyWaver
    rmdir /S /Q "%localappdata%\NINA\Plugins\AstroCircular.SkyWaver"
    set "FOUND=1"
)

if not defined FOUND (
    echo  Plugin not found. Nothing to remove.
) else (
    echo.
    echo  [OK] AstroCircular SkyWaver uninstalled.
)
echo.
pause
