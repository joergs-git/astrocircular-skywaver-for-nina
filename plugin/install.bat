@echo off
title Collimation Helper for SkyWave — Installer
echo.
echo  ============================================
echo   Collimation Helper for SkyWave
echo   Plugin Installer v0.2.0
echo   by joergsflow
echo  ============================================
echo.

set "SCRIPT_DIR=%~dp0"
set "DLL="

:: Find the DLL
if exist "%SCRIPT_DIR%CollimationHelper.SkyWave\NINA.CollimationHelper.SkyWave.dll" (
    set "DLL=%SCRIPT_DIR%CollimationHelper.SkyWave\NINA.CollimationHelper.SkyWave.dll"
    goto :found_dll
)
if exist "%SCRIPT_DIR%NINA.CollimationHelper.SkyWave.dll" (
    set "DLL=%SCRIPT_DIR%NINA.CollimationHelper.SkyWave.dll"
    goto :found_dll
)

echo  [ERROR] Cannot find NINA.CollimationHelper.SkyWave.dll
echo.
pause
goto :eof

:found_dll
echo  DLL: %DLL%

:: NINA 3.x uses Plugins\3.0.0\ after first-run migration
:: Try versioned path first, then fall back to root Plugins
set "TARGET="
if exist "%localappdata%\NINA\Plugins\3.0.0" (
    set "TARGET=%localappdata%\NINA\Plugins\3.0.0\CollimationHelper.SkyWave"
    goto :found_nina
)
if exist "%localappdata%\NINA\Plugins" (
    set "TARGET=%localappdata%\NINA\Plugins\CollimationHelper.SkyWave"
    goto :found_nina
)

echo.
echo  Could not find NINA Plugins folder.
echo  Enter the full path to your NINA Plugins folder:
echo.
set /p "TARGET_BASE=  Path: "
set "TARGET=%TARGET_BASE%\CollimationHelper.SkyWave"

:found_nina
echo  Target: %TARGET%
echo.

if exist "%TARGET%\NINA.CollimationHelper.SkyWave.dll" (
    echo  Existing installation found, updating...
    echo.
)

if not exist "%TARGET%" mkdir "%TARGET%"

copy /Y "%DLL%" "%TARGET%\" >NUL 2>&1
if errorlevel 1 (
    echo  [ERROR] Copy failed. Close N.I.N.A. first and retry.
    echo.
    pause
    goto :eof
)

:: Clean up old AstroCircular.SkyWaver installation if it exists
if exist "%localappdata%\NINA\Plugins\3.0.0\AstroCircular.SkyWaver\NINA.AstroCircular.SkyWaver.dll" (
    echo  Cleaning up old AstroCircular.SkyWaver installation...
    del /Q "%localappdata%\NINA\Plugins\3.0.0\AstroCircular.SkyWaver\NINA.AstroCircular.SkyWaver.dll" >NUL 2>&1
    rmdir "%localappdata%\NINA\Plugins\3.0.0\AstroCircular.SkyWaver" >NUL 2>&1
)
if exist "%localappdata%\NINA\Plugins\AstroCircular.SkyWaver\NINA.AstroCircular.SkyWaver.dll" (
    echo  Cleaning up old AstroCircular.SkyWaver installation...
    del /Q "%localappdata%\NINA\Plugins\AstroCircular.SkyWaver\NINA.AstroCircular.SkyWaver.dll" >NUL 2>&1
    rmdir "%localappdata%\NINA\Plugins\AstroCircular.SkyWaver" >NUL 2>&1
)

echo  [OK] Installed to %TARGET%
echo.
echo  Start N.I.N.A. and check:
echo    Options  - Plugins - Collimation Helper for SkyWave
echo    Sequencer - SkyWave Collimation category
echo.
pause
