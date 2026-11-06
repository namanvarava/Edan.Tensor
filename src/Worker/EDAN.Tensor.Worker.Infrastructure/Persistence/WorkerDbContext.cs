using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>Worker 작업 상태와 실행 소유권을 전용 저장소에 영속화합니다.</summary>
public sealed class WorkerDbContext(DbContextOptions<WorkerDbContext> options) : DbContext(options)
{
    public DbSet<WorkerTask> WorkerTasks => Set<WorkerTask>();
    public DbSet<WorkerTaskReplayRecord> WorkerTaskReplayRecords => Set<WorkerTaskReplayRecord>();
    public DbSet<WorkerTaskTypeCatalogEntry> WorkerTaskTypes => Set<WorkerTaskTypeCatalogEntry>();
    public DbSet<ExternalDelivery> ExternalDeliveries => Set<ExternalDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureModel(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareWorkerTaskIndexValues();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PrepareWorkerTaskIndexValues();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void PrepareWorkerTaskIndexValues()
    {
        foreach (var entry in ChangeTracker.Entries<WorkerTask>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var readyAt = entry.Entity.NextAttemptAtUtc ?? entry.Entity.CreatedAtUtc;
                entry.Property<long>("ReadyAtUtcUnixMilliseconds").CurrentValue = readyAt.ToUnixTimeMilliseconds();
                entry.Property<long>("CreatedAtUtcUnixMilliseconds").CurrentValue = entry.Entity.CreatedAtUtc.ToUnixTimeMilliseconds();
                entry.Property<long?>("LeaseExpiresAtUtcUnixMilliseconds").CurrentValue =
                    entry.Entity.LeaseExpiresAtUtc?.ToUnixTimeMilliseconds();
            }
        }
    }

    /// <summary>실제 컨텍스트와 마이그레이션 스냅샷이 공유하는 저장 모델을 구성합니다.</summary>
    internal static void ConfigureModel(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<WorkerTask>();
        entity.ToTable("worker_tasks");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedOnAdd();
        entity.Property(x => x.TenantId).IsRequired();
        entity.Property(x => x.TaskType).HasMaxLength(120).IsRequired();
        entity.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        entity.Property(x => x.PayloadReference).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(x => x.AttemptCount).IsRequired();
        entity.Property(x => x.MaxAttempts).IsRequired();
        entity.Property(x => x.LeaseId);
        entity.Property(x => x.FencingToken).IsRequired();
        entity.Property(x => x.LeaseExpiresAtUtc);
        entity.Property(x => x.CheckpointVersion).IsRequired();
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.CompletedAtUtc);
        entity.Property(x => x.NextAttemptAtUtc);
        entity.Property(x => x.WorkerId).HasMaxLength(200);
        entity.Property(x => x.Checkpoint).HasMaxLength(4000);
        entity.Property(x => x.ProgressPercent).IsRequired();
        entity.Property(x => x.ProgressStage).HasMaxLength(120);
        entity.Property(x => x.ProgressMessage).HasMaxLength(500);
        entity.Property(x => x.ResultReference).HasMaxLength(500);
        entity.Property(x => x.LastErrorCode).HasMaxLength(200);
        entity.Property(x => x.Version).IsConcurrencyToken();
        entity.Property<long>("ReadyAtUtcUnixMilliseconds").HasColumnType("INTEGER");
        entity.Property<long>("CreatedAtUtcUnixMilliseconds").HasColumnType("INTEGER");
        entity.Property<long?>("LeaseExpiresAtUtcUnixMilliseconds").HasColumnType("INTEGER");
        entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
        entity.HasIndex("TenantId", "Status", "ReadyAtUtcUnixMilliseconds");
        entity.HasIndex("Status", "ReadyAtUtcUnixMilliseconds", "TenantId");
        entity.HasIndex("TenantId", "Status", "CreatedAtUtcUnixMilliseconds", "Id");
        entity.HasIndex("Status", "LeaseExpiresAtUtcUnixMilliseconds");
        entity.HasIndex(x => new { x.TenantId, x.LeaseId });

        var replay = modelBuilder.Entity<WorkerTaskReplayRecord>();
        replay.ToTable("worker_task_replays");
        replay.HasKey(x => x.Id);
        replay.Property(x => x.TenantId).IsRequired();
        replay.Property(x => x.OriginalTaskId).IsRequired();
        replay.Property(x => x.ReplayTaskId).IsRequired();
        replay.Property(x => x.ReplayRequestId).IsRequired();
        replay.Property(x => x.RequestedBy).HasMaxLength(200).IsRequired();
        replay.Property(x => x.CreatedAtUtc).IsRequired();
        replay.HasIndex(x => new { x.TenantId, x.OriginalTaskId, x.CreatedAtUtc });
        replay.HasIndex(x => new { x.TenantId, x.ReplayTaskId }).IsUnique();
        replay.HasIndex(x => new { x.TenantId, x.OriginalTaskId, x.ReplayRequestId })
            .IsUnique()
            .HasFilter("\"ReplayRequestId\" <> '00000000-0000-0000-0000-000000000000'");

        var taskType = modelBuilder.Entity<WorkerTaskTypeCatalogEntry>();
        taskType.ToTable("worker_task_types");
        taskType.HasKey(x => x.TaskType);
        taskType.Property(x => x.TaskType).HasMaxLength(120).IsRequired();
        taskType.Property(x => x.Description).HasMaxLength(500);
        taskType.Property(x => x.IsEnabled).IsRequired();
        taskType.Property(x => x.UpdatedAtUtc).IsRequired();
        taskType.Property(x => x.Version).IsConcurrencyToken();

        var delivery = modelBuilder.Entity<ExternalDelivery>();
        delivery.ToTable("external_deliveries");
        delivery.HasKey(x => x.Id);
        delivery.Property(x => x.Id).ValueGeneratedNever();
        delivery.Property(x => x.TenantId).IsRequired();
        delivery.Property(x => x.TargetUri).HasMaxLength(2_048).IsRequired();
        delivery.Property(x => x.Payload).HasMaxLength(1_048_576).IsRequired();
        delivery.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        delivery.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        delivery.Property(x => x.AttemptCount).IsRequired();
        delivery.Property(x => x.MaxAttempts).IsRequired();
        delivery.Property(x => x.LastErrorCode).HasMaxLength(200);
        delivery.Property(x => x.CreatedAtUtc).IsRequired();
        delivery.Property(x => x.CompletedAtUtc);
        delivery.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        delivery.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
        delivery.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAtUtc });
    }
}
