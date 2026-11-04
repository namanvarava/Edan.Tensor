using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>접근 정책 변경자와 변경 전후 상태를 보관하는 감사 테이블을 추가합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261105160000_AddAuthorizationPolicyChangeAudit")]
public sealed class AddAuthorizationPolicyChangeAudit : Migration
{
    /// <summary>정책 변경 감사 테이블과 Tenant·정책별 조회 인덱스를 생성합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_authorization_policy_changes",
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
                table.PrimaryKey("PK_platform_authorization_policy_changes", x => x.Id);
                table.ForeignKey("FK_platform_authorization_policy_changes_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_platform_authorization_policy_changes_TenantId_OccurredAtUtc",
            "platform_authorization_policy_changes", new[] { "TenantId", "OccurredAtUtc" });
        migrationBuilder.CreateIndex("IX_platform_authorization_policy_changes_PolicyId_OccurredAtUtc",
            "platform_authorization_policy_changes", new[] { "PolicyId", "OccurredAtUtc" });
    }

    /// <summary>정책 변경 감사 테이블을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("platform_authorization_policy_changes");

    /// <summary>현재 Migration의 기준 모델을 EF Core에 제공합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        PlatformDbContextModelSnapshot.ConfigureTargetModel(modelBuilder);
}