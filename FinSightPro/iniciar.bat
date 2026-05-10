@echo off
echo Iniciando FinSight Pro...
start "" https://localhost:7080
dotnet run --project FinSightPro.Web --launch-profile FinSightPro.Web
