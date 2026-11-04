using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>계층형 접근 정책과 권한 평가 감사 테이블을 추가합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261105140000_AddAuthorizationPoliciesAndAudit")]
public sealed class AddAuthorizationPoliciesAndAudit : Migration
{
    /// <summary>접근 정책과 추가 전용 감사 테이블 및 조회 인덱스를 생성합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_authorization_policies",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: true),
                ProjectId = table.Column<Guid>(nullable: true),
                SubjectId = table.Column<string>(maxLength: 200, nullable: false),
                Action = table.Column<string>(maxLength: 120, nullable: false),
                ResourceType = table.Column<string>(maxLength: 80, nullable: false),
                Effect = table.Column<string>(maxLength: 16, nullable: false),
                Priority = table.Column<int>(nullable: false),
                IsEnabled = table.Column<bool>(nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_authorization_policies", x => x.Id);
                table.CheckConstraint("CK_platform_authorization_policies_project_requires_organization", "ProjectId IS NULL OR OrganizationId IS NOT NULL");
                table.ForeignKey("FK_platform_authorization_policies_platform_tenants_TenantId", x => x.TenantId,
                    "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_platform_authorization_policies_platform_organizations_OrganizationId_TenantId",
                    x => new { x.OrganizationId, x.TenantId }, "platform_organizations", new[] { "Id", "TenantId" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_platform_authorization_policies_platform_projects_ProjectId_OrganizationId_TenantId",
                    x => new { x.ProjectId, x.OrganizationId, x.TenantId }, "platform_projects",
                    new[] { "Id", "OrganizationId", "TenantId" }, onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "platform_authorization_audit",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                SubjectId = table.Column<string>(maxLength: 200, nullable: false),
                Action = table.Column<string>(maxLength: 120, nullable: false),
                ResourceType = table.Column<string>(maxLength: 80, nullable: false),
                Scope = table.Column<string>(maxLength: 600, nullable: false),
                IsAllowed = table.Column<bool>(nullable: false),
                ReasonCode = table.Column<string>(maxLength: 80, nullable: false),
                MatchedPolicyId = table.Column<string>(maxLength: 36, nullable: true),
                OccurredAtUtc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_authorization_audit", x => x.Id);
                table.ForeignKey("FK_platform_authorization_audit_platform_tenants_TenantId", x => x.TenantId,
                    "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_platform_authorization_policies_TenantId_SubjectId_Action_ResourceType_IsEnabled",
            "platform_authorization_policies", new[] { "TenantId", "SubjectId", "Action", "ResourceType", "IsEnabled" });
        migrationBuilder.CreateIndex("IX_platform_authorization_policies_OrganizationId_TenantId",
            "platform_authorization_policies", new[] { "OrganizationId", "TenantId" });
        migrationBuilder.CreateIndex("IX_platform_authorization_policies_ProjectId_OrganizationId_TenantId",
            "platform_authorization_policies", new[] { "ProjectId", "OrganizationId", "TenantId" });
        migrationBuilder.CreateIndex("IX_platform_authorization_audit_TenantId_OccurredAtUtc",
            "platform_authorization_audit", new[] { "TenantId", "OccurredAtUtc" });
        migrationBuilder.CreateIndex("IX_platform_authorization_audit_SubjectId_OccurredAtUtc",
            "platform_authorization_audit", new[] { "SubjectId", "OccurredAtUtc" });
    }

    /// <summary>접근 정책과 권한 감사 테이블을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("platform_authorization_audit");
        migrationBuilder.DropTable("platform_authorization_policies");
    }

    /// <summary>현재 Migration의 기준 모델을 EF Core에 제공합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        PlatformDbContextModelSnapshot.ConfigureTargetModel(modelBuilder);
}
