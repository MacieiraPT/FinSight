@echo off
setlocal enabledelayedexpansion

echo ================================
echo INSTALACAO DO FINSIGHT
echo ================================
echo.

REM Move to the directory where this script lives so paths work no matter where it's run from
pushd "%~dp0"

REM --- Verificar pre-requisitos ---
echo A verificar pre-requisitos...

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERRO] dotnet nao foi encontrado no PATH.
    echo Instale o .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
    popd
    pause
    exit /b 1
)

where psql >nul 2>&1
if errorlevel 1 (
    echo [AVISO] psql nao foi encontrado no PATH.
    echo Certifique-se de que o PostgreSQL esta instalado e que a pasta bin esta no PATH.
    echo A instalacao vai continuar, mas a criacao da BD pode falhar.
)

echo OK.
echo.

REM --- Garantir appsettings.json ---
set "APPSETTINGS=backend\GestaoDespesas\GestaoDespesas\appsettings.json"
set "APPSETTINGS_EXAMPLE=backend\GestaoDespesas\GestaoDespesas\appsettings.Example.json"
if not exist "%APPSETTINGS%" (
    if exist "%APPSETTINGS_EXAMPLE%" (
        echo A criar appsettings.json a partir do exemplo...
        copy /Y "%APPSETTINGS_EXAMPLE%" "%APPSETTINGS%" >nul
    ) else (
        echo [AVISO] appsettings.Example.json nao encontrado.
    )
)
echo.

REM --- Restaurar ferramentas dotnet ---
pushd "backend\GestaoDespesas"

echo A restaurar ferramentas EF locais...
dotnet tool restore
if errorlevel 1 (
    echo [ERRO] Falha ao restaurar ferramentas dotnet.
    popd
    popd
    pause
    exit /b 1
)
echo.

REM --- Criar utilizador e base de dados PostgreSQL ---
echo A criar utilizador PostgreSQL 'finsight' (ignora se ja existir)...
psql -U postgres -c "CREATE USER finsight WITH PASSWORD 'finsight123';" 2>nul
echo.

echo A criar base de dados DB_FinSight (ignora se ja existir)...
psql -U postgres -c "CREATE DATABASE \"DB_FinSight\" OWNER finsight;" 2>nul
echo.

REM --- Aplicar migrations ---
echo A aplicar migrations...
dotnet tool run dotnet-ef database update --project GestaoDespesas --startup-project GestaoDespesas
if errorlevel 1 (
    echo [ERRO] Falha ao aplicar migrations. Verifique a connection string em appsettings.json.
    popd
    popd
    pause
    exit /b 1
)

popd
popd

echo.
echo ================================
echo INSTALACAO CONCLUIDA!
echo ================================
pause
endlocal
