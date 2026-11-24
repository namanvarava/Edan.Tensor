using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class ApplyApprovedTableNamesPg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "edpf_tnt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_tnt", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "edpf_authaud",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Scope = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    MatchedPolicyId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_authaud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_authaud_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_authaud_arcjob",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ToUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ArchiveId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordCount = table.Column<int>(type: "integer", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SizeBytes = table.Column<int>(type: "integer", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_authaud_arcjob", x => x.Id);
                    table.UniqueConstraint("AK_edpf_authaud_arcjob_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_edpf_authaud_arcjob_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_authaud_arcschd",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NextRunAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastRunAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LeaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_authaud_arcschd", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_authaud_arcschd_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_authaud_rtn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RetentionDays = table.Column<int>(type: "integer", nullable: false),
                    IsLegalHold = table.Column<bool>(type: "boolean", nullable: false),
                    LegalHoldReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_authaud_rtn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_authaud_rtn_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_authaud_rtnchg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorSubjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BeforeJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AfterJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_authaud_rtnchg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_authaud_rtnchg_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_authplcy_chg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorSubjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BeforeJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AfterJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_authplcy_chg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_authplcy_chg_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_org",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_org", x => x.Id);
                    table.UniqueConstraint("AK_edpf_org_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_edpf_org_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_strqta_plcy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantLimitBytes = table.Column<long>(type: "bigint", nullable: true),
                    OrganizationLimitBytes = table.Column<long>(type: "bigint", nullable: true),
                    ProjectLimitBytes = table.Column<long>(type: "bigint", nullable: true),
                    DataSetLimitBytes = table.Column<long>(type: "bigint", nullable: true),
                    WarningThresholdPercent = table.Column<int>(type: "integer", nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_strqta_plcy", x => x.Id);
                    table.UniqueConstraint("AK_edpf_strqta_plcy_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_edpf_strqta_plcy_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_tntdbnd",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SecretKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ServerAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    DatabaseName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SchemaName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MigrationVersion = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastHealthAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_tntdbnd", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_tntdbnd_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_tntmbr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_tntmbr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_tntmbr_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_tntmut_aud",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: true),
                    Compensated = table.Column<bool>(type: "boolean", nullable: true),
                    ResultJson = table.Column<string>(type: "text", nullable: true),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_tntmut_aud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_tntmut_aud_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_tntrtnalw",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SchemaName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RoutineName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_tntrtnalw", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_tntrtnalw_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_usr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_usr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_usr_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_authaud_arcattm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: true),
                    ResultCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_authaud_arcattm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_authaud_arcattm_edpf_authaud_arcjob_JobId_TenantId",
                        columns: x => new { x.JobId, x.TenantId },
                        principalTable: "edpf_authaud_arcjob",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_authaud_arcattm_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_orgmbr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_orgmbr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_orgmbr_edpf_org_OrganizationId_TenantId",
                        columns: x => new { x.OrganizationId, x.TenantId },
                        principalTable: "edpf_org",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_prj",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_prj", x => x.Id);
                    table.UniqueConstraint("AK_edpf_prj_Id_OrganizationId_TenantId", x => new { x.Id, x.OrganizationId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_edpf_prj_edpf_org_OrganizationId_TenantId",
                        columns: x => new { x.OrganizationId, x.TenantId },
                        principalTable: "edpf_org",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_prj_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_strqta_plcychg",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorSubjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BeforeJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AfterJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_strqta_plcychg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_strqta_plcychg_edpf_strqta_plcy_PolicyId_TenantId",
                        columns: x => new { x.PolicyId, x.TenantId },
                        principalTable: "edpf_strqta_plcy",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_strqta_plcychg_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_inv",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResendCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_inv", x => x.Id);
                    table.UniqueConstraint("AK_edpf_inv_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_edpf_inv_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_inv_edpf_usr_UserId",
                        column: x => x.UserId,
                        principalTable: "edpf_usr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_authplcy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Effect = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_authplcy", x => x.Id);
                    table.CheckConstraint("CK_platform_authorization_policies_project_requires_organizati~", "\"ProjectId\" IS NULL OR \"OrganizationId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_edpf_authplcy_edpf_org_OrganizationId_TenantId",
                        columns: x => new { x.OrganizationId, x.TenantId },
                        principalTable: "edpf_org",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_authplcy_edpf_prj_ProjectId_OrganizationId_TenantId",
                        columns: x => new { x.ProjectId, x.OrganizationId, x.TenantId },
                        principalTable: "edpf_prj",
                        principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_authplcy_edpf_tnt_TenantId",
                        column: x => x.TenantId,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_plcy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    Key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_plcy", x => x.Id);
                    table.CheckConstraint("CK_edpf_plcy_project_requires_organization", "project_id IS NULL OR organization_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_edpf_plcy_edpf_org_organization_id_tenant_id",
                        columns: x => new { x.organization_id, x.tenant_id },
                        principalTable: "edpf_org",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_plcy_edpf_prj_project_id_organization_id_tenant_id",
                        columns: x => new { x.project_id, x.organization_id, x.tenant_id },
                        principalTable: "edpf_prj",
                        principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_plcy_edpf_tnt_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_prjmbr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_prjmbr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edpf_prjmbr_edpf_prj_ProjectId_OrganizationId_TenantId",
                        columns: x => new { x.ProjectId, x.OrganizationId, x.TenantId },
                        principalTable: "edpf_prj",
                        principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "edpf_rsrc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ResourceType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edpf_rsrc", x => x.Id);
                    table.CheckConstraint("CK_edpf_rsrc_project_requires_organization", "project_id IS NULL OR organization_id IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_edpf_rsrc_edpf_org_organization_id_tenant_id",
                        columns: x => new { x.organization_id, x.tenant_id },
                        principalTable: "edpf_org",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_rsrc_edpf_prj_project_id_organization_id_tenant_id",
                        columns: x => new { x.project_id, x.organization_id, x.tenant_id },
                        principalTable: "edpf_prj",
                        principalColumns: new[] { "Id", "OrganizationId", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_edpf_rsrc_edpf_tnt_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "edpf_tnt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authaud_SubjectId_OccurredAtUtc",
                table: "edpf_authaud",
                columns: new[] { "SubjectId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authaud_TenantId_OccurredAtUtc",
                table: "edpf_authaud",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authaud_arcattm_JobId_TenantId",
                table: "edpf_authaud_arcattm",
                columns: new[] { "JobId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authaud_arcattm_TenantId_JobId_StartedAtUtc",
                table: "edpf_authaud_arcattm",
                columns: new[] { "TenantId", "JobId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authaud_arcjob_TenantId_CreatedAtUtc",
                table: "edpf_authaud_arcjob",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authaud_arcschd_IsEnabled_NextRunAtUtc",
                table: "edpf_authaud_arcschd",
                columns: new[] { "IsEnabled", "NextRunAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authaud_arcschd_TenantId",
                table: "edpf_authaud_arcschd",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authaud_rtn_TenantId",
                table: "edpf_authaud_rtn",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authaud_rtnchg_TenantId_OccurredAtUtc",
                table: "edpf_authaud_rtnchg",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authplcy_OrganizationId_TenantId",
                table: "edpf_authplcy",
                columns: new[] { "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authplcy_ProjectId_OrganizationId_TenantId",
                table: "edpf_authplcy",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authplcy_TenantId_SubjectId_Action_ResourceType_IsEnab~",
                table: "edpf_authplcy",
                columns: new[] { "TenantId", "SubjectId", "Action", "ResourceType", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authplcy_chg_PolicyId_OccurredAtUtc",
                table: "edpf_authplcy_chg",
                columns: new[] { "PolicyId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_authplcy_chg_TenantId_OccurredAtUtc",
                table: "edpf_authplcy_chg",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_inv_TenantId_Status_CreatedAtUtc",
                table: "edpf_inv",
                columns: new[] { "TenantId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_inv_TenantId_UserId",
                table: "edpf_inv",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_inv_TokenHash",
                table: "edpf_inv",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_inv_UserId",
                table: "edpf_inv",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_edpf_org_TenantId",
                table: "edpf_org",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_edpf_org_TenantId_Code",
                table: "edpf_org",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_orgmbr_OrganizationId_TenantId",
                table: "edpf_orgmbr",
                columns: new[] { "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_orgmbr_TenantId_OrganizationId_SubjectId",
                table: "edpf_orgmbr",
                columns: new[] { "TenantId", "OrganizationId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_plcy_Key_Version",
                table: "edpf_plcy",
                columns: new[] { "Key", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_plcy_organization_id_tenant_id",
                table: "edpf_plcy",
                columns: new[] { "organization_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_plcy_project_id_organization_id_tenant_id",
                table: "edpf_plcy",
                columns: new[] { "project_id", "organization_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_plcy_tenant_id",
                table: "edpf_plcy",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_edpf_prj_OrganizationId_Code",
                table: "edpf_prj",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_prj_OrganizationId_TenantId",
                table: "edpf_prj",
                columns: new[] { "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_prj_TenantId_OrganizationId",
                table: "edpf_prj",
                columns: new[] { "TenantId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_prjmbr_ProjectId_OrganizationId_TenantId",
                table: "edpf_prjmbr",
                columns: new[] { "ProjectId", "OrganizationId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_prjmbr_TenantId_OrganizationId_ProjectId_SubjectId",
                table: "edpf_prjmbr",
                columns: new[] { "TenantId", "OrganizationId", "ProjectId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_rsrc_organization_id_tenant_id",
                table: "edpf_rsrc",
                columns: new[] { "organization_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_rsrc_project_id_organization_id_tenant_id",
                table: "edpf_rsrc",
                columns: new[] { "project_id", "organization_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_rsrc_tenant_id",
                table: "edpf_rsrc",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_edpf_strqta_plcy_TenantId_IsEnabled",
                table: "edpf_strqta_plcy",
                columns: new[] { "TenantId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_strqta_plcy_TenantId_Scope_TargetId",
                table: "edpf_strqta_plcy",
                columns: new[] { "TenantId", "Scope", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_strqta_plcychg_PolicyId_TenantId",
                table: "edpf_strqta_plcychg",
                columns: new[] { "PolicyId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_strqta_plcychg_TenantId_OccurredAtUtc",
                table: "edpf_strqta_plcychg",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_tnt_Code",
                table: "edpf_tnt",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_tntdbnd_TenantId_Provider",
                table: "edpf_tntdbnd",
                columns: new[] { "TenantId", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_edpf_tntdbnd_TenantId_Status",
                table: "edpf_tntdbnd",
                columns: new[] { "TenantId", "Status" },
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_edpf_tntmbr_TenantId_SubjectId",
                table: "edpf_tntmbr",
                columns: new[] { "TenantId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_tntmut_aud_TenantId_IdempotencyKey",
                table: "edpf_tntmut_aud",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_tntrtnalw_TenantId_SchemaName_RoutineName_Kind",
                table: "edpf_tntrtnalw",
                columns: new[] { "TenantId", "SchemaName", "RoutineName", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_edpf_usr_TenantId_Email",
                table: "edpf_usr",
                columns: new[] { "TenantId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "edpf_authaud");

            migrationBuilder.DropTable(
                name: "edpf_authaud_arcattm");

            migrationBuilder.DropTable(
                name: "edpf_authaud_arcschd");

            migrationBuilder.DropTable(
                name: "edpf_authaud_rtn");

            migrationBuilder.DropTable(
                name: "edpf_authaud_rtnchg");

            migrationBuilder.DropTable(
                name: "edpf_authplcy");

            migrationBuilder.DropTable(
                name: "edpf_authplcy_chg");

            migrationBuilder.DropTable(
                name: "edpf_inv");

            migrationBuilder.DropTable(
                name: "edpf_orgmbr");

            migrationBuilder.DropTable(
                name: "edpf_plcy");

            migrationBuilder.DropTable(
                name: "edpf_prjmbr");

            migrationBuilder.DropTable(
                name: "edpf_rsrc");

            migrationBuilder.DropTable(
                name: "edpf_strqta_plcychg");

            migrationBuilder.DropTable(
                name: "edpf_tntdbnd");

            migrationBuilder.DropTable(
                name: "edpf_tntmbr");

            migrationBuilder.DropTable(
                name: "edpf_tntmut_aud");

            migrationBuilder.DropTable(
                name: "edpf_tntrtnalw");

            migrationBuilder.DropTable(
                name: "edpf_authaud_arcjob");

            migrationBuilder.DropTable(
                name: "edpf_usr");

            migrationBuilder.DropTable(
                name: "edpf_prj");

            migrationBuilder.DropTable(
                name: "edpf_strqta_plcy");

            migrationBuilder.DropTable(
                name: "edpf_org");

            migrationBuilder.DropTable(
                name: "edpf_tnt");
        }
    }
}
