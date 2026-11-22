using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.OracleMigrations.Migrations.Worker
{
    /// <inheritdoc />
    public partial class InitialOracleWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "external_deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TargetUri = table.Column<string>(type: "NCLOB", maxLength: 2048, nullable: false),
                    Payload = table.Column<string>(type: "NCLOB", maxLength: 1048576, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(24)", maxLength: 24, nullable: false),
                    AttemptCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MaxAttempts = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LastErrorCode = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_deliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "worker_task_replays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OriginalTaskId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ReplayTaskId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ReplayRequestId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RequestedBy = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_task_replays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "worker_task_types",
                columns: table => new
                {
                    TaskType = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_task_types", x => x.TaskType);
                });

            migrationBuilder.CreateTable(
                name: "worker_tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TaskType = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    PayloadReference = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MaxAttempts = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LeaseId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    WorkerId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    FencingToken = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    LeaseExpiresAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    Checkpoint = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: true),
                    CheckpointVersion = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ProgressPercent = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ProgressStage = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: true),
                    ProgressMessage = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    ResultReference = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    LastErrorCode = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CreatedAtUtcUnixMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    LeaseExpiresAtUtcUnixMilliseconds = table.Column<long>(type: "INTEGER", nullable: true),
                    ReadyAtUtcUnixMilliseconds = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_tasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_external_deliveries_TenantId_IdempotencyKey",
                table: "external_deliveries",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_external_deliveries_TenantId_Status_CreatedAtUtc",
                table: "external_deliveries",
                columns: new[] { "TenantId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_worker_task_replays_TenantId_OriginalTaskId_CreatedAtUtc",
                table: "worker_task_replays",
                columns: new[] { "TenantId", "OriginalTaskId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_worker_task_replays_TenantId_OriginalTaskId_ReplayRequestId",
                table: "worker_task_replays",
                columns: new[] { "TenantId", "OriginalTaskId", "ReplayRequestId" },
                unique: true,
                filter: "\"ReplayRequestId\" <> '00000000-0000-0000-0000-000000000000'");

            migrationBuilder.CreateIndex(
                name: "IX_worker_task_replays_TenantId_ReplayTaskId",
                table: "worker_task_replays",
                columns: new[] { "TenantId", "ReplayTaskId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_worker_tasks_Status_LeaseExpiresAtUtcUnixMilliseconds",
                table: "worker_tasks",
                columns: new[] { "Status", "LeaseExpiresAtUtcUnixMilliseconds" });

            migrationBuilder.CreateIndex(
                name: "IX_worker_tasks_Status_ReadyAtUtcUnixMilliseconds_TenantId",
                table: "worker_tasks",
                columns: new[] { "Status", "ReadyAtUtcUnixMilliseconds", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_worker_tasks_TenantId_IdempotencyKey",
                table: "worker_tasks",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_worker_tasks_TenantId_LeaseId",
                table: "worker_tasks",
                columns: new[] { "TenantId", "LeaseId" });

            migrationBuilder.CreateIndex(
                name: "IX_worker_tasks_TenantId_Status_CreatedAtUtcUnixMilliseconds_Id",
                table: "worker_tasks",
                columns: new[] { "TenantId", "Status", "CreatedAtUtcUnixMilliseconds", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_worker_tasks_TenantId_Status_ReadyAtUtcUnixMilliseconds",
                table: "worker_tasks",
                columns: new[] { "TenantId", "Status", "ReadyAtUtcUnixMilliseconds" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_deliveries");

            migrationBuilder.DropTable(
                name: "worker_task_replays");

            migrationBuilder.DropTable(
                name: "worker_task_types");

            migrationBuilder.DropTable(
                name: "worker_tasks");
        }
    }
}
