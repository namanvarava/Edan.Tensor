using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Worker
{
    /// <inheritdoc />
    public partial class ApplyApprovedSchemaNaming : Migration
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

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "edwk_wkrtsk",
                newName: "wkrid");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edwk_wkrtsk",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edwk_wkrtsk",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edwk_wkrtsk",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "ResultReference",
                table: "edwk_wkrtsk",
                newName: "rslt_ref");

            migrationBuilder.RenameColumn(
                name: "ReadyAtUtcUnixMilliseconds",
                table: "edwk_wkrtsk",
                newName: "ready_utc_ms");

            migrationBuilder.RenameColumn(
                name: "ProgressStage",
                table: "edwk_wkrtsk",
                newName: "prgrs_stg");

            migrationBuilder.RenameColumn(
                name: "ProgressPercent",
                table: "edwk_wkrtsk",
                newName: "prgrs_pct");

            migrationBuilder.RenameColumn(
                name: "ProgressMessage",
                table: "edwk_wkrtsk",
                newName: "prgrs_msg");

            migrationBuilder.RenameColumn(
                name: "NextAttemptAtUtc",
                table: "edwk_wkrtsk",
                newName: "nxt_attm_utc");

            migrationBuilder.RenameColumn(
                name: "MaxAttempts",
                table: "edwk_wkrtsk",
                newName: "max_attm_cnt");

            migrationBuilder.RenameColumn(
                name: "LeaseExpiresAtUtcUnixMilliseconds",
                table: "edwk_wkrtsk",
                newName: "lease_exp_utc_ms");

            migrationBuilder.RenameColumn(
                name: "LeaseExpiresAtUtc",
                table: "edwk_wkrtsk",
                newName: "lease_exp_utc");

            migrationBuilder.RenameColumn(
                name: "LastErrorCode",
                table: "edwk_wkrtsk",
                newName: "last_err_cd");

            migrationBuilder.RenameColumn(
                name: "IdempotencyKey",
                table: "edwk_wkrtsk",
                newName: "idem_key");

            migrationBuilder.RenameColumn(
                name: "FencingToken",
                table: "edwk_wkrtsk",
                newName: "fence_tkn");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtcUnixMilliseconds",
                table: "edwk_wkrtsk",
                newName: "crt_utc_ms");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edwk_wkrtsk",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                table: "edwk_wkrtsk",
                newName: "cmp_utc");

            migrationBuilder.RenameColumn(
                name: "CheckpointVersion",
                table: "edwk_wkrtsk",
                newName: "ckpt_ver");

            migrationBuilder.RenameColumn(
                name: "Checkpoint",
                table: "edwk_wkrtsk",
                newName: "ckpt");

            migrationBuilder.RenameColumn(
                name: "AttemptCount",
                table: "edwk_wkrtsk",
                newName: "attm_cnt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edwk_wkrtsk",
                newName: "wkrtskid");

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

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edwk_wkrtyp",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "edwk_wkrtyp",
                newName: "upd_utc");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "edwk_wkrtyp",
                newName: "ena_yn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "edwk_wkrtyp",
                newName: "desc");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edwk_wkrrpl",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "ReplayTaskId",
                table: "edwk_wkrrpl",
                newName: "rpl_tskid");

            migrationBuilder.RenameColumn(
                name: "ReplayRequestId",
                table: "edwk_wkrrpl",
                newName: "rpl_reqid");

            migrationBuilder.RenameColumn(
                name: "OriginalTaskId",
                table: "edwk_wkrrpl",
                newName: "orig_tskid");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edwk_wkrrpl",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edwk_wkrrpl",
                newName: "wkrrplid");

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

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "edwk_wkrdlv",
                newName: "ver");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "edwk_wkrdlv",
                newName: "tntid");

            migrationBuilder.RenameColumn(
                name: "TargetUri",
                table: "edwk_wkrdlv",
                newName: "tgt_uri");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "edwk_wkrdlv",
                newName: "st");

            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "edwk_wkrdlv",
                newName: "pld");

            migrationBuilder.RenameColumn(
                name: "MaxAttempts",
                table: "edwk_wkrdlv",
                newName: "max_attm_cnt");

            migrationBuilder.RenameColumn(
                name: "LastErrorCode",
                table: "edwk_wkrdlv",
                newName: "last_err_cd");

            migrationBuilder.RenameColumn(
                name: "IdempotencyKey",
                table: "edwk_wkrdlv",
                newName: "idem_key");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "edwk_wkrdlv",
                newName: "crt_utc");

            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                table: "edwk_wkrdlv",
                newName: "cmp_utc");

            migrationBuilder.RenameColumn(
                name: "AttemptCount",
                table: "edwk_wkrdlv",
                newName: "attm_cnt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edwk_wkrdlv",
                newName: "wkrdlvid");

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
                column: "wkrtskid");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrtyp_PK",
                table: "edwk_wkrtyp",
                column: "TaskType");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrrpl_PK",
                table: "edwk_wkrrpl",
                column: "wkrrplid");

            migrationBuilder.AddPrimaryKey(
                name: "edwk_wkrdlv_PK",
                table: "edwk_wkrdlv",
                column: "wkrdlvid");
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

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "worker_task_types",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "worker_task_types",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "worker_task_types",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "desc",
                table: "worker_task_types",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "wkrid",
                table: "worker_tasks",
                newName: "WorkerId");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "worker_tasks",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "worker_tasks",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "worker_tasks",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "rslt_ref",
                table: "worker_tasks",
                newName: "ResultReference");

            migrationBuilder.RenameColumn(
                name: "ready_utc_ms",
                table: "worker_tasks",
                newName: "ReadyAtUtcUnixMilliseconds");

            migrationBuilder.RenameColumn(
                name: "prgrs_stg",
                table: "worker_tasks",
                newName: "ProgressStage");

            migrationBuilder.RenameColumn(
                name: "prgrs_pct",
                table: "worker_tasks",
                newName: "ProgressPercent");

            migrationBuilder.RenameColumn(
                name: "prgrs_msg",
                table: "worker_tasks",
                newName: "ProgressMessage");

            migrationBuilder.RenameColumn(
                name: "nxt_attm_utc",
                table: "worker_tasks",
                newName: "NextAttemptAtUtc");

            migrationBuilder.RenameColumn(
                name: "max_attm_cnt",
                table: "worker_tasks",
                newName: "MaxAttempts");

            migrationBuilder.RenameColumn(
                name: "lease_exp_utc_ms",
                table: "worker_tasks",
                newName: "LeaseExpiresAtUtcUnixMilliseconds");

            migrationBuilder.RenameColumn(
                name: "lease_exp_utc",
                table: "worker_tasks",
                newName: "LeaseExpiresAtUtc");

            migrationBuilder.RenameColumn(
                name: "last_err_cd",
                table: "worker_tasks",
                newName: "LastErrorCode");

            migrationBuilder.RenameColumn(
                name: "idem_key",
                table: "worker_tasks",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "fence_tkn",
                table: "worker_tasks",
                newName: "FencingToken");

            migrationBuilder.RenameColumn(
                name: "crt_utc_ms",
                table: "worker_tasks",
                newName: "CreatedAtUtcUnixMilliseconds");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "worker_tasks",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "worker_tasks",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ckpt_ver",
                table: "worker_tasks",
                newName: "CheckpointVersion");

            migrationBuilder.RenameColumn(
                name: "ckpt",
                table: "worker_tasks",
                newName: "Checkpoint");

            migrationBuilder.RenameColumn(
                name: "attm_cnt",
                table: "worker_tasks",
                newName: "AttemptCount");

            migrationBuilder.RenameColumn(
                name: "wkrtskid",
                table: "worker_tasks",
                newName: "Id");

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

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "worker_task_replays",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "rpl_tskid",
                table: "worker_task_replays",
                newName: "ReplayTaskId");

            migrationBuilder.RenameColumn(
                name: "rpl_reqid",
                table: "worker_task_replays",
                newName: "ReplayRequestId");

            migrationBuilder.RenameColumn(
                name: "orig_tskid",
                table: "worker_task_replays",
                newName: "OriginalTaskId");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "worker_task_replays",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "wkrrplid",
                table: "worker_task_replays",
                newName: "Id");

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

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "external_deliveries",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "external_deliveries",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "tgt_uri",
                table: "external_deliveries",
                newName: "TargetUri");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "external_deliveries",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "pld",
                table: "external_deliveries",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "max_attm_cnt",
                table: "external_deliveries",
                newName: "MaxAttempts");

            migrationBuilder.RenameColumn(
                name: "last_err_cd",
                table: "external_deliveries",
                newName: "LastErrorCode");

            migrationBuilder.RenameColumn(
                name: "idem_key",
                table: "external_deliveries",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "external_deliveries",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "external_deliveries",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "attm_cnt",
                table: "external_deliveries",
                newName: "AttemptCount");

            migrationBuilder.RenameColumn(
                name: "wkrdlvid",
                table: "external_deliveries",
                newName: "Id");

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
