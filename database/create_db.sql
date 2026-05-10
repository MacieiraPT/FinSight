-- =============================================================================
-- FinSight - Criação do utilizador e da base de dados PostgreSQL
-- =============================================================================
-- Executar como superutilizador (ex: `psql -U postgres -f create_db.sql`).
--
-- Valores predefinidos (alinhados com appsettings.Example.json):
--   - Utilizador:      finsight
--   - Password:        finsight123
--   - Base de dados:   DB_FinSight
--
-- Após este script, aplicar as migrations EF Core a partir de
-- `backend/GestaoDespesas`:
--     dotnet ef database update --project GestaoDespesas
--
-- Em alternativa, aplicar o esquema completo através de `init.sql`.
-- =============================================================================

CREATE USER finsight WITH PASSWORD 'finsight123';

CREATE DATABASE "DB_FinSight" OWNER finsight;

\c "DB_FinSight"

GRANT ALL PRIVILEGES ON DATABASE "DB_FinSight" TO finsight;
GRANT ALL ON SCHEMA public TO finsight;
