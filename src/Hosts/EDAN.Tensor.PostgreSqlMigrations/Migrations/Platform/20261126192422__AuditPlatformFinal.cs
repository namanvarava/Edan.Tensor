using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Platform;

public partial class _AuditPlatformFinal : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The preceding reconciliation migration already applied the physical changes.
        // This migration records the final model snapshot without replaying them.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
