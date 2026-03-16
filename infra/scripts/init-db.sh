#!/bin/bash
set -e

# Create additional databases needed by services sharing the same PostgreSQL instance.
# The primary 'musicratic' database is created by POSTGRES_DB env var.

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE authentik OWNER musicratic;
    CREATE DATABASE sonarqube OWNER musicratic;
EOSQL

# Create per-module schemas inside the main musicratic database.
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" \
    -f "$SCRIPT_DIR/init-schemas.sql"
