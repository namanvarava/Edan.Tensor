using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>계층형 저장 용량 정책과 변경 감사 이력을 추가합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261105220000_AddStorageQuotaGovernance")]
public sealed class AddStorageQuotaGovernance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_storage_quota_policies",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Scope = table.Column<string>(maxLength: 24, nullable: false),
                TargetId = table.Column<Guid>(nullable: false),
                TenantLimitBytes = table.Column<long>(nullable: true),
                OrganizationLimitBytes = table.Column<long>(nullable: true),
                ProjectLimitBytes = table.Column<long>(nullable: true),
                DataSetLimitBytes = table.Column<long>(nullable: true),
                WarningThresholdPercent = table.Column<int>(nullable: false),
                PlanCode = table.Column<string>(maxLength: 64, nullable: false),
                IsEnabled = table.Column<bool>(nullable: false),
                Version = table.Column<long>(nullable: false),
                UpdatedBy = table.Column<string>(maxLength: 200, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_storage_quota_policies", x => x.Id);
                table.UniqueConstraint("AK_platform_storage_quota_policies_Id_TenantId", x => new { x.Id, x.TenantId });
                table.ForeignKey("FK_platform_storage_quota_policies_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_platform_storage_quota_policies_TenantId_Scope_TargetId",
            "platform_storage_quota_policies", new[] { "TenantId", "Scope", "TargetId" }, unique: true);
        migrationBuilder.CreateIndex("IX_platform_storage_quota_policies_TenantId_IsEnabled",
            "platform_storage_quota_policies", new[] { "TenantId", "IsEnabled" });

        migrationBuilder.CreateTable(
            name: "platform_storage_quota_policy_changes",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                PolicyId = table.Column<Guid>(nullable: false),
                ActorSubjectId = table.Column<string>(maxLength: 200, nullable: false),
                ChangeType = table.Column<string>(maxLength: 32, nullable: false),
                BeforeJson = table.Column<string>(maxLength: 4000, nullable: false),
                AfterJson = table.Column<string>(maxLength: 4000, nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_storage_quota_policy_changes", x => x.Id);
                table.ForeignKey("FK_platform_storage_quota_policy_changes_platform_storage_quota_policies_PolicyId_TenantId",
                    x => new { x.PolicyId, x.TenantId }, "platform_storage_quota_policies", new[] { "Id", "TenantId" }, onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_platform_storage_quota_policy_changes_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_platform_storage_quota_policy_changes_TenantId_OccurredAtUtc",
            "platform_storage_quota_policy_changes", new[] { "TenantId", "OccurredAtUtc" });
        migrationBuilder.CreateIndex("IX_platform_storage_quota_policy_changes_PolicyId_TenantId",
            "platform_storage_quota_policy_changes", new[] { "PolicyId", "TenantId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("platform_storage_quota_policy_changes");
        migrationBuilder.DropTable("platform_storage_quota_policies");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        PlatformDbContextModelSnapshot.ConfigureTargetModel(modelBuilder);
}
