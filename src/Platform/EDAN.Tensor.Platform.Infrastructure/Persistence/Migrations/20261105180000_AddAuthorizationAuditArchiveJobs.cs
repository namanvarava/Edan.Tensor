using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>감사 아카이브 작업과 실행 시도 이력 테이블을 추가합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261105180000_AddAuthorizationAuditArchiveJobs")]
public sealed class AddAuthorizationAuditArchiveJobs : Migration
{
    /// <summary>아카이브 작업과 실행 이력 및 Tenant 조회 인덱스를 생성합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_authorization_audit_archive_jobs",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                FromUtc = table.Column<DateTimeOffset>(nullable: false),
                ToUtc = table.Column<DateTimeOffset>(nullable: false),
                RequestedBy = table.Column<string>(maxLength: 200, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false),
                ArchiveId = table.Column<Guid>(nullable: true),
                RecordCount = table.Column<int>(nullable: false),
                Sha256 = table.Column<string>(maxLength: 64, nullable: true),
                SizeBytes = table.Column<int>(nullable: true),
                CompletedAtUtc = table.Column<DateTimeOffset>(nullable: true),
                FailureReason = table.Column<string>(maxLength: 500, nullable: true),
                AttemptCount = table.Column<int>(nullable: false),
                Version = table.Column<long>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_authorization_audit_archive_jobs", x => x.Id);
                table.UniqueConstraint("AK_platform_authorization_audit_archive_jobs_Id_TenantId", x => new { x.Id, x.TenantId });
                table.ForeignKey("FK_platform_authorization_audit_archive_jobs_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "platform_authorization_audit_archive_attempts",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                JobId = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                AttemptNumber = table.Column<int>(nullable: false),
                Operation = table.Column<string>(maxLength: 32, nullable: false),
                StartedAtUtc = table.Column<DateTimeOffset>(nullable: false),
                FinishedAtUtc = table.Column<DateTimeOffset>(nullable: true),
                IsSuccessful = table.Column<bool>(nullable: true),
                ResultCode = table.Column<string>(maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_authorization_audit_archive_attempts", x => x.Id);
                table.ForeignKey("FK_platform_authorization_audit_archive_attempts_platform_authorization_audit_archive_jobs_JobId_TenantId",
                    x => new { x.JobId, x.TenantId }, "platform_authorization_audit_archive_jobs",
                    new[] { "Id", "TenantId" }, onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_platform_authorization_audit_archive_attempts_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_platform_authorization_audit_archive_jobs_TenantId_CreatedAtUtc",
            "platform_authorization_audit_archive_jobs", new[] { "TenantId", "CreatedAtUtc" });
        migrationBuilder.CreateIndex("IX_platform_authorization_audit_archive_attempts_TenantId_JobId_StartedAtUtc",
            "platform_authorization_audit_archive_attempts", new[] { "TenantId", "JobId", "StartedAtUtc" });
        migrationBuilder.CreateIndex("IX_platform_authorization_audit_archive_attempts_JobId_TenantId",
            "platform_authorization_audit_archive_attempts", new[] { "JobId", "TenantId" });
    }

    /// <summary>아카이브 작업과 실행 이력 테이블을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("platform_authorization_audit_archive_attempts");
        migrationBuilder.DropTable("platform_authorization_audit_archive_jobs");
    }

    /// <summary>현재 Migration의 기준 모델을 EF Core에 제공합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        PlatformDbContextModelSnapshot.ConfigureTargetModel(modelBuilder);
}
