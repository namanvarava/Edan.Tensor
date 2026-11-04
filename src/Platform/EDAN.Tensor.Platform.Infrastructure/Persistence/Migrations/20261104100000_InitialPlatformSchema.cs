using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>Platform 초기 테이블과 테넌트 소유권 제약을 생성합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261104100000_InitialPlatformSchema")]
public sealed class InitialPlatformSchema : Migration
{
    /// <summary>관계형 공급자에 맞는 초기 테이블과 복합 외래 키를 생성합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_tenants",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Code = table.Column<string>(maxLength: 64, nullable: false),
                Name = table.Column<string>(maxLength: 160, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_platform_tenants", x => x.Id));

        migrationBuilder.CreateTable(
            name: "platform_organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Code = table.Column<string>(maxLength: 64, nullable: false),
                Name = table.Column<string>(maxLength: 160, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_organizations", x => x.Id);
                table.UniqueConstraint("AK_platform_organizations_Id_TenantId", x => new { x.Id, x.TenantId });
                table.ForeignKey("FK_platform_organizations_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "platform_projects",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                OrganizationId = table.Column<Guid>(nullable: false),
                Code = table.Column<string>(maxLength: 64, nullable: false),
                Name = table.Column<string>(maxLength: 160, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_projects", x => x.Id);
                table.UniqueConstraint("AK_platform_projects_Id_OrganizationId_TenantId",
                    x => new { x.Id, x.OrganizationId, x.TenantId });
                table.ForeignKey("FK_platform_projects_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_platform_projects_platform_organizations_OrganizationId_TenantId",
                    x => new { x.OrganizationId, x.TenantId }, "platform_organizations",
                    new[] { "Id", "TenantId" }, onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "platform_users",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                Email = table.Column<string>(maxLength: 320, nullable: false),
                DisplayName = table.Column<string>(maxLength: 120, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_users", x => x.Id);
                table.ForeignKey("FK_platform_users_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "platform_resources",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                tenant_id = table.Column<Guid>(nullable: false),
                organization_id = table.Column<Guid>(nullable: true),
                project_id = table.Column<Guid>(nullable: true),
                ResourceType = table.Column<string>(maxLength: 80, nullable: false),
                Name = table.Column<string>(maxLength: 160, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_resources", x => x.Id);
                table.CheckConstraint("CK_platform_resources_project_requires_organization",
                    "project_id IS NULL OR organization_id IS NOT NULL");
                table.ForeignKey("FK_platform_resources_platform_tenants_tenant_id",
                    x => x.tenant_id, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_platform_resources_platform_organizations_organization_id_tenant_id",
                    x => new { x.organization_id, x.tenant_id }, "platform_organizations",
                    new[] { "Id", "TenantId" }, onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_platform_resources_platform_projects_project_id_organization_id_tenant_id",
                    x => new { x.project_id, x.organization_id, x.tenant_id }, "platform_projects",
                    new[] { "Id", "OrganizationId", "TenantId" }, onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "platform_policies",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                tenant_id = table.Column<Guid>(nullable: false),
                organization_id = table.Column<Guid>(nullable: true),
                project_id = table.Column<Guid>(nullable: true),
                Key = table.Column<string>(maxLength: 120, nullable: false),
                Value = table.Column<string>(maxLength: 4000, nullable: false),
                Version = table.Column<long>(nullable: false),
                IsEnabled = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_policies", x => x.Id);
                table.CheckConstraint("CK_platform_policies_project_requires_organization",
                    "project_id IS NULL OR organization_id IS NOT NULL");
                table.ForeignKey("FK_platform_policies_platform_tenants_tenant_id",
                    x => x.tenant_id, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_platform_policies_platform_organizations_organization_id_tenant_id",
                    x => new { x.organization_id, x.tenant_id }, "platform_organizations",
                    new[] { "Id", "TenantId" }, onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_platform_policies_platform_projects_project_id_organization_id_tenant_id",
                    x => new { x.project_id, x.organization_id, x.tenant_id }, "platform_projects",
                    new[] { "Id", "OrganizationId", "TenantId" }, onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_platform_tenants_Code", "platform_tenants", "Code", unique: true);
        migrationBuilder.CreateIndex("IX_platform_organizations_TenantId_Code", "platform_organizations",
            new[] { "TenantId", "Code" }, unique: true);
        migrationBuilder.CreateIndex("IX_platform_organizations_TenantId", "platform_organizations", "TenantId");
        migrationBuilder.CreateIndex("IX_platform_projects_OrganizationId_Code", "platform_projects",
            new[] { "OrganizationId", "Code" }, unique: true);
        migrationBuilder.CreateIndex("IX_platform_projects_TenantId_OrganizationId", "platform_projects",
            new[] { "TenantId", "OrganizationId" });
        migrationBuilder.CreateIndex("IX_platform_users_TenantId_Email", "platform_users",
            new[] { "TenantId", "Email" }, unique: true);
        migrationBuilder.CreateIndex("IX_platform_resources_tenant_id", "platform_resources", "tenant_id");
        migrationBuilder.CreateIndex("IX_platform_policies_tenant_id", "platform_policies", "tenant_id");
        migrationBuilder.CreateIndex("IX_platform_policies_Key_Version", "platform_policies", new[] { "Key", "Version" });
    }

    /// <summary>초기 스키마를 외래 키 의존성의 역순으로 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "platform_policies");
        migrationBuilder.DropTable(name: "platform_resources");
        migrationBuilder.DropTable(name: "platform_users");
        migrationBuilder.DropTable(name: "platform_projects");
        migrationBuilder.DropTable(name: "platform_organizations");
        migrationBuilder.DropTable(name: "platform_tenants");
    }
}
