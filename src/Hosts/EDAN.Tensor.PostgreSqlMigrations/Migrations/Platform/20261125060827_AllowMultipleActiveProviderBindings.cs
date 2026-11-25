using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AllowMultipleActiveProviderBindings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"edpf_tntdbnd_N02\"; DROP INDEX IF EXISTS \"IX_platform_tenant_database_bindings_TenantId_Status\"; CREATE UNIQUE INDEX \"edpf_tntdbnd_N02\" ON public.edpf_tntdbnd (tntid, prvdr, st) WHERE st = 'Active';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"edpf_tntdbnd_N02\"; DROP INDEX IF EXISTS \"IX_platform_tenant_database_bindings_TenantId_Status\"; CREATE UNIQUE INDEX \"edpf_tntdbnd_N02\" ON public.edpf_tntdbnd (tntid, st) WHERE st = 'Active';");
        }
    }
}
