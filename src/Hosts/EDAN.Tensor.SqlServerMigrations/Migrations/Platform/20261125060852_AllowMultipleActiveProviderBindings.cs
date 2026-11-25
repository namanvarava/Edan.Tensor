using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AllowMultipleActiveProviderBindings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'edpf_tntdbnd_N02' AND object_id = OBJECT_ID(N'edpf_tntdbnd')) DROP INDEX [edpf_tntdbnd_N02] ON [edpf_tntdbnd]; IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_platform_tenant_database_bindings_TenantId_Status' AND object_id = OBJECT_ID(N'edpf_tntdbnd')) DROP INDEX [IX_platform_tenant_database_bindings_TenantId_Status] ON [edpf_tntdbnd]; CREATE UNIQUE INDEX [edpf_tntdbnd_N02] ON [edpf_tntdbnd] ([tntid], [prvdr], [st]) WHERE [st] = 'Active';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'edpf_tntdbnd_N02' AND object_id = OBJECT_ID(N'edpf_tntdbnd')) DROP INDEX [edpf_tntdbnd_N02] ON [edpf_tntdbnd]; IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_platform_tenant_database_bindings_TenantId_Status' AND object_id = OBJECT_ID(N'edpf_tntdbnd')) DROP INDEX [IX_platform_tenant_database_bindings_TenantId_Status] ON [edpf_tntdbnd]; CREATE UNIQUE INDEX [edpf_tntdbnd_N02] ON [edpf_tntdbnd] ([tntid], [st]) WHERE [st] = 'Active';");
        }
    }
}
