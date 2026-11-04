using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>외부 인증 주체의 Tenant 소속과 역할 테이블을 추가합니다.</summary>
[DbContext(typeof(PlatformDbContext))]
[Migration("20261105100000_AddTenantMemberships")]
public sealed class AddTenantMemberships : Migration
{
    /// <summary>Tenant별 외부 주체 소속을 고유하게 저장합니다.</summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_tenant_memberships",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                SubjectId = table.Column<string>(maxLength: 200, nullable: false),
                Role = table.Column<string>(maxLength: 32, nullable: false),
                Status = table.Column<string>(maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_tenant_memberships", x => x.Id);
                table.ForeignKey("FK_platform_tenant_memberships_platform_tenants_TenantId",
                    x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_platform_tenant_memberships_TenantId_SubjectId",
            table: "platform_tenant_memberships",
            columns: new[] { "TenantId", "SubjectId" },
            unique: true);
    }

    /// <summary>Tenant 소속 테이블을 제거합니다.</summary>
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "platform_tenant_memberships");
}
