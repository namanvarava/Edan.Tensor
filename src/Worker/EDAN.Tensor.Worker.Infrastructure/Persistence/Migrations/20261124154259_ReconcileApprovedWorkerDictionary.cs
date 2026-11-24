using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReconcileApprovedWorkerDictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "TaskType", table: "edwk_wkrtyp", newName: "tsktyp");
            migrationBuilder.RenameColumn(name: "LeaseId", table: "edwk_wkrtsk", newName: "leaseid");
            migrationBuilder.RenameColumn(name: "TaskType", table: "edwk_wkrtsk", newName: "tsktyp");
            migrationBuilder.RenameColumn(name: "PayloadReference", table: "edwk_wkrtsk", newName: "pld_ref");
            migrationBuilder.RenameColumn(name: "wkrtskid", table: "edwk_wkrtsk", newName: "wkr_tskid");
            migrationBuilder.RenameColumn(name: "wkrrplid", table: "edwk_wkrrpl", newName: "wkr_rplid");
            migrationBuilder.RenameColumn(name: "wkrdlvid", table: "edwk_wkrdlv", newName: "wkr_dlvid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "tsktyp", table: "edwk_wkrtyp", newName: "TaskType");
            migrationBuilder.RenameColumn(name: "leaseid", table: "edwk_wkrtsk", newName: "LeaseId");
            migrationBuilder.RenameColumn(name: "tsktyp", table: "edwk_wkrtsk", newName: "TaskType");
            migrationBuilder.RenameColumn(name: "pld_ref", table: "edwk_wkrtsk", newName: "PayloadReference");
            migrationBuilder.RenameColumn(name: "wkr_tskid", table: "edwk_wkrtsk", newName: "wkrtskid");
            migrationBuilder.RenameColumn(name: "wkr_rplid", table: "edwk_wkrrpl", newName: "wkrrplid");
            migrationBuilder.RenameColumn(name: "wkr_dlvid", table: "edwk_wkrdlv", newName: "wkrdlvid");
        }
    }
}
