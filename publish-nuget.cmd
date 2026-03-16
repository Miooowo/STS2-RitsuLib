@echo off
setlocal

set "ROOT=%~dp0"
set "PS1=%ROOT%scripts\publish-nuget.ps1"

if not exist "%PS1%" (
  echo [ERROR] Script not found: %PS1%
  exit /b 1
)

where pwsh >nul 2>&1
if %ERRORLEVEL% EQU 0 (
  set "PS_EXE=pwsh"
) else (
  set "PS_EXE=powershell"
)

%PS_EXE% -NoProfile -ExecutionPolicy Bypass -File "%PS1%" %*
set "EXIT_CODE=%ERRORLEVEL%"

endlocal & exit /b %EXIT_CODE%
