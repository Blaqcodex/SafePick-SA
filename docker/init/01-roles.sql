-- =========================================================
-- SafePick SA — PostgreSQL initialisation script
-- Runs ONCE when the container is first created (empty volume).
--
-- Three roles, cleanly separated:
--
--   safepick        — bootstrap role. Created by the postgres image
--                     as a superuser. PostgreSQL will not let us
--                     demote it. Used only for migrations (DDL).
--                     Never used by the running application.
--
--   safepick_owner  — normal role. Will own the schema objects.
--                     Never used by the running application.
--
--   safepick_app    — the application role. Used by the running API.
--                     No superuser. No DDL. Least privilege.
--
-- The whole point of this separation is that a REVOKE against a
-- superuser is a no-op (superusers bypass privilege checks). By
-- making the running application use a NON-superuser, non-owner
-- role, our REVOKE statements become effective.
-- =========================================================

-- 1. Create the owner role.
CREATE ROLE safepick_owner WITH LOGIN PASSWORD 'safepick_owner_password';

-- 2. Create the application role.
CREATE ROLE safepick_app WITH LOGIN PASSWORD 'safepick_app_password';

-- 3. Grant the application role the minimum it needs to connect
--    and use the schema.
GRANT CONNECT ON DATABASE safepick TO safepick_app;
GRANT USAGE ON SCHEMA public TO safepick_app;