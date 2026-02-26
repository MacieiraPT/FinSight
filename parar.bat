@echo off
echo ================================
echo PARAR FINSIGHT
echo ================================
echo.

echo A procurar processos na porta 6767...

for /f "tokens=5" %%a in ('netstat -aon ^| findstr :6767 ^| findstr LISTENING') do (
    echo A terminar processo %%a
    taskkill /PID %%a /F >nul 2>&1
)

echo Backend parado.

echo.
echo A terminar PostgreSQL...

taskkill /IM postgres.exe /F >nul 2>&1
taskkill /IM pg_ctl.exe /F >nul 2>&1

echo PostgreSQL parado.

echo.
echo Tudo encerrado com sucesso!
pause