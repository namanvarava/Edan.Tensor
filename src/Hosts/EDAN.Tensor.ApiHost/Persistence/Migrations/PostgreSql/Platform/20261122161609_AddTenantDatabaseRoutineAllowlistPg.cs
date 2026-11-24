using System;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations;

[DbContext(typeof(PlatformDbContext))]
[Migration("20261122161609_AddTenantDatabaseRoutineAllowlistPg")]
public partial class AddTenantDatabaseRoutineAllowlistPg : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_tenant_database_routine_allowlists",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                SchemaName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                RoutineName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_tenant_database_routine_allowlists", x => x.Id);
                table.ForeignKey("FK_platform_tenant_database_routine_allowlists_platform_tenants_TenantId", x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_platform_tenant_database_routine_allowlists_TenantId_SchemaName_RoutineName_Kind", "platform_tenant_database_routine_allowlists", new[] { "TenantId", "SchemaName", "RoutineName", "Kind" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "platform_tenant_database_routine_allowlists");
}
