@echo off
setlocal enabledelayedexpansion

:: Load .env variables
for /f "delims=" %%a in ('type ..\..\..\.env') do (
    for /f "tokens=1,2 delims==" %%b in ("%%a") do set %%b=%%c
)

:: Run SteamCMD to upload the build
steamcmd +login %STEAM_USERNAME% %STEAM_PASSWORD% +run_app_build %VDF_PATH_DEMO% +quit

echo Demo build upload complete!
pause