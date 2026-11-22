using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantDatabaseBindingsPg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_tenant_database_bindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SecretKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ServerAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    DatabaseName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SchemaName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MigrationVersion = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastHealthAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_tenant_database_bindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_platform_tenant_database_bindings_platform_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "platform_tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_tenant_database_bindings_TenantId_Provider",
                table: "platform_tenant_database_bindings",
                columns: new[] { "TenantId", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_tenant_database_bindings_TenantId_Status",
                table: "platform_tenant_database_bindings",
                columns: new[] { "TenantId", "Status" },
                unique: true,
                filter: "\"Status\" = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_tenant_database_bindings");
        }
    }
}
