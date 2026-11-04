using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>Platform 초기 테이블과 테넌트 소유권 제약을 생성합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261104100000_InitialPlatformSchema")]
public sealed class InitialPlatformSchema : Migration
{
    /// <summary>테넌트 경계와 복합 외래 키를 포함한 초기 스키마를 생성합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE platform_tenants (
                Id TEXT NOT NULL PRIMARY KEY,
                Code TEXT NOT NULL,
                Name TEXT NOT NULL,
                Status TEXT NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IX_platform_tenants_Code ON platform_tenants (Code);

            CREATE TABLE platform_organizations (
                Id TEXT NOT NULL PRIMARY KEY,
                TenantId TEXT NOT NULL,
                Code TEXT NOT NULL,
                Name TEXT NOT NULL,
                Status TEXT NOT NULL,
                CONSTRAINT AK_platform_organizations_Id_TenantId UNIQUE (Id, TenantId),
                CONSTRAINT FK_platform_organizations_platform_tenants_TenantId
                    FOREIGN KEY (TenantId) REFERENCES platform_tenants (Id) ON DELETE RESTRICT
            );
            CREATE UNIQUE INDEX IX_platform_organizations_TenantId_Code ON platform_organizations (TenantId, Code);
            CREATE INDEX IX_platform_organizations_TenantId ON platform_organizations (TenantId);

            CREATE TABLE platform_projects (
                Id TEXT NOT NULL PRIMARY KEY,
                TenantId TEXT NOT NULL,
                OrganizationId TEXT NOT NULL,
                Code TEXT NOT NULL,
                Name TEXT NOT NULL,
                Status TEXT NOT NULL,
                CONSTRAINT AK_platform_projects_Id_OrganizationId_TenantId UNIQUE (Id, OrganizationId, TenantId),
                CONSTRAINT FK_platform_projects_platform_tenants_TenantId
                    FOREIGN KEY (TenantId) REFERENCES platform_tenants (Id) ON DELETE RESTRICT,
                CONSTRAINT FK_platform_projects_platform_organizations_OrganizationId_TenantId
                    FOREIGN KEY (OrganizationId, TenantId) REFERENCES platform_organizations (Id, TenantId) ON DELETE RESTRICT
            );
            CREATE UNIQUE INDEX IX_platform_projects_OrganizationId_Code ON platform_projects (OrganizationId, Code);
            CREATE INDEX IX_platform_projects_TenantId_OrganizationId ON platform_projects (TenantId, OrganizationId);

            CREATE TABLE platform_users (
                Id TEXT NOT NULL PRIMARY KEY,
                TenantId TEXT NOT NULL,
                Email TEXT NOT NULL,
                DisplayName TEXT NOT NULL,
                Status TEXT NOT NULL,
                CONSTRAINT FK_platform_users_platform_tenants_TenantId
                    FOREIGN KEY (TenantId) REFERENCES platform_tenants (Id) ON DELETE RESTRICT
            );
            CREATE UNIQUE INDEX IX_platform_users_TenantId_Email ON platform_users (TenantId, Email);

            CREATE TABLE platform_resources (
                Id TEXT NOT NULL PRIMARY KEY,
                tenant_id TEXT NOT NULL,
                organization_id TEXT NULL,
                project_id TEXT NULL,
                ResourceType TEXT NOT NULL,
                Name TEXT NOT NULL,
                Status TEXT NOT NULL,
                CONSTRAINT CK_platform_resources_project_requires_organization
                    CHECK (project_id IS NULL OR organization_id IS NOT NULL),
                CONSTRAINT FK_platform_resources_platform_tenants_tenant_id
                    FOREIGN KEY (tenant_id) REFERENCES platform_tenants (Id) ON DELETE RESTRICT,
                CONSTRAINT FK_platform_resources_platform_organizations_organization_id_tenant_id
                    FOREIGN KEY (organization_id, tenant_id) REFERENCES platform_organizations (Id, TenantId) ON DELETE RESTRICT,
                CONSTRAINT FK_platform_resources_platform_projects_project_id_organization_id_tenant_id
                    FOREIGN KEY (project_id, organization_id, tenant_id) REFERENCES platform_projects (Id, OrganizationId, TenantId) ON DELETE RESTRICT
            );
            CREATE INDEX IX_platform_resources_tenant_id ON platform_resources (tenant_id);

            CREATE TABLE platform_policies (
                Id TEXT NOT NULL PRIMARY KEY,
                tenant_id TEXT NOT NULL,
                organization_id TEXT NULL,
                project_id TEXT NULL,
                Key TEXT NOT NULL,
                Value TEXT NOT NULL,
                Version INTEGER NOT NULL,
                IsEnabled INTEGER NOT NULL,
                CONSTRAINT CK_platform_policies_project_requires_organization
                    CHECK (project_id IS NULL OR organization_id IS NOT NULL),
                CONSTRAINT FK_platform_policies_platform_tenants_tenant_id
                    FOREIGN KEY (tenant_id) REFERENCES platform_tenants (Id) ON DELETE RESTRICT,
                CONSTRAINT FK_platform_policies_platform_organizations_organization_id_tenant_id
                    FOREIGN KEY (organization_id, tenant_id) REFERENCES platform_organizations (Id, TenantId) ON DELETE RESTRICT,
                CONSTRAINT FK_platform_policies_platform_projects_project_id_organization_id_tenant_id
                    FOREIGN KEY (project_id, organization_id, tenant_id) REFERENCES platform_projects (Id, OrganizationId, TenantId) ON DELETE RESTRICT
            );
            CREATE INDEX IX_platform_policies_tenant_id ON platform_policies (tenant_id);
            CREATE INDEX IX_platform_policies_Key_Version ON platform_policies (Key, Version);
            """);
    }

    /// <summary>초기 스키마를 역순으로 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS platform_policies;
            DROP TABLE IF EXISTS platform_resources;
            DROP TABLE IF EXISTS platform_users;
            DROP TABLE IF EXISTS platform_projects;
            DROP TABLE IF EXISTS platform_organizations;
            DROP TABLE IF EXISTS platform_tenants;
            """);
    }
}
