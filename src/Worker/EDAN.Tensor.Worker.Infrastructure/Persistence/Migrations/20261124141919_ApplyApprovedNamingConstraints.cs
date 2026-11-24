using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplyApprovedNamingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_edwk_wkrtyp",
                table: "edwk_wkrtyp");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edwk_wkrtsk",
                table: "edwk_wkrtsk");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edwk_wkrrpl",
                table: "edwk_wkrrpl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edwk_wkrdlv",
                table: "edwk_wkrdlv");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrtsk_TenantId_Status_ReadyAtUtcUnixMilliseconds",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N05");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrtsk_TenantId_Status_CreatedAtUtcUnixMilliseconds_Id",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N06");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrtsk_TenantId_LeaseId",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N03");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrtsk_TenantId_IdempotencyKey",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrtsk_Status_ReadyAtUtcUnixMilliseconds_TenantId",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N04");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrtsk_Status_LeaseExpiresAtUtcUnixMilliseconds",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrrpl_TenantId_ReplayTaskId",
                table: "edwk_wkrrpl",
                newName: "edwk_wkrrpl_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrrpl_TenantId_OriginalTaskId_ReplayRequestId",
                table: "edwk_wkrrpl",
                newName: "edwk_wkrrpl_N03");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrrpl_TenantId_OriginalTaskId_CreatedAtUtc",
                table: "edwk_wkrrpl",
                newName: "edwk_wkrrpl_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrdlv_TenantId_Status_CreatedAtUtc",
                table: "edwk_wkrdlv",
                newName: "edwk_wkrdlv_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edwk_wkrdlv_TenantId_IdempotencyKey",
                table: "edwk_wkrdlv",
                newName: "edwk_wkrdlv_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrtyp_PK",
                table: "edwk_wkrtyp",
                column: "TaskType");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrtsk_PK",
                table: "edwk_wkrtsk",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrrpl_PK",
                table: "edwk_wkrrpl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrdlv_PK",
                table: "edwk_wkrdlv",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N06",
                table: "edwk_wkrtsk",
                newName: "IX_edwk_wkrtsk_TenantId_Status_CreatedAtUtcUnixMilliseconds_Id");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N05",
                table: "edwk_wkrtsk",
                newName: "IX_edwk_wkrtsk_TenantId_Status_ReadyAtUtcUnixMilliseconds");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N04",
                table: "edwk_wkrtsk",
                newName: "IX_edwk_wkrtsk_Status_ReadyAtUtcUnixMilliseconds_TenantId");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N03",
                table: "edwk_wkrtsk",
                newName: "IX_edwk_wkrtsk_TenantId_LeaseId");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N02",
                table: "edwk_wkrtsk",
                newName: "IX_edwk_wkrtsk_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N01",
                table: "edwk_wkrtsk",
                newName: "IX_edwk_wkrtsk_Status_LeaseExpiresAtUtcUnixMilliseconds");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrrpl_N03",
                table: "edwk_wkrrpl",
                newName: "IX_edwk_wkrrpl_TenantId_OriginalTaskId_ReplayRequestId");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrrpl_N02",
                table: "edwk_wkrrpl",
                newName: "IX_edwk_wkrrpl_TenantId_OriginalTaskId_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrrpl_N01",
                table: "edwk_wkrrpl",
                newName: "IX_edwk_wkrrpl_TenantId_ReplayTaskId");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrdlv_N02",
                table: "edwk_wkrdlv",
                newName: "IX_edwk_wkrdlv_TenantId_Status_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrdlv_N01",
                table: "edwk_wkrdlv",
                newName: "IX_edwk_wkrdlv_TenantId_IdempotencyKey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edwk_wkrtyp",
                table: "edwk_wkrtyp",
                column: "TaskType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edwk_wkrtsk",
                table: "edwk_wkrtsk",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edwk_wkrrpl",
                table: "edwk_wkrrpl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edwk_wkrdlv",
                table: "edwk_wkrdlv",
                column: "Id");
        }
    }
}
