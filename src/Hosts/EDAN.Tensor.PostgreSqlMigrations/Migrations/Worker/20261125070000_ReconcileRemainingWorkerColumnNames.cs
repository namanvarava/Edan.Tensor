using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Worker;

[Migration("20261125070000_ReconcileRemainingWorkerColumnNames")]
public partial class ReconcileRemainingWorkerColumnNames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_attribute WHERE attrelid = 'public.edwk_wkrtsk'::regclass AND attname = 'LeaseId' AND NOT attisdropped)
       AND NOT EXISTS (SELECT 1 FROM pg_attribute WHERE attrelid = 'public.edwk_wkrtsk'::regclass AND attname = 'leaseid' AND NOT attisdropped) THEN
        EXECUTE 'ALTER TABLE public.edwk_wkrtsk RENAME COLUMN "LeaseId" TO leaseid';
    END IF;
    IF EXISTS (SELECT 1 FROM pg_attribute WHERE attrelid = 'public.edwk_wkrtyp'::regclass AND attname = 'TaskType' AND NOT attisdropped)
       AND NOT EXISTS (SELECT 1 FROM pg_attribute WHERE attrelid = 'public.edwk_wkrtyp'::regclass AND attname = 'tsktyp' AND NOT attisdropped) THEN
        EXECUTE 'ALTER TABLE public.edwk_wkrtyp RENAME COLUMN "TaskType" TO tsktyp';
    END IF;
END $$;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_attribute WHERE attrelid = 'public.edwk_wkrtsk'::regclass AND attname = 'leaseid' AND NOT attisdropped)
       AND NOT EXISTS (SELECT 1 FROM pg_attribute WHERE attrelid = 'public.edwk_wkrtsk'::regclass AND attname = 'LeaseId' AND NOT attisdropped) THEN
        EXECUTE 'ALTER TABLE public.edwk_wkrtsk RENAME COLUMN leaseid TO "LeaseId"';
    END IF;
    IF EXISTS (SELECT 1 FROM pg_attribute WHERE attrelid = 'public.edwk_wkrtyp'::regclass AND attname = 'tsktyp' AND NOT attisdropped)
       AND NOT EXISTS (SELECT 1 FROM pg_attribute WHERE attrelid = 'public.edwk_wkrtyp'::regclass AND attname = 'TaskType' AND NOT attisdropped) THEN
        EXECUTE 'ALTER TABLE public.edwk_wkrtyp RENAME COLUMN tsktyp TO "TaskType"';
    END IF;
END $$;
""");
    }
}
