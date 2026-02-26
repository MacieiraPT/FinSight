@echo off
echo ================================
echo INSTALACAO DO FINSIGHT
echo ================================
echo.

cd backend\GestaoDespesas

echo A restaurar ferramentas EF locais...
dotnet tool restore

echo.

echo A criar utilizador PostgreSQL...
psql -U postgres -c "CREATE USER finsight WITH PASSWORD 'finsight123';" 2>nul

echo.

echo A criar base de dados DB_FinSight...
psql -U postgres -c "CREATE DATABASE \"DB_FinSight\" OWNER finsight;" 2>nul

echo.

echo A aplicar migrations...
dotnet tool run dotnet-ef database update --project GestaoDespesas --startup-project GestaoDespesas

echo.
echo INSTALACAO CONCLUIDA!
pause