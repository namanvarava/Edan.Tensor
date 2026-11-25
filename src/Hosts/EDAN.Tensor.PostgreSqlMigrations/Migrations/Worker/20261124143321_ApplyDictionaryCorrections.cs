using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Worker;

public partial class ApplyDictionaryCorrections : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The preceding naming migration already contains the approved
        // dictionary names. Keep this migration as a compatibility marker.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally empty: dictionary corrections are not reversible.
    }
}
