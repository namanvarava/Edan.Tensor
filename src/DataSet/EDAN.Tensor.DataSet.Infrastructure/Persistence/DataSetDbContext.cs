using EDAN.Tensor.DataSet.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DataSetAggregate = EDAN.Tensor.DataSet.Domain.Model.DataSet;
using EDAN.Tensor.DataSet.Infrastructure.Validation;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence;

/// <summary>DataSet 메타데이터와 스키마 이력을 전용 데이터베이스 경계에 영속화합니다.</summary>
public sealed class DataSetDbContext(DbContextOptions<DataSetDbContext> options) : DbContext(options)
{
    public DbSet<DataSetAggregate> DataSets => Set<DataSetAggregate>();
    public DbSet<DataSetStorageUsageSnapshot> StorageUsageSnapshots => Set<DataSetStorageUsageSnapshot>();
    public DbSet<DataSetQuotaThresholdEvent> QuotaThresholdEvents => Set<DataSetQuotaThresholdEvent>();
    public DbSet<DataSetValidationProfile> ValidationProfiles => Set<DataSetValidationProfile>();
    public DbSet<DataSetValidationProfileRevision> ValidationProfileRevisions => Set<DataSetValidationProfileRevision>();
    public DbSet<DataSetValidationExecution> ValidationExecutions => Set<DataSetValidationExecution>();
    public DbSet<DataSetValidationDetail> ValidationDetails => Set<DataSetValidationDetail>();
    public DbSet<DataSetValidationSeenKey> ValidationSeenKeys => Set<DataSetValidationSeenKey>();

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
        entity.Property(x => x.ContentFormat).HasMaxLength(24).IsRequired();
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

        var profile = modelBuilder.Entity<DataSetValidationProfile>();
        profile.ToTable("dataset_validation_profiles");
        profile.HasKey(x => x.Id);
        profile.Property(x => x.TenantId).IsRequired();
        profile.Property(x => x.Name).HasMaxLength(160).IsRequired();
        profile.Property(x => x.CreatedAtUtc).IsRequired();
        profile.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();

        var profileRevision = modelBuilder.Entity<DataSetValidationProfileRevision>();
        profileRevision.ToTable("dataset_validation_profile_revisions");
        profileRevision.HasKey(x => new { x.ProfileId, x.Revision });
        profileRevision.Property(x => x.ProfileId).IsRequired();
        profileRevision.Property(x => x.TenantId).IsRequired();
        profileRevision.Property(x => x.Revision).IsRequired();
        profileRevision.Property(x => x.DefinitionJson).HasMaxLength(1_048_576).IsRequired();
        profileRevision.Property(x => x.CreatedAtUtc).IsRequired();
        profileRevision.HasIndex(x => new { x.TenantId, x.ProfileId, x.Revision });

        var execution = modelBuilder.Entity<DataSetValidationExecution>();
        execution.ToTable("dataset_validation_executions");
        execution.HasKey(x => x.Id);
        execution.Property(x => x.TenantId).IsRequired();
        execution.Property(x => x.DataSetId).IsRequired();
        execution.Property(x => x.ExpectedDataSetVersion).IsRequired();
        execution.Property(x => x.SchemaVersion).IsRequired();
        execution.Property(x => x.ProfileId).IsRequired();
        execution.Property(x => x.ProfileRevision).IsRequired();
        execution.Property(x => x.Status).HasMaxLength(24).IsRequired();
        execution.Property(x => x.DetailReference).HasMaxLength(128).IsRequired();
        execution.Property(x => x.ProcessedRecords).IsRequired();
        execution.Property(x => x.TotalRecords).IsRequired();
        execution.Property(x => x.ErrorCount).IsRequired();
        execution.Property(x => x.WarningCount).IsRequired();
        execution.Property(x => x.DetailItemCount).IsRequired();
        execution.Property(x => x.DetailTruncated).IsRequired();
        execution.Property(x => x.RuleCountsJson).HasMaxLength(1_048_576).IsRequired();
        execution.Property(x => x.FailureCode).HasMaxLength(200);
        execution.Property(x => x.FailureMessage).HasMaxLength(500);
        execution.Property(x => x.CreatedAtUtc).IsRequired();
        execution.Property(x => x.CompletedAtUtc);
        execution.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        execution.HasIndex(x => new { x.TenantId, x.DataSetId, x.CreatedAtUtc });
        execution.HasIndex(x => new { x.TenantId, x.DetailReference }).IsUnique();

        var detail = modelBuilder.Entity<DataSetValidationDetail>();
        detail.ToTable("dataset_validation_details");
        detail.HasKey(x => x.Id);
        detail.Property(x => x.Id).ValueGeneratedOnAdd().HasAnnotation("Sqlite:Autoincrement", true);
        detail.Property(x => x.ExecutionId).IsRequired();
        detail.Property(x => x.TenantId).IsRequired();
        detail.Property(x => x.DetailReference).HasMaxLength(128).IsRequired();
        detail.Property(x => x.RowNumber).IsRequired();
        detail.Property(x => x.FieldName).HasMaxLength(128);
        detail.Property(x => x.Code).HasMaxLength(200).IsRequired();
        detail.Property(x => x.Severity).HasMaxLength(16).IsRequired();
        detail.Property(x => x.Message).HasMaxLength(500).IsRequired();
        detail.HasIndex(x => new { x.TenantId, x.ExecutionId, x.RowNumber });
        detail.HasIndex(x => new { x.ExecutionId, x.RowNumber, x.Code, x.FieldName }).IsUnique();

        var seenKey = modelBuilder.Entity<DataSetValidationSeenKey>();
        seenKey.ToTable("dataset_validation_seen_keys");
        seenKey.HasKey(x => x.Id);
        seenKey.Property(x => x.Id).ValueGeneratedOnAdd().HasAnnotation("Sqlite:Autoincrement", true);
        seenKey.Property(x => x.ExecutionId).IsRequired();
        seenKey.Property(x => x.RuleCode).HasMaxLength(200).IsRequired();
        seenKey.Property(x => x.KeyHash).HasMaxLength(64).IsRequired();
        seenKey.HasIndex(x => new { x.ExecutionId, x.RuleCode, x.KeyHash }).IsUnique();
    }
}
