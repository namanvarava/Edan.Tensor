using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.OracleMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AllowMultipleActiveProviderBindings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"BEGIN
    BEGIN EXECUTE IMMEDIATE 'DROP INDEX ""edpf_tntdbnd_N02""'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1418 THEN RAISE; END IF; END;
    BEGIN EXECUTE IMMEDIATE 'DROP INDEX ""IX_platform_tenant_database_bindings_TenantId_Status""'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1418 THEN RAISE; END IF; END;
END;");
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX ""edpf_tntdbnd_N02"" ON ""edpf_tntdbnd""
    (""tntid"", ""prvdr"", CASE WHEN ""st"" = 'Active' THEN ""st"" ELSE NULL END)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"BEGIN
    BEGIN EXECUTE IMMEDIATE 'DROP INDEX ""edpf_tntdbnd_N02""'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1418 THEN RAISE; END IF; END;
    BEGIN EXECUTE IMMEDIATE 'DROP INDEX ""IX_platform_tenant_database_bindings_TenantId_Status""'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1418 THEN RAISE; END IF; END;
END;");
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX ""edpf_tntdbnd_N02"" ON ""edpf_tntdbnd""
    (""tntid"", CASE WHEN ""st"" = 'Active' THEN ""st"" ELSE NULL END)");
        }
    }
}
