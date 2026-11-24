using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.Worker
{
    /// <inheritdoc />
    public partial class ApplyApprovedColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edwk_wkrtyp",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "edwk_wkrtyp",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edwk_wkrtyp",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "desc",
                table: "edwk_wkrtyp",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "wkrid",
                table: "edwk_wkrtsk",
                newName: "WorkerId");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edwk_wkrtsk",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edwk_wkrtsk",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edwk_wkrtsk",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "rslt_ref",
                table: "edwk_wkrtsk",
                newName: "ResultReference");

            migrationBuilder.RenameColumn(
                name: "ready_utc_ms",
                table: "edwk_wkrtsk",
                newName: "ReadyAtUtcUnixMilliseconds");

            migrationBuilder.RenameColumn(
                name: "prgrs_stg",
                table: "edwk_wkrtsk",
                newName: "ProgressStage");

            migrationBuilder.RenameColumn(
                name: "prgrs_pct",
                table: "edwk_wkrtsk",
                newName: "ProgressPercent");

            migrationBuilder.RenameColumn(
                name: "prgrs_msg",
                table: "edwk_wkrtsk",
                newName: "ProgressMessage");

            migrationBuilder.RenameColumn(
                name: "nxt_attm_utc",
                table: "edwk_wkrtsk",
                newName: "NextAttemptAtUtc");

            migrationBuilder.RenameColumn(
                name: "max_attm_cnt",
                table: "edwk_wkrtsk",
                newName: "MaxAttempts");

            migrationBuilder.RenameColumn(
                name: "lease_exp_utc_ms",
                table: "edwk_wkrtsk",
                newName: "LeaseExpiresAtUtcUnixMilliseconds");

            migrationBuilder.RenameColumn(
                name: "lease_exp_utc",
                table: "edwk_wkrtsk",
                newName: "LeaseExpiresAtUtc");

            migrationBuilder.RenameColumn(
                name: "last_err_cd",
                table: "edwk_wkrtsk",
                newName: "LastErrorCode");

            migrationBuilder.RenameColumn(
                name: "idem_key",
                table: "edwk_wkrtsk",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "fence_tkn",
                table: "edwk_wkrtsk",
                newName: "FencingToken");

            migrationBuilder.RenameColumn(
                name: "crt_utc_ms",
                table: "edwk_wkrtsk",
                newName: "CreatedAtUtcUnixMilliseconds");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edwk_wkrtsk",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "edwk_wkrtsk",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ckpt_ver",
                table: "edwk_wkrtsk",
                newName: "CheckpointVersion");

            migrationBuilder.RenameColumn(
                name: "ckpt",
                table: "edwk_wkrtsk",
                newName: "Checkpoint");

            migrationBuilder.RenameColumn(
                name: "attm_cnt",
                table: "edwk_wkrtsk",
                newName: "AttemptCount");

            migrationBuilder.RenameColumn(
                name: "wkrtskid",
                table: "edwk_wkrtsk",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edwk_wkrrpl",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "rpl_tskid",
                table: "edwk_wkrrpl",
                newName: "ReplayTaskId");

            migrationBuilder.RenameColumn(
                name: "rpl_reqid",
                table: "edwk_wkrrpl",
                newName: "ReplayRequestId");

            migrationBuilder.RenameColumn(
                name: "orig_tskid",
                table: "edwk_wkrrpl",
                newName: "OriginalTaskId");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edwk_wkrrpl",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "wkrrplid",
                table: "edwk_wkrrpl",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edwk_wkrdlv",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edwk_wkrdlv",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "tgt_uri",
                table: "edwk_wkrdlv",
                newName: "TargetUri");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edwk_wkrdlv",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "pld",
                table: "edwk_wkrdlv",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "max_attm_cnt",
                table: "edwk_wkrdlv",
                newName: "MaxAttempts");

            migrationBuilder.RenameColumn(
                name: "last_err_cd",
                table: "edwk_wkrdlv",
                newName: "LastErrorCode");

            migrationBuilder.RenameColumn(
                name: "idem_key",
                table: "edwk_wkrdlv",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edwk_wkrdlv",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "edwk_wkrdlv",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "attm_cnt",
                table: "edwk_wkrdlv",
                newName: "AttemptCount");

            migrationBuilder.RenameColumn(
                name: "wkrdlvid",
                table: "edwk_wkrdlv",
                newName: "Id");
        }
    }
}
