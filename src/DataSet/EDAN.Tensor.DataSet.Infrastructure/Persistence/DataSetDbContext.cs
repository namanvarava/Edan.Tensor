using EDAN.Tensor.DataSet.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DataSetAggregate = EDAN.Tensor.DataSet.Domain.Model.DataSet;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence;

/// <summary>DataSet 메타데이터와 스키마 이력을 전용 데이터베이스 경계에 영속화합니다.</summary>
public sealed class DataSetDbContext(DbContextOptions<DataSetDbContext> options) : DbContext(options)
{
    public DbSet<DataSetAggregate> DataSets => Set<DataSetAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureModel(modelBuilder);
    }

    /// <summary>실행 컨텍스트와 모델 검증이 공유하는 DataSet 저장 모델을 구성합니다.</summary>
    internal static void ConfigureModel(ModelBuilder modelBuilder)
    {
        var idConverter = new ValueConverter<DataSetId, Guid>(id => id.Value, value => new DataSetId(value));
        var entity = modelBuilder.Entity<DataSetAggregate>();
        entity.ToTable("datasets");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasConversion(idConverter).ValueGeneratedNever();
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        entity.Property(x => x.StorageAdapter).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.Version).IsConcurrencyToken().IsRequired();

        entity.OwnsOne(x => x.Scope, scope =>
        {
            scope.Property(x => x.TenantId).HasColumnName("TenantId").IsRequired();
            scope.Property(x => x.OrganizationId).HasColumnName("OrganizationId").IsRequired();
            scope.Property(x => x.ProjectId).HasColumnName("ProjectId").IsRequired();
            scope.HasIndex(x => new { x.TenantId, x.OrganizationId, x.ProjectId });
        });
        entity.Navigation(x => x.Scope).IsRequired();

        entity.OwnsOne(x => x.StorageReference, storage =>
        {
            storage.Property(x => x.Value).HasColumnName("StorageReference").HasMaxLength(256).IsRequired();
        });
        entity.Navigation(x => x.StorageReference).IsRequired();

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
    }
}
