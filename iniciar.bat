@echo off
setlocal enabledelayedexpansion

echo =====================================
echo A INICIAR FINSIGHT
echo =====================================
echo.

pushd "%~dp0"

REM --- Tentar arrancar o servico do PostgreSQL (se nao estiver ja a correr) ---
echo A verificar servico PostgreSQL...
sc query state= all | findstr /R /C:"SERVICE_NAME: postgresql" >nul 2>&1
if not errorlevel 1 (
    for /f "tokens=2 delims=: " %%s in ('sc query state^= all ^| findstr /R /C:"SERVICE_NAME: postgresql"') do (
        echo A iniciar servico %%s ...
        net start "%%s" >nul 2>&1
    )
) else (
    echo [AVISO] Nenhum servico PostgreSQL detetado. Garante que a BD esta acessivel.
)
echo.

REM --- Arrancar backend ---
if not exist "backend\GestaoDespesas" (
    echo [ERRO] Pasta backend\GestaoDespesas nao encontrada.
    popd
    pause
    exit /b 1
)

pushd "backend\GestaoDespesas"

echo A iniciar servidor backend (dotnet run)...
start "FinSight Backend" cmd /k "dotnet run --project GestaoDespesas --launch-profile https"

popd
popd

REM --- Esperar que o servidor responda na porta HTTPS ---
echo.
echo A aguardar que o servidor fique disponivel em https://localhost:7093 ...
set /a tries=0
:wait_loop
set /a tries+=1
timeout /t 2 /nobreak >nul
netstat -ano | findstr :7093 | findstr LISTENING >nul 2>&1
if not errorlevel 1 goto :ready
if %tries% GEQ 30 goto :timeout
goto :wait_loop

:timeout
echo [AVISO] O servidor demorou mais do que o esperado a arrancar.
echo Pode na mesma tentar abrir https://localhost:7093 manualmente.
goto :open

:ready
echo Servidor pronto!

:open
echo.
echo =====================================
echo FINSIGHT INICIADO!
echo URL: https://localhost:7093
echo =====================================
start "" "https://localhost:7093"

endlocal
