using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence.Migrations;

/// <summary>Tenant별 시간 단위 저장량 스냅샷과 멱등 키를 추가합니다.</summary>
[DbContext(typeof(DataSetDbContext))]
[Migration("20261106110000_AddDataSetStorageUsageSnapshots")]
public sealed class AddDataSetStorageUsageSnapshots : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "dataset_storage_usage_snapshots",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                PeriodStartUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                CapturedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UsedBytes = table.Column<long>(type: "INTEGER", nullable: false),
                DataSetCount = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_dataset_storage_usage_snapshots", x => x.Id));

        migrationBuilder.CreateIndex("IX_dataset_storage_usage_snapshots_TenantId_PeriodStartUtc",
            "dataset_storage_usage_snapshots", new[] { "TenantId", "PeriodStartUtc" }, unique: true);
        migrationBuilder.CreateIndex("IX_dataset_storage_usage_snapshots_TenantId_CapturedAtUtc",
            "dataset_storage_usage_snapshots", new[] { "TenantId", "CapturedAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("dataset_storage_usage_snapshots");

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        DataSetDbContext.ConfigureModel(modelBuilder);
    }
}
