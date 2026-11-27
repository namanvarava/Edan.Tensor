using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReconcileSequentialDataSetSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"edds_dset\" RENAME TO \"edw_05001\";");
            migrationBuilder.Sql("ALTER TABLE \"edds_dsetschm_rev\" RENAME TO \"edw_05002\";");
            migrationBuilder.Sql("ALTER TABLE \"edds_dsetstr_usgsnp\" RENAME TO \"edw_05003\";");
            migrationBuilder.Sql("ALTER TABLE \"edds_dsetqta_threvt\" RENAME TO \"edw_05004\";");
            migrationBuilder.Sql("ALTER TABLE \"edds_dsetvld_prf\" RENAME TO \"edw_05005\";");
            migrationBuilder.Sql("ALTER TABLE \"edds_dsetvld_prfrev\" RENAME TO \"edw_05006\";");
            migrationBuilder.Sql("ALTER TABLE \"edds_dsetvld_exc\" RENAME TO \"edw_05007\";");
            migrationBuilder.Sql("ALTER TABLE \"edds_dsetvld_dtl\" RENAME TO \"edw_05008\";");
            migrationBuilder.Sql("ALTER TABLE \"edds_dsetvld_seenkey\" RENAME TO \"edw_05009\";");
            return;
            /*
            migrationBuilder.DropTable(
                name: "edds_dsetschm_rev");

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

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_seenkey_N01",
                table: "edw_05009",
                newName: "edw_05009_N01");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prfrev_N01",
                table: "edw_05006",
                newName: "edw_05006_N01");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prf_N01",
                table: "edw_05005",
                newName: "edw_05005_N01");

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

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edw_05008",
                newName: "edw_05008_N02");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N01",
                table: "edw_05008",
                newName: "edw_05008_N01");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N02",
                table: "edw_05003",
                newName: "edw_05003_N02");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N01",
                table: "edw_05003",
                newName: "edw_05003_N01");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N02",
                table: "edw_05004",
                newName: "edw_05004_N02");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N01",
                table: "edw_05004",
                newName: "edw_05004_N01");

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
                column: "dset_vld_seen_keyid");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05006_PK",
                table: "edw_05006",
                columns: new[] { "prfid", "rev" });

            migrationBuilder.AddPrimaryKey(
                name: "edw_05005_PK",
                table: "edw_05005",
                column: "dset_vld_prfid");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05007_PK",
                table: "edw_05007",
                column: "dset_vld_excid");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05008_PK",
                table: "edw_05008",
                column: "dset_vld_dtlid");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05003_PK",
                table: "edw_05003",
                column: "dset_str_usg_snpid");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05004_PK",
                table: "edw_05004",
                column: "dset_qta_thr_evtid");

            migrationBuilder.AddPrimaryKey(
                name: "edw_05001_PK",
                table: "edw_05001",
                column: "dsetid");

            migrationBuilder.CreateTable(
                name: "edw_05002",
                columns: table => new
                {
                    ver = table.Column<int>(type: "INTEGER", nullable: false),
                    dsetid = table.Column<Guid>(type: "TEXT", nullable: false),
                    schm_json = table.Column<string>(type: "TEXT", maxLength: 1048576, nullable: false),
                    crt_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("edw_05002_PK", x => new { x.dsetid, x.ver });
                    table.ForeignKey(
                        name: "edw_05002_FKDataSetId",
                        column: x => x.dsetid,
                        principalTable: "edw_05001",
                        principalColumn: "dsetid",
                        onDelete: ReferentialAction.Cascade);
                });
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"edw_05001\" RENAME TO \"edds_dset\";");
            migrationBuilder.Sql("ALTER TABLE \"edw_05002\" RENAME TO \"edds_dsetschm_rev\";");
            migrationBuilder.Sql("ALTER TABLE \"edw_05003\" RENAME TO \"edds_dsetstr_usgsnp\";");
            migrationBuilder.Sql("ALTER TABLE \"edw_05004\" RENAME TO \"edds_dsetqta_threvt\";");
            migrationBuilder.Sql("ALTER TABLE \"edw_05005\" RENAME TO \"edds_dsetvld_prf\";");
            migrationBuilder.Sql("ALTER TABLE \"edw_05006\" RENAME TO \"edds_dsetvld_prfrev\";");
            migrationBuilder.Sql("ALTER TABLE \"edw_05007\" RENAME TO \"edds_dsetvld_exc\";");
            migrationBuilder.Sql("ALTER TABLE \"edw_05008\" RENAME TO \"edds_dsetvld_dtl\";");
            migrationBuilder.Sql("ALTER TABLE \"edw_05009\" RENAME TO \"edds_dsetvld_seenkey\";");
            return;
            /*
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

            migrationBuilder.RenameIndex(
                name: "edw_05009_N01",
                table: "edds_dsetvld_seenkey",
                newName: "edds_dsetvld_seenkey_N01");

            migrationBuilder.RenameIndex(
                name: "edw_05008_N02",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N02");

            migrationBuilder.RenameIndex(
                name: "edw_05008_N01",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N01");

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

            migrationBuilder.RenameIndex(
                name: "edw_05005_N01",
                table: "edds_dsetvld_prf",
                newName: "edds_dsetvld_prf_N01");

            migrationBuilder.RenameIndex(
                name: "edw_05004_N02",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N02");

            migrationBuilder.RenameIndex(
                name: "edw_05004_N01",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N01");

            migrationBuilder.RenameIndex(
                name: "edw_05003_N02",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N02");

            migrationBuilder.RenameIndex(
                name: "edw_05003_N01",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N01");

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
                    dsetid = table.Column<Guid>(type: "TEXT", nullable: false),
                    ver = table.Column<int>(type: "INTEGER", nullable: false),
                    crt_utc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    schm_json = table.Column<string>(type: "TEXT", maxLength: 1048576, nullable: false)
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
            */
        }
    }
}
