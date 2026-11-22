using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.OracleMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class InitialOraclePlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_authorization_audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SubjectId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    ResourceType = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Scope = table.Column<string>(type: "NVARCHAR2(600)", maxLength: 600, nullable: false),
                    IsAllowed = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    ReasonCode = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    MatchedPolicyId = table.Column<string>(type: "NVARCHAR2(36)", maxLength: 36, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_authorization_audit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_authorization_audit_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_authorization_audit_archive_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FromUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    ToUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    RequestedBy = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    ArchiveId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    RecordCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Sha256 = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    SizeBytes = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    FailureReason = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    AttemptCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_authorization_audit_archive_jobs", x => x.Id);
                    table.UniqueConstraint("AK_platform_authorization_audit_archive_jobs_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_platform_authorization_audit_archive_jobs_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_authorization_audit_archive_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    IntervalHours = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    NextRunAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    LastRunAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    LeaseId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    LeaseExpiresAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_authorization_audit_archive_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_authorization_audit_archive_schedules_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_authorization_audit_retention",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RetentionDays = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsLegalHold = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    LegalHoldReason = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_authorization_audit_retention", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_authorization_audit_retention_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_authorization_audit_retention_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PolicyId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ActorSubjectId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    ChangeType = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    BeforeJson = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    AfterJson = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_authorization_audit_retention_changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_authorization_audit_retention_changes_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_authorization_policy_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PolicyId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ActorSubjectId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    ChangeType = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    BeforeJson = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    AfterJson = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_authorization_policy_changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_authorization_policy_changes_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_organizations", x => x.Id);
                    table.UniqueConstraint("AK_platform_organizations_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_platform_organizations_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_storage_quota_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Scope = table.Column<string>(type: "NVARCHAR2(24)", maxLength: 24, nullable: false),
                    TargetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantLimitBytes = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    OrganizationLimitBytes = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    ProjectLimitBytes = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    DataSetLimitBytes = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    WarningThresholdPercent = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PlanCode = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    IsEnabled = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_storage_quota_policies", x => x.Id);
                    table.UniqueConstraint("AK_platform_storage_quota_policies_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_platform_storage_quota_policies_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_tenant_database_bindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Provider = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    Username = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    SecretKey = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    ServerAddress = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: true),
                    Port = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    DatabaseName = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: true),
                    SchemaName = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    MigrationVersion = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: true),
                    LastHealthAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_tenant_database_bindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_tenant_database_bindings_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_tenant_database_routine_allowlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Kind = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    SchemaName = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    RoutineName = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    IsEnabled = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_tenant_database_routine_allowlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_tenant_database_routine_allowlists_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_tenant_memberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SubjectId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_tenant_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_tenant_memberships_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_users_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_authorization_audit_archive_attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    JobId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    AttemptNumber = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Operation = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    FinishedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "BOOLEAN", nullable: true),
                    ResultCode = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_authorization_audit_archive_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_authorization_audit_archive_attempts_platform_authorization_audit_archive_jobs_JobId_TenantId",
                        columns: x => new { x.JobId, x.TenantId },
                        principalTable: "platform_authorization_audit_archive_jobs",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_authorization_audit_archive_attempts_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_organization_memberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SubjectId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_organization_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_organization_memberships_platform_organizations_OrganizationId_TenantId",
                        columns: x => new { x.OrganizationId, x.TenantId },
                        principalTable: "platform_organizations",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Code = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_projects", x => x.Id);
                    table.UniqueConstraint("AK_platform_projects_Id_OrganizationId_TenantId", x => new { x.Id, x.OrganizationId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_platform_projects_platform_organizations_OrganizationId_TenantId",
                        columns: x => new { x.OrganizationId, x.TenantId },
                        principalTable: "platform_organizations",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_projects_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_storage_quota_policy_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PolicyId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ActorSubjectId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    ChangeType = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    BeforeJson = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    AfterJson = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_storage_quota_policy_changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_storage_quota_policy_changes_platform_storage_quota_policies_PolicyId_TenantId",
                        columns: x => new { x.PolicyId, x.TenantId },
                        principalTable: "platform_storage_quota_policies",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_storage_quota_policy_changes_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    TokenHash = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    AcceptedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    ResendCount = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_invitations", x => x.Id);
                    table.UniqueConstraint("AK_platform_invitations_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_platform_invitations_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_invitations_platform_users_UserId",
                        column: x => x.UserId,
                        principalTable: "platform_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_authorization_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    ProjectId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    SubjectId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    ResourceType = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Effect = table.Column<string>(type: "NVARCHAR2(16)", maxLength: 16, nullable: false),
                    Priority = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_authorization_policies", x => x.Id);
                    table.CheckConstraint("CK_platform_authorization_policies_project_requires_organization", "\"ProjectId\" IS NULL OR \"OrganizationId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_platform_authorization_policies_platform_organizations_OrganizationId_TenantId",
                        columns: x => new { x.OrganizationId, x.TenantId },
                        principalTable: "platform_organizations",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_authorization_policies_platform_projects_ProjectId_OrganizationId_TenantId",
                        columns: x => new { x.ProjectId, x.OrganizationId, x.TenantId },
                        principalTable: "platform_projects",
                        principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_authorization_policies_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    tenant_id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    organization_id = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    project_id = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    Key = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Value = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    Version = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_policies", x => x.Id);
                    table.CheckConstraint("CK_platform_policies_project_requires_organization", "project_id IS NULL OR organization_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_platform_policies_platform_organizations_organization_id_tenant_id",
                        columns: x => new { x.organization_id, x.tenant_id },
                        principalTable: "platform_organizations",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_policies_platform_projects_project_id_organization_id_tenant_id",
                        columns: x => new { x.project_id, x.organization_id, x.tenant_id },
                        principalTable: "platform_projects",
                        principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_policies_platform_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_project_memberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProjectId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SubjectId = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_project_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_project_memberships_platform_projects_ProjectId_OrganizationId_TenantId",
                        columns: x => new { x.ProjectId, x.OrganizationId, x.TenantId },
                        principalTable: "platform_projects",
                        principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    tenant_id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    organization_id = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    project_id = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    ResourceType = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_resources", x => x.Id);
                    table.CheckConstraint("CK_platform_resources_project_requires_organization", "project_id IS NULL OR organization_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_platform_resources_platform_organizations_organization_id_tenant_id",
                        columns: x => new { x.organization_id, x.tenant_id },
                        principalTable: "platform_organizations",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_resources_platform_projects_project_id_organization_id_tenant_id",
                        columns: x => new { x.project_id, x.organization_id, x.tenant_id },
                        principalTable: "platform_projects",
                        principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_platform_resources_platform_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_audit_SubjectId_OccurredAtUtc",
                table: "platform_authorization_audit",
                columns: new[] { "SubjectId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_audit_TenantId_OccurredAtUtc",
                table: "platform_authorization_audit",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_audit_archive_attempts_JobId_TenantId",
                table: "platform_authorization_audit_archive_attempts",
                columns: new[] { "JobId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_audit_archive_attempts_TenantId_JobId_StartedAtUtc",
                table: "platform_authorization_audit_archive_attempts",
                columns: new[] { "TenantId", "JobId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_audit_archive_jobs_TenantId_CreatedAtUtc",
                table: "platform_authorization_audit_archive_jobs",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_audit_archive_schedules_IsEnabled_NextRunAtUtc",
                table: "platform_authorization_audit_archive_schedules",
                columns: new[] { "IsEnabled", "NextRunAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_audit_archive_schedules_TenantId",
                table: "platform_authorization_audit_archive_schedules",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_audit_retention_TenantId",
                table: "platform_authorization_audit_retention",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_audit_retention_changes_TenantId_OccurredAtUtc",
                table: "platform_authorization_audit_retention_changes",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_policies_OrganizationId_TenantId",
                table: "platform_authorization_policies",
                columns: new[] { "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_policies_ProjectId_OrganizationId_TenantId",
                table: "platform_authorization_policies",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_policies_TenantId_SubjectId_Action_ResourceType_IsEnabled",
                table: "platform_authorization_policies",
                columns: new[] { "TenantId", "SubjectId", "Action", "ResourceType", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_policy_changes_PolicyId_OccurredAtUtc",
                table: "platform_authorization_policy_changes",
                columns: new[] { "PolicyId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_authorization_policy_changes_TenantId_OccurredAtUtc",
                table: "platform_authorization_policy_changes",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_invitations_TenantId_Status_CreatedAtUtc",
                table: "platform_invitations",
                columns: new[] { "TenantId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_invitations_TenantId_UserId",
                table: "platform_invitations",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_invitations_TokenHash",
                table: "platform_invitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_invitations_UserId",
                table: "platform_invitations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_organization_memberships_OrganizationId_TenantId",
                table: "platform_organization_memberships",
                columns: new[] { "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_organization_memberships_TenantId_OrganizationId_SubjectId",
                table: "platform_organization_memberships",
                columns: new[] { "TenantId", "OrganizationId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_organizations_TenantId",
                table: "platform_organizations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_organizations_TenantId_Code",
                table: "platform_organizations",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_policies_Key_Version",
                table: "platform_policies",
                columns: new[] { "Key", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_policies_organization_id_tenant_id",
                table: "platform_policies",
                columns: new[] { "organization_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_policies_project_id_organization_id_tenant_id",
                table: "platform_policies",
                columns: new[] { "project_id", "organization_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_policies_tenant_id",
                table: "platform_policies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_project_memberships_ProjectId_OrganizationId_TenantId",
                table: "platform_project_memberships",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_project_memberships_TenantId_OrganizationId_ProjectId_SubjectId",
                table: "platform_project_memberships",
                columns: new[] { "TenantId", "OrganizationId", "ProjectId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_projects_OrganizationId_Code",
                table: "platform_projects",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_projects_OrganizationId_TenantId",
                table: "platform_projects",
                columns: new[] { "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_projects_TenantId_OrganizationId",
                table: "platform_projects",
                columns: new[] { "TenantId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_resources_organization_id_tenant_id",
                table: "platform_resources",
                columns: new[] { "organization_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_resources_project_id_organization_id_tenant_id",
                table: "platform_resources",
                columns: new[] { "project_id", "organization_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_resources_tenant_id",
                table: "platform_resources",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_storage_quota_policies_TenantId_IsEnabled",
                table: "platform_storage_quota_policies",
                columns: new[] { "TenantId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_storage_quota_policies_TenantId_Scope_TargetId",
                table: "platform_storage_quota_policies",
                columns: new[] { "TenantId", "Scope", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_storage_quota_policy_changes_PolicyId_TenantId",
                table: "platform_storage_quota_policy_changes",
                columns: new[] { "PolicyId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_storage_quota_policy_changes_TenantId_OccurredAtUtc",
                table: "platform_storage_quota_policy_changes",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_tenant_database_bindings_TenantId_Provider",
                table: "platform_tenant_database_bindings",
                columns: new[] { "TenantId", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_tenant_database_bindings_TenantId_Status",
                table: "platform_tenant_database_bindings",
                columns: new[] { "TenantId", "Status" },
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_platform_tenant_database_routine_allowlists_TenantId_SchemaName_RoutineName_Kind",
                table: "platform_tenant_database_routine_allowlists",
                columns: new[] { "TenantId", "SchemaName", "RoutineName", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_tenant_memberships_TenantId_SubjectId",
                table: "platform_tenant_memberships",
                columns: new[] { "TenantId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_tenants_Code",
                table: "platform_tenants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_users_TenantId_Email",
                table: "platform_users",
                columns: new[] { "TenantId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_authorization_audit");

            migrationBuilder.DropTable(
                name: "platform_authorization_audit_archive_attempts");

            migrationBuilder.DropTable(
                name: "platform_authorization_audit_archive_schedules");

            migrationBuilder.DropTable(
                name: "platform_authorization_audit_retention");

            migrationBuilder.DropTable(
                name: "platform_authorization_audit_retention_changes");

            migrationBuilder.DropTable(
                name: "platform_authorization_policies");

            migrationBuilder.DropTable(
                name: "platform_authorization_policy_changes");

            migrationBuilder.DropTable(
                name: "platform_invitations");

            migrationBuilder.DropTable(
                name: "platform_organization_memberships");

            migrationBuilder.DropTable(
                name: "platform_policies");

            migrationBuilder.DropTable(
                name: "platform_project_memberships");

            migrationBuilder.DropTable(
                name: "platform_resources");

            migrationBuilder.DropTable(
                name: "platform_storage_quota_policy_changes");

            migrationBuilder.DropTable(
                name: "platform_tenant_database_bindings");

            migrationBuilder.DropTable(
                name: "platform_tenant_database_routine_allowlists");

            migrationBuilder.DropTable(
                name: "platform_tenant_memberships");

            migrationBuilder.DropTable(
                name: "platform_authorization_audit_archive_jobs");

            migrationBuilder.DropTable(
                name: "platform_users");

            migrationBuilder.DropTable(
                name: "platform_projects");

            migrationBuilder.DropTable(
                name: "platform_storage_quota_policies");

            migrationBuilder.DropTable(
                name: "platform_organizations");

            migrationBuilder.DropTable(
                name: "platform_tenants");
        }
    }
}
