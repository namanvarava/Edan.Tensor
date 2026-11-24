using System;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations;

[DbContext(typeof(PlatformDbContext))]
[Migration("20261122163951_AddTenantDatabaseMutationAuditsPg")]
public partial class AddTenantDatabaseMutationAuditsPg : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "platform_tenant_database_mutation_audits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Succeeded = table.Column<bool>(type: "boolean", nullable: true),
                Compensated = table.Column<bool>(type: "boolean", nullable: true),
                ResultJson = table.Column<string>(type: "text", nullable: true),
                Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_platform_tenant_database_mutation_audits", x => x.Id);
                table.ForeignKey("FK_platform_tenant_database_mutation_audits_platform_tenants_TenantId", x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_platform_tenant_database_mutation_audits_TenantId_IdempotencyKey", "platform_tenant_database_mutation_audits", new[] { "TenantId", "IdempotencyKey" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "platform_tenant_database_mutation_audits");
}
