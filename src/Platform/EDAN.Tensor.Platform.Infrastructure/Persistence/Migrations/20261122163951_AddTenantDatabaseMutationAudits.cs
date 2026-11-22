using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantDatabaseMutationAudits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_tenant_database_mutation_audits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Succeeded = table.Column<bool>(type: "INTEGER", nullable: true),
                    Compensated = table.Column<bool>(type: "INTEGER", nullable: true),
                    ResultJson = table.Column<string>(type: "TEXT", nullable: true),
                    Error = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_tenant_database_mutation_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_tenant_database_mutation_audits_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_tenant_database_mutation_audits_TenantId_IdempotencyKey",
                table: "platform_tenant_database_mutation_audits",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_tenant_database_mutation_audits");
        }
    }
}
