using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>조직과 프로젝트 범위별 구성원 및 역할 테이블을 추가합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261105120000_AddScopedMemberships")]
public sealed class AddScopedMemberships : Migration
{
    /// <summary>조직·프로젝트 소속 테이블과 소유권 외래 키를 생성합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_organization_memberships",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                SubjectId = table.Column<string>(maxLength: 200, nullable: false),
                Role = table.Column<string>(maxLength: 32, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_organization_memberships", x => x.Id);
                table.ForeignKey("FK_platform_organization_memberships_platform_organizations_OrganizationId_TenantId",
                    x => new { x.OrganizationId, x.TenantId }, "platform_organizations",
                    new[] { "Id", "TenantId" }, onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "platform_project_memberships",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                ProjectId = table.Column<Guid>(nullable: false),
                SubjectId = table.Column<string>(maxLength: 200, nullable: false),
                Role = table.Column<string>(maxLength: 32, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_project_memberships", x => x.Id);
                table.ForeignKey("FK_platform_project_memberships_platform_projects_ProjectId_OrganizationId_TenantId",
                    x => new { x.ProjectId, x.OrganizationId, x.TenantId }, "platform_projects",
                    new[] { "Id", "OrganizationId", "TenantId" }, onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_platform_organization_memberships_TenantId_OrganizationId_SubjectId",
            "platform_organization_memberships", new[] { "TenantId", "OrganizationId", "SubjectId" }, unique: true);
        migrationBuilder.CreateIndex("IX_platform_project_memberships_TenantId_OrganizationId_ProjectId_SubjectId",
            "platform_project_memberships", new[] { "TenantId", "OrganizationId", "ProjectId", "SubjectId" }, unique: true);
        migrationBuilder.CreateIndex("IX_platform_organization_memberships_OrganizationId_TenantId",
            "platform_organization_memberships", new[] { "OrganizationId", "TenantId" });
        migrationBuilder.CreateIndex("IX_platform_project_memberships_ProjectId_OrganizationId_TenantId",
            "platform_project_memberships", new[] { "ProjectId", "OrganizationId", "TenantId" });
    }

    /// <summary>조직·프로젝트 소속 테이블을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("platform_project_memberships");
        migrationBuilder.DropTable("platform_organization_memberships");
    }

    /// <summary>현재 Migration의 기준 모델을 EF Core에 제공합니다.</summary>
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        PlatformDbContextModelSnapshot.ConfigureTargetModel(modelBuilder);
}
