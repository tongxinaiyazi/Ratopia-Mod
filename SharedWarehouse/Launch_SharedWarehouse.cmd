@echo off
setlocal EnableExtensions
set "GAME_DIR=%~dp0"
set "MAP_DRIVE="

for %%D in (R S T U V W X Y Z) do (
    if not exist "%%D:\" if not defined MAP_DRIVE set "MAP_DRIVE=%%D:"
)

if not defined MAP_DRIVE (
    echo No free drive letter was found between R: and Z:.
    echo Close this window and move Ratopia to an English-only path instead.
    pause
    exit /b 1
)

subst %MAP_DRIVE% "%GAME_DIR:~0,-1%"
if errorlevel 1 (
    echo Failed to create the temporary ASCII drive mapping.
    pause
    exit /b 1
)

start "" /wait "%MAP_DRIVE%\Ratopia.exe"
set "GAME_EXIT_CODE=%ERRORLEVEL%"
subst %MAP_DRIVE% /D >nul 2>&1
exit /b %GAME_EXIT_CODE%
