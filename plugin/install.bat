@echo off
title AstroCircular SkyWaver Installer
echo.
echo  ============================================
echo   AstroCircular SkyWaver for N.I.N.A.
echo   Plugin Installer v0.1.0
echo   by joergsflow
echo  ============================================
echo.

set "SCRIPT_DIR=%~dp0"
set "DLL="

:: Find the DLL
if exist "%SCRIPT_DIR%AstroCircular.SkyWaver\NINA.AstroCircular.SkyWaver.dll" (
    set "DLL=%SCRIPT_DIR%AstroCircular.SkyWaver\NINA.AstroCircular.SkyWaver.dll"
    goto :found_dll
)
if exist "%SCRIPT_DIR%NINA.AstroCircular.SkyWaver.dll" (
    set "DLL=%SCRIPT_DIR%NINA.AstroCircular.SkyWaver.dll"
    goto :found_dll
)

echo  [ERROR] Cannot find NINA.AstroCircular.SkyWaver.dll
echo.
pause
goto :eof

:found_dll
echo  DLL: %DLL%

:: Find NINA Plugins folder
set "TARGET="
if exist "%localappdata%\NINA\3\Plugins" (
    set "TARGET=%localappdata%\NINA\3\Plugins\AstroCircular.SkyWaver"
    goto :found_nina
)
if exist "%localappdata%\NINA\Plugins" (
    set "TARGET=%localappdata%\NINA\Plugins\AstroCircular.SkyWaver"
    goto :found_nina
)

echo.
echo  Could not find NINA Plugins folder automatically.
echo  Enter path to your NINA Plugins folder:
echo.
set /p "TARGET_BASE=  Path: "
set "TARGET=%TARGET_BASE%\AstroCircular.SkyWaver"

:found_nina
echo  Target: %TARGET%
echo.

if exist "%TARGET%\NINA.AstroCircular.SkyWaver.dll" (
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

echo  [OK] Installed to %TARGET%
echo.
echo  Start N.I.N.A. and check:
echo    Options  - Plugins - AstroCircular SkyWaver
echo    Sequencer - AstroCircular SKW category
echo.
pause
