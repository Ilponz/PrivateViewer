@echo off
setlocal
cd /d "%~dp0"
echo ===================================================
echo   Compilazione di PrivateViewer (Chromium Edition)
echo ===================================================

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll,Microsoft.Web.WebView2.Core.dll,Microsoft.Web.WebView2.WinForms.dll /out:"PrivateViewer.exe" "PrivateViewer.cs"

if %ERRORLEVEL% EQU 0 (
    echo [OK] Compilazione completata con successo!
    echo Avvio dell'applicazione...
    start "" "PrivateViewer.exe"
) else (
    echo [ERRORE] Compilazione fallita.
)
pause
