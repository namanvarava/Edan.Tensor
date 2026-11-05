using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>테넌트별 감사 보존 정책과 변경 이력 테이블을 추가합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261105160000_AddAuthorizationAuditRetention")]
public sealed class AddAuthorizationAuditRetention : Migration
{
    /// <summary>보존 정책과 추가 전용 변경 이력 테이블 및 인덱스를 생성합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_authorization_audit_retention",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                RetentionDays = table.Column<int>(nullable: false),
                IsLegalHold = table.Column<bool>(nullable: false),
                LegalHoldReason = table.Column<string>(maxLength: 500, nullable: true),
                UpdatedBy = table.Column<string>(maxLength: 200, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(nullable: false),
                Version = table.Column<long>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_authorization_audit_retention", x => x.Id);
                table.ForeignKey("FK_platform_authorization_audit_retention_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "platform_authorization_audit_retention_changes",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                PolicyId = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                ActorSubjectId = table.Column<string>(maxLength: 200, nullable: false),
                ChangeType = table.Column<string>(maxLength: 32, nullable: false),
                BeforeJson = table.Column<string>(maxLength: 4000, nullable: false),
                AfterJson = table.Column<string>(maxLength: 4000, nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_authorization_audit_retention_changes", x => x.Id);
                table.ForeignKey("FK_platform_authorization_audit_retention_changes_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_platform_authorization_audit_retention_TenantId",
            "platform_authorization_audit_retention", "TenantId", unique: true);
        migrationBuilder.CreateIndex("IX_platform_authorization_audit_retention_changes_TenantId_OccurredAtUtc",
            "platform_authorization_audit_retention_changes", new[] { "TenantId", "OccurredAtUtc" });
    }

    /// <summary>보존 정책 테이블과 변경 이력 테이블을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("platform_authorization_audit_retention_changes");
        migrationBuilder.DropTable("platform_authorization_audit_retention");
    }

    /// <summary>현재 Migration의 기준 모델을 EF Core에 제공합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        PlatformDbContextModelSnapshot.ConfigureTargetModel(modelBuilder);
}
