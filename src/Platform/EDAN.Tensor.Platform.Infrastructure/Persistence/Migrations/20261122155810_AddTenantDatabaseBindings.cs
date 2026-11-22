using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantDatabaseBindings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_tenant_database_bindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    SecretKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ServerAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Port = table.Column<int>(type: "INTEGER", nullable: true),
                    DatabaseName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SchemaName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    MigrationVersion = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    LastHealthAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_tenant_database_bindings", x => x.Id);
                    table.ForeignKey("FK_platform_tenant_database_bindings_platform_tenants_TenantId", x => x.TenantId,
                        "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_platform_tenant_database_bindings_TenantId_Provider",
                "platform_tenant_database_bindings", new[] { "TenantId", "Provider" });
            migrationBuilder.CreateIndex("IX_platform_tenant_database_bindings_TenantId_Status",
                "platform_tenant_database_bindings", new[] { "TenantId", "Status" }, unique: true,
                filter: "Status = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "platform_tenant_database_bindings");
        }
    }
}
