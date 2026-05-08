@echo off
setlocal
echo ===========================================
echo  FinSight Pro - Instalacao (Windows)
echo ===========================================

REM Variaveis
set DB_NAME=DB_FinSightPro
set DB_USER=finsightpro
set DB_PASS=finsightpro123
set PG_USER=postgres

echo.
echo [1/4] A restaurar dotnet tools (dotnet-ef)...
dotnet tool restore
if errorlevel 1 goto :error

echo.
echo [2/4] A criar utilizador e base de dados PostgreSQL...
psql -U %PG_USER% -c "DROP DATABASE IF EXISTS \"%DB_NAME%\";"
psql -U %PG_USER% -c "DROP USER IF EXISTS %DB_USER%;"
psql -U %PG_USER% -c "CREATE USER %DB_USER% WITH PASSWORD '%DB_PASS%';"
psql -U %PG_USER% -c "CREATE DATABASE \"%DB_NAME%\" OWNER %DB_USER%;"

echo.
echo [3/4] A restaurar pacotes NuGet...
dotnet restore FinSightPro.sln
if errorlevel 1 goto :error

echo.
echo [4/4] A aplicar migracoes...
dotnet ef database update --project FinSightPro.Infrastructure --startup-project FinSightPro.Web
if errorlevel 1 goto :error

echo.
echo Instalacao concluida! Use iniciar.bat para arrancar a aplicacao.
goto :eof

:error
echo.
echo Ocorreu um erro durante a instalacao.
exit /b 1
