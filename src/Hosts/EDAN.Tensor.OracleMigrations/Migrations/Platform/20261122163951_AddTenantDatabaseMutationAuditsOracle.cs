using System;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.OracleMigrations.Migrations;

[DbContext(typeof(PlatformDbContext))]
[Migration("20261122163951_AddTenantDatabaseMutationAuditsOracle")]
public partial class AddTenantDatabaseMutationAuditsOracle : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("platform_tenant_database_mutation_audits", table => new
        {
            Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
            TenantId = table.Column<Guid>(type: "RAW(16)", nullable: false),
            IdempotencyKey = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
            Status = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
            Succeeded = table.Column<bool>(type: "BOOLEAN", nullable: true),
            Compensated = table.Column<bool>(type: "BOOLEAN", nullable: true),
            ResultJson = table.Column<string>(type: "NCLOB", nullable: true),
            Error = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
            CreatedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
            CompletedAtUtc = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_platform_tenant_database_mutation_audits", x => x.Id);
            table.ForeignKey("FK_platform_tenant_database_mutation_audits_platform_tenants_TenantId", x => x.TenantId, "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex("IX_platform_tenant_database_mutation_audits_TenantId_IdempotencyKey", "platform_tenant_database_mutation_audits", new[] { "TenantId", "IdempotencyKey" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("platform_tenant_database_mutation_audits");
}
