using System;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations;

[DbContext(typeof(PlatformDbContext))]
[Migration("20261122161609_AddTenantDatabaseRoutineAllowlistSqlServer")]
public partial class AddTenantDatabaseRoutineAllowlistSqlServer : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("platform_tenant_database_routine_allowlists", table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            SchemaName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            RoutineName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            IsEnabled = table.Column<bool>(type: "bit", nullable: false),
            CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_platform_tenant_database_routine_allowlists", x => x.Id);
            table.ForeignKey("FK_platform_tenant_database_routine_allowlists_platform_tenants_TenantId", x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex("IX_platform_tenant_database_routine_allowlists_TenantId_SchemaName_RoutineName_Kind", "platform_tenant_database_routine_allowlists", new[] { "TenantId", "SchemaName", "RoutineName", "Kind" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("platform_tenant_database_routine_allowlists");
}
