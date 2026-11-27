using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.Worker
{
    /// <inheritdoc />
    public partial class _AuditSequentialMappingV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The preceding worker table migration may already have applied the
            // sequential names. Rewind only those columns so the generated
            // rename operations below are safe to replay.
            migrationBuilder.Sql(@"
DECLARE @table sysname, @old sysname, @new sysname, @sql nvarchar(max);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
SELECT table_name, old_name, new_name
FROM (VALUES
 (N'edw_03006',N'wkr_rplid',N'03006id'),
 (N'edw_03005',N'wkr_tskid',N'03005id'),
 (N'edw_03004',N'wkr_dlvid',N'03004id')) AS m(table_name,old_name,new_name);
OPEN c;
FETCH NEXT FROM c INTO @table,@old,@new;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(N'dbo.' + @table) IS NOT NULL
       AND COL_LENGTH(N'dbo.' + @table, @old) IS NULL
       AND COL_LENGTH(N'dbo.' + @table, @new) IS NOT NULL
    BEGIN
        SET @sql = N'EXEC sp_rename N''dbo.' + @table + N'.' + @new + N''', N''' + @old + N''', N''COLUMN'';';
        EXEC sp_executesql @sql;
    END;
    FETCH NEXT FROM c INTO @table,@old,@new;
END;
CLOSE c; DEALLOCATE c;");

            migrationBuilder.RenameColumn(
                name: "wkr_rplid",
                table: "edw_03006",
                newName: "03006id");

            migrationBuilder.RenameColumn(
                name: "wkr_tskid",
                table: "edw_03005",
                newName: "03005id");

            migrationBuilder.RenameColumn(
                name: "wkr_dlvid",
                table: "edw_03004",
                newName: "03004id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "03006id",
                table: "edw_03006",
                newName: "wkr_rplid");

            migrationBuilder.RenameColumn(
                name: "03005id",
                table: "edw_03005",
                newName: "wkr_tskid");

            migrationBuilder.RenameColumn(
                name: "03004id",
                table: "edw_03004",
                newName: "wkr_dlvid");
        }
    }
}
