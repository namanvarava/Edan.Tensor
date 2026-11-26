using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.DataSet
{
    /// <inheritdoc />
    public partial class ApplySequentialFrameworkDataSetTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Older approved-schema installs may already use compact column names.
            // Normalize them to the migration's source names before applying the final rename.
            migrationBuilder.Sql("""
DO $$
DECLARE
    item record;
    existing_name text;
BEGIN
    FOR item IN
        SELECT * FROM (VALUES
                ('edds_dsetvld_prf','nm','dset_vld_prfnm'),
                ('edds_dsetvld_prf','dsetvldprfid','dset_vld_prfid'),
                ('edds_dsetvld_exc','dsetvldexcid','dset_vld_excid'),
                ('edds_dsetvld_dtl','dsetvlddtlid','dset_vld_dtlid'),
                ('edds_dsetstr_usgsnp','dsetstrusgsnpid','dset_str_usg_snpid'),
                ('edds_dsetqta_threvt','dsetqtathrevtid','dset_qta_thr_evtid'),
                ('edds_dsetvld_seenkey','dsetvldseenkeyid','dset_vld_seen_keyid'),
                ('edds_dset','nm','dsetnm')
        ) AS v(table_name, existing_name, canonical_name)
    LOOP
        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = item.table_name)
           AND EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = item.table_name AND column_name = item.existing_name)
           AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = item.table_name AND column_name = item.canonical_name) THEN
            EXECUTE format('ALTER TABLE %I RENAME COLUMN %I TO %I', item.table_name, item.existing_name, item.canonical_name);
        END IF;
    END LOOP;

    FOR item IN
        SELECT * FROM (VALUES
                ('edds_dsetvld_seenkey','dset_vld_seen_keyid','05009id'),
                ('edds_dsetvld_prf','dset_vld_prfnm','05005nm'),
                ('edds_dsetvld_prf','dset_vld_prfid','05005id'),
                ('edds_dsetvld_exc','dset_vld_excid','05007id'),
                ('edds_dsetvld_dtl','dset_vld_dtlcd','05008cd'),
                ('edds_dsetvld_dtl','dset_vld_dtlid','05008id'),
                ('edds_dsetstr_usgsnp','dset_str_usg_snpid','05003id'),
                ('edds_dsetqta_threvt','dset_qta_thr_evtid','05004id'),
                ('edds_dset','dsetnm','05001nm'),
                ('edds_dset','dsetid','05001id')
        ) AS v(table_name, source_name, target_name)
    LOOP
        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = item.table_name)
           AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = item.table_name AND column_name = item.source_name)
           AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = item.table_name AND column_name = item.target_name) THEN
            SELECT column_name INTO existing_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = item.table_name
              AND replace(column_name, '_', '') = replace(item.source_name, '_', '')
            LIMIT 1;
            IF existing_name IS NOT NULL THEN
                EXECUTE format('ALTER TABLE %I RENAME COLUMN %I TO %I', item.table_name, existing_name, item.source_name);
            END IF;
        END IF;
    END LOOP;
END $$;
""");

            migrationBuilder.Sql("DROP TABLE IF EXISTS edds_dsetschm_rev;");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_seenkey_PK",
                table: "edds_dsetvld_seenkey");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_prfrev_PK",
                table: "edds_dsetvld_prfrev");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_prf_PK",
                table: "edds_dsetvld_prf");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_exc_PK",
                table: "edds_dsetvld_exc");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_dtl_PK",
                table: "edds_dsetvld_dtl");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetstr_usgsnp_PK",
                table: "edds_dsetstr_usgsnp");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetqta_threvt_PK",
                table: "edds_dsetqta_threvt");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dset_PK",
                table: "edds_dset");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_seenkey",
                newName: "edw_05009");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_prfrev",
                newName: "edw_05006");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_prf",
                newName: "edw_05005");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_exc",
                newName: "edw_05007");

            migrationBuilder.RenameTable(
                name: "edds_dsetvld_dtl",
                newName: "edw_05008");

            migrationBuilder.RenameTable(
                name: "edds_dsetstr_usgsnp",
                newName: "edw_05003");

            migrationBuilder.RenameTable(
                name: "edds_dsetqta_threvt",
                newName: "edw_05004");

            migrationBuilder.RenameTable(
                name: "edds_dset",
                newName: "edw_05001");

            migrationBuilder.RenameColumn(
                name: "dset_vld_seen_keyid",
                table: "edw_05009",
                newName: "05009id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_seenkey_N01",
                table: "edw_05009",
                newName: "edw_05009_N01");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prfrev_N01",
                table: "edw_05006",
                newName: "edw_05006_N01");

            migrationBuilder.RenameColumn(
                name: "dset_vld_prfnm",
                table: "edw_05005",
                newName: "05005nm");

            migrationBuilder.RenameColumn(
                name: "dset_vld_prfid",
                table: "edw_05005",
                newName: "05005id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prf_N01",
                table: "edw_05005",
                newName: "edw_05005_N01");

            migrationBuilder.RenameColumn(
                name: "dset_vld_excid",
                table: "edw_05007",
                newName: "05007id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N03",
                table: "edw_05007",
                newName: "edw_05007_N03");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N02",
                table: "edw_05007",
                newName: "edw_05007_N02");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N01",
                table: "edw_05007",
                newName: "edw_05007_N01");

            migrationBuilder.RenameColumn(
                name: "dset_vld_dtlcd",
                table: "edw_05008",
                newName: "05008cd");

            migrationBuilder.RenameColumn(
                name: "dset_vld_dtlid",
                table: "edw_05008",
                newName: "05008id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edw_05008",
                newName: "edw_05008_N02");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N01",
                table: "edw_05008",
                newName: "edw_05008_N01");

            migrationBuilder.RenameColumn(
                name: "dset_str_usg_snpid",
                table: "edw_05003",
                newName: "05003id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N02",
                table: "edw_05003",
                newName: "edw_05003_N02");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N01",
                table: "edw_05003",
                newName: "edw_05003_N01");

            migrationBuilder.RenameColumn(
                name: "dset_qta_thr_evtid",
                table: "edw_05004",
                newName: "05004id");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N02",
                table: "edw_05004",
                newName: "edw_05004_N02");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N01",
                table: "edw_05004",
                newName: "edw_05004_N01");

            migrationBuilder.RenameColumn(
                name: "dsetnm",
                table: "edw_05001",
                newName: "05001nm");

            migrationBuilder.RenameColumn(
                name: "dsetid",
                table: "edw_05001",
                newName: "05001id");

            migrationBuilder.RenameIndex(
                name: "edds_dset_N02",
                table: "edw_05001",
                newName: "edw_05001_N02");

            migrationBuilder.RenameIndex(
                name: "edds_dset_N01",
                table: "edw_05001",
                newName: "edw_05001_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05009_PK",
                table: "edw_05009",
                column: "05009id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05006_PK",
                table: "edw_05006",
                columns: new[] { "prfid", "rev" });

            migrationBuilder.AddPrimaryKey(
                name: "edw_05005_PK",
                table: "edw_05005",
                column: "05005id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05007_PK",
                table: "edw_05007",
                column: "05007id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05008_PK",
                table: "edw_05008",
                column: "05008id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05003_PK",
                table: "edw_05003",
                column: "05003id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05004_PK",
                table: "edw_05004",
                column: "05004id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05001_PK",
                table: "edw_05001",
                column: "05001id");

            migrationBuilder.CreateTable(
                name: "edw_05002",
                columns: table => new
                {
                    ver = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dsetid = table.Column<Guid>(type: "uuid", nullable: false),
                    schm_json = table.Column<string>(type: "character varying(1048576)", maxLength: 1048576, nullable: false),
                    crt_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edw_05002_PK", x => new { x.dsetid, x.ver });
                    table.ForeignKey(
                        name: "edw_05002_FKDataSetId",
                        column: x => x.dsetid,
                        principalTable: "edw_05001",
                        principalColumn: "05001id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "edw_05002");

            migrationBuilder.DropPrimaryKey(
                name: "edw_05009_PK",
                table: "edw_05009");

            migrationBuilder.DropPrimaryKey(
                name: "edw_05008_PK",
                table: "edw_05008");

            migrationBuilder.DropPrimaryKey(
                name: "edw_05007_PK",
                table: "edw_05007");

            migrationBuilder.DropPrimaryKey(
                name: "edw_05006_PK",
                table: "edw_05006");

            migrationBuilder.DropPrimaryKey(
                name: "edw_05005_PK",
                table: "edw_05005");

            migrationBuilder.DropPrimaryKey(
                name: "edw_05004_PK",
                table: "edw_05004");

            migrationBuilder.DropPrimaryKey(
                name: "edw_05003_PK",
                table: "edw_05003");

            migrationBuilder.DropPrimaryKey(
                name: "edw_05001_PK",
                table: "edw_05001");

            migrationBuilder.RenameTable(
                name: "edw_05009",
                newName: "edds_dsetvld_seenkey");

            migrationBuilder.RenameTable(
                name: "edw_05008",
                newName: "edds_dsetvld_dtl");

            migrationBuilder.RenameTable(
                name: "edw_05007",
                newName: "edds_dsetvld_exc");

            migrationBuilder.RenameTable(
                name: "edw_05006",
                newName: "edds_dsetvld_prfrev");

            migrationBuilder.RenameTable(
                name: "edw_05005",
                newName: "edds_dsetvld_prf");

            migrationBuilder.RenameTable(
                name: "edw_05004",
                newName: "edds_dsetqta_threvt");

            migrationBuilder.RenameTable(
                name: "edw_05003",
                newName: "edds_dsetstr_usgsnp");

            migrationBuilder.RenameTable(
                name: "edw_05001",
                newName: "edds_dset");

            migrationBuilder.RenameColumn(
                name: "05009id",
                table: "edds_dsetvld_seenkey",
                newName: "dset_vld_seen_keyid");

            migrationBuilder.RenameIndex(
                name: "edw_05009_N01",
                table: "edds_dsetvld_seenkey",
                newName: "edds_dsetvld_seenkey_N01");

            migrationBuilder.RenameColumn(
                name: "05008cd",
                table: "edds_dsetvld_dtl",
                newName: "dset_vld_dtlcd");

            migrationBuilder.RenameColumn(
                name: "05008id",
                table: "edds_dsetvld_dtl",
                newName: "dset_vld_dtlid");

            migrationBuilder.RenameIndex(
                name: "edw_05008_N02",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N02");

            migrationBuilder.RenameIndex(
                name: "edw_05008_N01",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N01");

            migrationBuilder.RenameColumn(
                name: "05007id",
                table: "edds_dsetvld_exc",
                newName: "dset_vld_excid");

            migrationBuilder.RenameIndex(
                name: "edw_05007_N03",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N03");

            migrationBuilder.RenameIndex(
                name: "edw_05007_N02",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N02");

            migrationBuilder.RenameIndex(
                name: "edw_05007_N01",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N01");

            migrationBuilder.RenameIndex(
                name: "edw_05006_N01",
                table: "edds_dsetvld_prfrev",
                newName: "edds_dsetvld_prfrev_N01");

            migrationBuilder.RenameColumn(
                name: "05005nm",
                table: "edds_dsetvld_prf",
                newName: "dset_vld_prfnm");

            migrationBuilder.RenameColumn(
                name: "05005id",
                table: "edds_dsetvld_prf",
                newName: "dset_vld_prfid");

            migrationBuilder.RenameIndex(
                name: "edw_05005_N01",
                table: "edds_dsetvld_prf",
                newName: "edds_dsetvld_prf_N01");

            migrationBuilder.RenameColumn(
                name: "05004id",
                table: "edds_dsetqta_threvt",
                newName: "dset_qta_thr_evtid");

            migrationBuilder.RenameIndex(
                name: "edw_05004_N02",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N02");

            migrationBuilder.RenameIndex(
                name: "edw_05004_N01",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N01");

            migrationBuilder.RenameColumn(
                name: "05003id",
                table: "edds_dsetstr_usgsnp",
                newName: "dset_str_usg_snpid");

            migrationBuilder.RenameIndex(
                name: "edw_05003_N02",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N02");

            migrationBuilder.RenameIndex(
                name: "edw_05003_N01",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N01");

            migrationBuilder.RenameColumn(
                name: "05001nm",
                table: "edds_dset",
                newName: "dsetnm");

            migrationBuilder.RenameColumn(
                name: "05001id",
                table: "edds_dset",
                newName: "dsetid");

            migrationBuilder.RenameIndex(
                name: "edw_05001_N02",
                table: "edds_dset",
                newName: "edds_dset_N02");

            migrationBuilder.RenameIndex(
                name: "edw_05001_N01",
                table: "edds_dset",
                newName: "edds_dset_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_seenkey_PK",
                table: "edds_dsetvld_seenkey",
                column: "dset_vld_seen_keyid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_dtl_PK",
                table: "edds_dsetvld_dtl",
                column: "dset_vld_dtlid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_exc_PK",
                table: "edds_dsetvld_exc",
                column: "dset_vld_excid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_prfrev_PK",
                table: "edds_dsetvld_prfrev",
                columns: new[] { "prfid", "rev" });

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_prf_PK",
                table: "edds_dsetvld_prf",
                column: "dset_vld_prfid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetqta_threvt_PK",
                table: "edds_dsetqta_threvt",
                column: "dset_qta_thr_evtid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetstr_usgsnp_PK",
                table: "edds_dsetstr_usgsnp",
                column: "dset_str_usg_snpid");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dset_PK",
                table: "edds_dset",
                column: "dsetid");

            migrationBuilder.CreateTable(
                name: "edds_dsetschm_rev",
                columns: table => new
                {
                    dsetid = table.Column<Guid>(type: "uuid", nullable: false),
                    ver = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    crt_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    schm_json = table.Column<string>(type: "character varying(1048576)", maxLength: 1048576, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edds_dsetschm_rev_PK", x => new { x.dsetid, x.ver });
                    table.ForeignKey(
                        name: "edds_dsetschm_rev_FKDataSetId",
                        column: x => x.dsetid,
                        principalTable: "edds_dset",
                        principalColumn: "dsetid",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
