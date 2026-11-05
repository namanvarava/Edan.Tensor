using EDAN.Tensor.DataSet.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DataSetAggregate = EDAN.Tensor.DataSet.Domain.Model.DataSet;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence;

/// <summary>DataSet 메타데이터와 스키마 이력을 전용 데이터베이스 경계에 영속화합니다.</summary>
public sealed class DataSetDbContext(DbContextOptions<DataSetDbContext> options) : DbContext(options)
{
    public DbSet<DataSetAggregate> DataSets => Set<DataSetAggregate>();
    public DbSet<DataSetStorageUsageSnapshot> StorageUsageSnapshots => Set<DataSetStorageUsageSnapshot>();
    public DbSet<DataSetQuotaThresholdEvent> QuotaThresholdEvents => Set<DataSetQuotaThresholdEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureModel(modelBuilder);
    }

    /// <summary>실행 컨텍스트와 마이그레이션 모델이 공유하는 DataSet 저장 모델을 구성합니다.</summary>
    internal static void ConfigureModel(ModelBuilder modelBuilder)
    {
        var idConverter = new ValueConverter<DataSetId, Guid>(id => id.Value, value => new DataSetId(value));
        var storageReferenceConverter = new ValueConverter<StorageReference, string>(reference => reference.Value, value => new StorageReference(value));
        var entity = modelBuilder.Entity<DataSetAggregate>();
        entity.ToTable("datasets");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasConversion(idConverter).ValueGeneratedNever();
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        entity.Property(x => x.StorageAdapter).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        entity.Property(x => x.TenantId).HasColumnName("TenantId").IsRequired();
        entity.Property(x => x.OrganizationId).HasColumnName("OrganizationId").IsRequired();
        entity.Property(x => x.ProjectId).HasColumnName("ProjectId").IsRequired();
        entity.Property(x => x.StorageReference).HasConversion(storageReferenceConverter)
            .HasColumnName("StorageReference").HasMaxLength(256).IsRequired();
        entity.HasIndex(x => new { x.TenantId, x.OrganizationId, x.ProjectId });
        entity.OwnsMany(x => x.SchemaRevisions, revisions =>
        {
            revisions.ToTable("dataset_schema_revisions");
            revisions.WithOwner().HasForeignKey("DataSetId");
            revisions.Property<DataSetId>("DataSetId").HasConversion(idConverter);
            revisions.Property(x => x.Version).IsRequired();
            revisions.Property(x => x.SchemaJson).HasMaxLength(1_048_576).IsRequired();
            revisions.Property(x => x.CreatedAtUtc).IsRequired();
            revisions.HasKey("DataSetId", nameof(DataSetSchemaRevision.Version));
        });
        entity.Navigation(x => x.SchemaRevisions).UsePropertyAccessMode(PropertyAccessMode.Field);
        entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });

        var usageSnapshot = modelBuilder.Entity<DataSetStorageUsageSnapshot>();
        usageSnapshot.ToTable("dataset_storage_usage_snapshots");
        usageSnapshot.HasKey(x => x.Id);
        usageSnapshot.Property(x => x.Id).ValueGeneratedOnAdd().HasAnnotation("Sqlite:Autoincrement", true);
        usageSnapshot.Property(x => x.TenantId).IsRequired();
        usageSnapshot.Property(x => x.PeriodStartUtc).IsRequired();
        usageSnapshot.Property(x => x.CapturedAtUtc).IsRequired();
        usageSnapshot.Property(x => x.UsedBytes).IsRequired();
        usageSnapshot.Property(x => x.DataSetCount).IsRequired();
        usageSnapshot.HasIndex(x => new { x.TenantId, x.PeriodStartUtc }).IsUnique();
        usageSnapshot.HasIndex(x => new { x.TenantId, x.CapturedAtUtc });

        var quotaEvent = modelBuilder.Entity<DataSetQuotaThresholdEvent>();
        quotaEvent.ToTable("dataset_quota_threshold_events");
        quotaEvent.HasKey(x => x.Id);
        quotaEvent.Property(x => x.Id).ValueGeneratedOnAdd().HasAnnotation("Sqlite:Autoincrement", true);
        quotaEvent.Property(x => x.TenantId).IsRequired();
        quotaEvent.Property(x => x.PolicyId).IsRequired();
        quotaEvent.Property(x => x.Scope).HasMaxLength(24).IsRequired();
        quotaEvent.Property(x => x.TargetId).IsRequired();
        quotaEvent.Property(x => x.EventType).HasMaxLength(16).IsRequired();
        quotaEvent.Property(x => x.UsedBytes).IsRequired();
        quotaEvent.Property(x => x.LimitBytes).IsRequired();
        quotaEvent.Property(x => x.UtilizationPercent).IsRequired();
        quotaEvent.Property(x => x.PolicyVersion).IsRequired();
        quotaEvent.Property(x => x.OccurredAtUtc).IsRequired();
        quotaEvent.HasIndex(x => new { x.TenantId, x.PolicyId, x.OccurredAtUtc });
        quotaEvent.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
    }
}
