using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.Worker
{
    /// <inheritdoc />
    public partial class ApplyApprovedNamingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_worker_tasks",
                table: "worker_tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_worker_task_types",
                table: "worker_task_types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_worker_task_replays",
                table: "worker_task_replays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_external_deliveries",
                table: "external_deliveries");

            migrationBuilder.RenameTable(
                name: "worker_tasks",
                newName: "edwk_wkrtsk");

            migrationBuilder.RenameTable(
                name: "worker_task_types",
                newName: "edwk_wkrtyp");

            migrationBuilder.RenameTable(
                name: "worker_task_replays",
                newName: "edwk_wkrrpl");

            migrationBuilder.RenameTable(
                name: "external_deliveries",
                newName: "edwk_wkrdlv");

            migrationBuilder.RenameIndex(
                name: "IX_worker_tasks_TenantId_Status_ReadyAtUtcUnixMilliseconds",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N05");

            migrationBuilder.RenameIndex(
                name: "IX_worker_tasks_TenantId_Status_CreatedAtUtcUnixMilliseconds_Id",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N06");

            migrationBuilder.RenameIndex(
                name: "IX_worker_tasks_TenantId_LeaseId",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N03");

            migrationBuilder.RenameIndex(
                name: "IX_worker_tasks_TenantId_IdempotencyKey",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N02");

            migrationBuilder.RenameIndex(
                name: "IX_worker_tasks_Status_ReadyAtUtcUnixMilliseconds_TenantId",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N04");

            migrationBuilder.RenameIndex(
                name: "IX_worker_tasks_Status_LeaseExpiresAtUtcUnixMilliseconds",
                table: "edwk_wkrtsk",
                newName: "edwk_wkrtsk_N01");

            migrationBuilder.RenameIndex(
                name: "IX_worker_task_replays_TenantId_ReplayTaskId",
                table: "edwk_wkrrpl",
                newName: "edwk_wkrrpl_N01");

            migrationBuilder.RenameIndex(
                name: "IX_worker_task_replays_TenantId_OriginalTaskId_ReplayRequestId",
                table: "edwk_wkrrpl",
                newName: "edwk_wkrrpl_N03");

            migrationBuilder.RenameIndex(
                name: "IX_worker_task_replays_TenantId_OriginalTaskId_CreatedAtUtc",
                table: "edwk_wkrrpl",
                newName: "edwk_wkrrpl_N02");

            migrationBuilder.RenameIndex(
                name: "IX_external_deliveries_TenantId_Status_CreatedAtUtc",
                table: "edwk_wkrdlv",
                newName: "edwk_wkrdlv_N02");

            migrationBuilder.RenameIndex(
                name: "IX_external_deliveries_TenantId_IdempotencyKey",
                table: "edwk_wkrdlv",
                newName: "edwk_wkrdlv_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrtsk_PK",
                table: "edwk_wkrtsk",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrtyp_PK",
                table: "edwk_wkrtyp",
                column: "TaskType");

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

            migrationBuilder.RenameTable(
                name: "edwk_wkrtyp",
                newName: "worker_task_types");

            migrationBuilder.RenameTable(
                name: "edwk_wkrtsk",
                newName: "worker_tasks");

            migrationBuilder.RenameTable(
                name: "edwk_wkrrpl",
                newName: "worker_task_replays");

            migrationBuilder.RenameTable(
                name: "edwk_wkrdlv",
                newName: "external_deliveries");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N06",
                table: "worker_tasks",
                newName: "IX_worker_tasks_TenantId_Status_CreatedAtUtcUnixMilliseconds_Id");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N05",
                table: "worker_tasks",
                newName: "IX_worker_tasks_TenantId_Status_ReadyAtUtcUnixMilliseconds");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N04",
                table: "worker_tasks",
                newName: "IX_worker_tasks_Status_ReadyAtUtcUnixMilliseconds_TenantId");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N03",
                table: "worker_tasks",
                newName: "IX_worker_tasks_TenantId_LeaseId");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N02",
                table: "worker_tasks",
                newName: "IX_worker_tasks_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrtsk_N01",
                table: "worker_tasks",
                newName: "IX_worker_tasks_Status_LeaseExpiresAtUtcUnixMilliseconds");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrrpl_N03",
                table: "worker_task_replays",
                newName: "IX_worker_task_replays_TenantId_OriginalTaskId_ReplayRequestId");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrrpl_N02",
                table: "worker_task_replays",
                newName: "IX_worker_task_replays_TenantId_OriginalTaskId_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrrpl_N01",
                table: "worker_task_replays",
                newName: "IX_worker_task_replays_TenantId_ReplayTaskId");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrdlv_N02",
                table: "external_deliveries",
                newName: "IX_external_deliveries_TenantId_Status_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edwk_wkrdlv_N01",
                table: "external_deliveries",
                newName: "IX_external_deliveries_TenantId_IdempotencyKey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_worker_task_types",
                table: "worker_task_types",
                column: "TaskType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_worker_tasks",
                table: "worker_tasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_worker_task_replays",
                table: "worker_task_replays",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_external_deliveries",
                table: "external_deliveries",
                column: "Id");
        }
    }
}
