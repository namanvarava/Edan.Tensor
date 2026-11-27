using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Worker
{
    /// <inheritdoc />
    public partial class ApplySequentialFrameworkWorkerTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "edwk_wkrtyp_PK",
                table: "edwk_wkrtyp");

            migrationBuilder.DropPrimaryKey(
                name: "edwk_wkrtsk_PK",
                table: "edwk_wkrtsk");

            migrationBuilder.DropPrimaryKey(
                name: "edwk_wkrrpl_PK",
                table: "edwk_wkrrpl");

            migrationBuilder.DropPrimaryKey(
                name: "edwk_wkrdlv_PK",
                table: "edwk_wkrdlv");

            migrationBuilder.RenameTable(
                name: "edwk_wkrtyp",
                newName: "edw_03007");

            migrationBuilder.RenameTable(
                name: "edwk_wkrtsk",
                newName: "edw_03005");

            migrationBuilder.RenameTable(
                name: "edwk_wkrrpl",
                newName: "edw_03006");

            migrationBuilder.RenameTable(
                name: "edwk_wkrdlv",
                newName: "edw_03004");

            migrationBuilder.RenameColumn(
                name: "wkr_tskid",
                table: "edw_03005",
                newName: "03005id");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N06",
                table: "edw_03005",
                newName: "edw_03005_N06");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N05",
                table: "edw_03005",
                newName: "edw_03005_N05");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N04",
                table: "edw_03005",
                newName: "edw_03005_N04");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N03",
                table: "edw_03005",
                newName: "edw_03005_N03");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N02",
                table: "edw_03005",
                newName: "edw_03005_N02");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N01",
                table: "edw_03005",
                newName: "edw_03005_N01");

            migrationBuilder.RenameColumn(
                name: "wkr_rplid",
                table: "edw_03006",
                newName: "03006id");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrrpl_N03",
                table: "edw_03006",
                newName: "edw_03006_N03");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrrpl_N02",
                table: "edw_03006",
                newName: "edw_03006_N02");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrrpl_N01",
                table: "edw_03006",
                newName: "edw_03006_N01");

            migrationBuilder.RenameColumn(
                name: "wkr_dlvid",
                table: "edw_03004",
                newName: "03004id");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrdlv_N02",
                table: "edw_03004",
                newName: "edw_03004_N02");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrdlv_N01",
                table: "edw_03004",
                newName: "edw_03004_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edw_03007_PK",
                table: "edw_03007",
                column: "tsktyp");

            migrationBuilder.AddPrimaryKey(
                name: "edw_03005_PK",
                table: "edw_03005",
                column: "03005id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_03006_PK",
                table: "edw_03006",
                column: "03006id");

            migrationBuilder.AddPrimaryKey(
                name: "edw_03004_PK",
                table: "edw_03004",
                column: "03004id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "edw_03007_PK",
                table: "edw_03007");

            migrationBuilder.DropPrimaryKey(
                name: "edw_03006_PK",
                table: "edw_03006");

            migrationBuilder.DropPrimaryKey(
                name: "edw_03005_PK",
                table: "edw_03005");

            migrationBuilder.DropPrimaryKey(
                name: "edw_03004_PK",
                table: "edw_03004");

            migrationBuilder.RenameTable(
                name: "edw_03007",
                newName: "edwk_wkrtyp");

            migrationBuilder.RenameTable(
                name: "edw_03006",
                newName: "edwk_wkrrpl");

            migrationBuilder.RenameTable(
                name: "edw_03005",
                newName: "edwk_wkrtsk");

            migrationBuilder.RenameTable(
                name: "edw_03004",
                newName: "edwk_wkrdlv");

            migrationBuilder.RenameColumn(
                name: "03006id",
                table: "edwk_wkrrpl",
                newName: "wkr_rplid");

            migrationBuilder.RenameIndex(
                name: "edw_03006_N03",
                table: "edwk_wkrrpl",
                newName: "edwk_wkrrpl_N03");

            migrationBuilder.RenameIndex(
                name: "edw_03006_N02",
                table: "edwk_wkrrpl",
                newName: "edwk_wkrrpl_N02");

            migrationBuilder.RenameIndex(
                name: "edw_03006_N01",
                table: "edwk_wkrrpl",
                newName: "edwk_wkrrpl_N01");

            migrationBuilder.RenameColumn(
                name: "03005id",
                table: "edwk_wkrtsk",
                newName: "wkr_tskid");

            migrationBuilder.RenameIndex(
                name: "edw_03005_N06",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N06");

            migrationBuilder.RenameIndex(
                name: "edw_03005_N05",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N05");

            migrationBuilder.RenameIndex(
                name: "edw_03005_N04",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N04");

            migrationBuilder.RenameIndex(
                name: "edw_03005_N03",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N03");

            migrationBuilder.RenameIndex(
                name: "edw_03005_N02",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N02");

            migrationBuilder.RenameIndex(
                name: "edw_03005_N01",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N01");

            migrationBuilder.RenameColumn(
                name: "03004id",
                table: "edwk_wkrdlv",
                newName: "wkr_dlvid");

            migrationBuilder.RenameIndex(
                name: "edw_03004_N02",
                table: "edwk_wkrdlv",
                newName: "edwk_wkrdlv_N02");

            migrationBuilder.RenameIndex(
                name: "edw_03004_N01",
                table: "edwk_wkrdlv",
                newName: "edwk_wkrdlv_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrtyp_PK",
                table: "edwk_wkrtyp",
                column: "tsktyp");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrrpl_PK",
                table: "edwk_wkrrpl",
                column: "wkr_rplid");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrtsk_PK",
                table: "edwk_wkrtsk",
                column: "wkr_tskid");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrdlv_PK",
                table: "edwk_wkrdlv",
                column: "wkr_dlvid");
        }
    }
}
