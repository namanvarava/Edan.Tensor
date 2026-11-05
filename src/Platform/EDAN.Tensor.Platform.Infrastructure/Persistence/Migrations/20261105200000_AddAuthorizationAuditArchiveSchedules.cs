using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>Tenant별 감사 아카이브 예약 정책 테이블을 추가합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261105200000_AddAuthorizationAuditArchiveSchedules")]
public sealed class AddAuthorizationAuditArchiveSchedules : Migration
{
    /// <summary>예약 정책과 실행 시각 조회 인덱스를 생성합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_authorization_audit_archive_schedules",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                IntervalHours = table.Column<int>(nullable: false),
                IsEnabled = table.Column<bool>(nullable: false),
                NextRunAtUtc = table.Column<DateTimeOffset>(nullable: false),
                LastRunAtUtc = table.Column<DateTimeOffset>(nullable: true),
                LeaseId = table.Column<Guid>(nullable: true),
                LeaseExpiresAtUtc = table.Column<DateTimeOffset>(nullable: true),
                Version = table.Column<long>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_authorization_audit_archive_schedules", x => x.Id);
                table.ForeignKey("FK_platform_authorization_audit_archive_schedules_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_platform_authorization_audit_archive_schedules_TenantId",
            table: "platform_authorization_audit_archive_schedules",
            column: "TenantId", unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_platform_authorization_audit_archive_schedules_IsEnabled_NextRunAtUtc",
            table: "platform_authorization_audit_archive_schedules",
            columns: new[] { "IsEnabled", "NextRunAtUtc" });
    }

    /// <summary>예약 정책 테이블을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("platform_authorization_audit_archive_schedules");

    /// <summary>현재 Migration의 기준 모델을 제공합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        PlatformDbContextModelSnapshot.ConfigureTargetModel(modelBuilder);
}
