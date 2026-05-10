@echo off
setlocal enabledelayedexpansion

echo ================================
echo PARAR FINSIGHT
echo ================================
echo.

REM --- Parar backend (procurar processos a escutar nas portas do FinSight) ---
echo A procurar processos do backend nas portas 7093 e 5067...

set "FOUND="
for %%P in (7093 5067) do (
    for /f "tokens=5" %%a in ('netstat -aon ^| findstr :%%P ^| findstr LISTENING') do (
        echo A terminar processo PID %%a (porta %%P)
        taskkill /PID %%a /F >nul 2>&1
        set "FOUND=1"
    )
)

if not defined FOUND (
    echo Nenhum processo backend encontrado a escutar.
)

REM --- Por seguranca, terminar quaisquer dotnet run pendentes deste projeto ---
for /f "tokens=2" %%a in ('tasklist /FI "WINDOWTITLE eq FinSight Backend*" /FO LIST ^| findstr PID:') do (
    echo A terminar janela do backend PID %%a
    taskkill /PID %%a /F /T >nul 2>&1
)

echo Backend parado.
echo.

REM --- Parar PostgreSQL ---
echo A parar servico PostgreSQL...
set "PG_STOPPED="
for /f "tokens=2 delims=: " %%s in ('sc query state^= all ^| findstr /R /C:"SERVICE_NAME: postgresql"') do (
    echo A parar servico %%s ...
    net stop "%%s" >nul 2>&1
    set "PG_STOPPED=1"
)

if not defined PG_STOPPED (
    echo Nenhum servico postgresql detetado, a tentar terminar processos...
    taskkill /IM postgres.exe /F >nul 2>&1
    taskkill /IM pg_ctl.exe /F >nul 2>&1
)

echo PostgreSQL parado.

echo.
echo ================================
echo Tudo encerrado com sucesso!
echo ================================
pause
endlocal
