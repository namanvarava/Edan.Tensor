using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence.Migrations;

/// <summary>Tenant별 Quota 임계치 상태 전이 이력을 추가합니다.</summary>
[DbContext(typeof(DataSetDbContext))]
[Migration("20261106130000_AddDataSetQuotaThresholdEvents")]
public sealed class AddDataSetQuotaThresholdEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "dataset_quota_threshold_events",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                PolicyId = table.Column<Guid>(type: "TEXT", nullable: false),
                Scope = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                TargetId = table.Column<Guid>(type: "TEXT", nullable: false),
                EventType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                UsedBytes = table.Column<long>(type: "INTEGER", nullable: false),
                LimitBytes = table.Column<long>(type: "INTEGER", nullable: false),
                UtilizationPercent = table.Column<double>(type: "REAL", nullable: false),
                PolicyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_dataset_quota_threshold_events", x => x.Id));

        migrationBuilder.CreateIndex("IX_dataset_quota_threshold_events_TenantId_PolicyId_OccurredAtUtc",
            "dataset_quota_threshold_events", new[] { "TenantId", "PolicyId", "OccurredAtUtc" });
        migrationBuilder.CreateIndex("IX_dataset_quota_threshold_events_TenantId_OccurredAtUtc",
            "dataset_quota_threshold_events", new[] { "TenantId", "OccurredAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("dataset_quota_threshold_events");

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        DataSetDbContext.ConfigureModel(modelBuilder);
    }
}
