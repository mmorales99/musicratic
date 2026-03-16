-- Musicratic — Schema-per-module initialization
-- Idempotent: safe to run multiple times.

-- Module schemas
CREATE SCHEMA IF NOT EXISTS auth;
CREATE SCHEMA IF NOT EXISTS hub;
CREATE SCHEMA IF NOT EXISTS playback;
CREATE SCHEMA IF NOT EXISTS voting;
CREATE SCHEMA IF NOT EXISTS economy;
CREATE SCHEMA IF NOT EXISTS analytics;
CREATE SCHEMA IF NOT EXISTS social;
CREATE SCHEMA IF NOT EXISTS notification;

-- Grant usage and object-creation privileges to the application user.
-- When POSTGRES_USER=musicratic, this user already owns the DB, but explicit
-- grants keep the setup correct if the owner is ever changed to a superuser.
DO $$
DECLARE
    schema_name TEXT;
BEGIN
    FOREACH schema_name IN ARRAY ARRAY[
        'auth','hub','playback','voting',
        'economy','analytics','social','notification'
    ]
    LOOP
        EXECUTE format('GRANT USAGE  ON SCHEMA %I TO musicratic', schema_name);
        EXECUTE format('GRANT CREATE ON SCHEMA %I TO musicratic', schema_name);
        EXECUTE format(
            'ALTER DEFAULT PRIVILEGES IN SCHEMA %I '
            'GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO musicratic',
            schema_name
        );
    END LOOP;
END
$$;
