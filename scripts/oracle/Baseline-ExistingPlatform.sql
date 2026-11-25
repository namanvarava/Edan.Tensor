-- Existing Oracle platform schema baseline.
-- Run only against the existing EDAN platform schema after a backup.
-- This script does not create or drop application tables.

WHENEVER SQLERROR EXIT SQL.SQLCODE

DECLARE
    v_history_count NUMBER;
    v_tenant_count NUMBER;
    v_binding_count NUMBER;
    v_allowlist_count NUMBER;
BEGIN
    SELECT COUNT(*)
      INTO v_history_count
      FROM "__EFMigrationsHistory_Platform";

    SELECT COUNT(*)
      INTO v_tenant_count
      FROM USER_TABLES
     WHERE UPPER(TABLE_NAME) = 'PLATFORM_TENANTS';

    SELECT COUNT(*)
      INTO v_binding_count
      FROM USER_TABLES
     WHERE UPPER(TABLE_NAME) = 'PLATFORM_TENANT_DATABASE_BINDINGS';

    SELECT COUNT(*)
      INTO v_allowlist_count
      FROM USER_TABLES
     WHERE UPPER(TABLE_NAME) = 'PLATFORM_TENANT_DATABASE_ROUTINE_ALLOWLISTS';

    IF v_history_count <> 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Migration history is not empty; baseline was not applied.');
    END IF;

    IF v_tenant_count <> 1 OR v_binding_count <> 1 OR v_allowlist_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20002, 'Expected existing platform tables were not found; baseline was not applied.');
    END IF;

    INSERT INTO "__EFMigrationsHistory_Platform" ("MigrationId", "ProductVersion")
    VALUES ('20261122163058_InitialOraclePlatform', '10.0.0');

    COMMIT;
END;
/

SELECT "MigrationId", "ProductVersion"
  FROM "__EFMigrationsHistory_Platform"
 ORDER BY "MigrationId";

EXIT;
