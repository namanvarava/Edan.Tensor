using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Worker
{
    /// <inheritdoc />
    public partial class _AuditSequentialMappingV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "wkr_rplid",
                table: "edw_03006",
                newName: "03006id");

            migrationBuilder.RenameColumn(
                name: "wkr_tskid",
                table: "edw_03005",
                newName: "03005id");

            migrationBuilder.RenameColumn(
                name: "wkr_dlvid",
                table: "edw_03004",
                newName: "03004id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "03006id",
                table: "edw_03006",
                newName: "wkr_rplid");

            migrationBuilder.RenameColumn(
                name: "03005id",
                table: "edw_03005",
                newName: "wkr_tskid");

            migrationBuilder.RenameColumn(
                name: "03004id",
                table: "edw_03004",
                newName: "wkr_dlvid");
        }
    }
}
