#!/usr/bin/env bash
set -euo pipefail

echo "==========================================="
echo " FinSight Pro - Instalação (Linux/macOS)"
echo "==========================================="

DB_NAME="${DB_NAME:-DB_FinSightPro}"
DB_USER="${DB_USER:-finsightpro}"
DB_PASS="${DB_PASS:-finsightpro123}"
PG_USER="${PG_USER:-postgres}"

cd "$(dirname "$0")"

echo "[1/4] A restaurar dotnet tools (dotnet-ef)..."
dotnet tool restore

echo "[2/4] A criar utilizador e base de dados PostgreSQL..."
psql -U "$PG_USER" -c "DROP DATABASE IF EXISTS \"$DB_NAME\";" || true
psql -U "$PG_USER" -c "DROP USER IF EXISTS $DB_USER;" || true
psql -U "$PG_USER" -c "CREATE USER $DB_USER WITH PASSWORD '$DB_PASS';"
psql -U "$PG_USER" -c "CREATE DATABASE \"$DB_NAME\" OWNER $DB_USER;"

echo "[3/4] A restaurar pacotes NuGet..."
dotnet restore FinSightPro.sln

echo "[4/4] A aplicar migrações..."
dotnet ef database update --project FinSightPro.Infrastructure --startup-project FinSightPro.Web

echo ""
echo "Instalação concluída! Use ./iniciar.sh para arrancar a aplicação."
