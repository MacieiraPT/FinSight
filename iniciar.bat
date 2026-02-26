@echo off
echo =====================================
echo A INICIAR FINSIGHT
echo =====================================
echo.

cd backend\GestaoDespesas

start "" dotnet run --project GestaoDespesas

timeout /t 5 >nul

start https://localhost:7093

echo.
echo FINSIGHT INICIADO!