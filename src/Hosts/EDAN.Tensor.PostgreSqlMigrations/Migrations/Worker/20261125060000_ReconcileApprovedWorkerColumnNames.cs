using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Worker;

[Migration("20261125060000_ReconcileApprovedWorkerColumnNames")]
public partial class ReconcileApprovedWorkerColumnNames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrdlv' AND column_name = 'wkrdlvid')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrdlv' AND column_name = 'wkr_dlvid') THEN
        ALTER TABLE public.edwk_wkrdlv RENAME COLUMN wkrdlvid TO wkr_dlvid;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrtsk' AND column_name = 'wkrtskid')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrtsk' AND column_name = 'wkr_tskid') THEN
        ALTER TABLE public.edwk_wkrtsk RENAME COLUMN wkrtskid TO wkr_tskid;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrtsk' AND column_name = 'TaskType')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrtsk' AND column_name = 'tsktyp') THEN
        ALTER TABLE public.edwk_wkrtsk RENAME COLUMN "TaskType" TO tsktyp;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrtsk' AND column_name = 'PayloadReference')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrtsk' AND column_name = 'pld_ref') THEN
        ALTER TABLE public.edwk_wkrtsk RENAME COLUMN "PayloadReference" TO pld_ref;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrrpl' AND column_name = 'wkrrplid')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrrpl' AND column_name = 'wkr_rplid') THEN
        ALTER TABLE public.edwk_wkrrpl RENAME COLUMN wkrrplid TO wkr_rplid;
    END IF;
END $$;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrdlv' AND column_name = 'wkr_dlvid') THEN ALTER TABLE public.edwk_wkrdlv RENAME COLUMN wkr_dlvid TO wkrdlvid; END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrtsk' AND column_name = 'wkr_tskid') THEN ALTER TABLE public.edwk_wkrtsk RENAME COLUMN wkr_tskid TO wkrtskid; END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrtsk' AND column_name = 'tsktyp') THEN ALTER TABLE public.edwk_wkrtsk RENAME COLUMN tsktyp TO "TaskType"; END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrtsk' AND column_name = 'pld_ref') THEN ALTER TABLE public.edwk_wkrtsk RENAME COLUMN pld_ref TO "PayloadReference"; END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'edwk_wkrrpl' AND column_name = 'wkr_rplid') THEN ALTER TABLE public.edwk_wkrrpl RENAME COLUMN wkr_rplid TO wkrrplid; END IF;
END $$;
""");
    }
}
