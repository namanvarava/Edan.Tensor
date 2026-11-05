using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>Worker 작업 상태와 실행 소유권을 전용 저장소에 영속화합니다.</summary>
public sealed class WorkerDbContext(DbContextOptions<WorkerDbContext> options) : DbContext(options)
{
    public DbSet<WorkerTask> WorkerTasks => Set<WorkerTask>();
    public DbSet<WorkerTaskReplayRecord> WorkerTaskReplayRecords => Set<WorkerTaskReplayRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureModel(modelBuilder);
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
        entity.Property(x => x.ResultReference).HasMaxLength(500);\n        entity.Property(x => x.ProgressPercent).IsRequired();\n        entity.Property(x => x.ProgressStage).HasMaxLength(120);\n        entity.Property(x => x.ProgressMessage).HasMaxLength(500);\n        entity.Property(x => x.ResultReference).HasMaxLength(500);
        entity.Property(x => x.LastErrorCode).HasMaxLength(200);
        entity.Property(x => x.Version).IsConcurrencyToken();
        entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
        entity.HasIndex(x => new { x.TenantId, x.Status, x.NextAttemptAtUtc });
        entity.HasIndex(x => new { x.TenantId, x.LeaseId });

        var replay = modelBuilder.Entity<WorkerTaskReplayRecord>();
        replay.ToTable("worker_task_replays");
        replay.HasKey(x => x.Id);
        replay.Property(x => x.TenantId).IsRequired();
        replay.Property(x => x.OriginalTaskId).IsRequired();
        replay.Property(x => x.ReplayTaskId).IsRequired();
        replay.Property(x => x.RequestedBy).HasMaxLength(200).IsRequired();
        replay.Property(x => x.CreatedAtUtc).IsRequired();
        replay.HasIndex(x => new { x.TenantId, x.OriginalTaskId, x.CreatedAtUtc });
        replay.HasIndex(x => new { x.TenantId, x.ReplayTaskId }).IsUnique();
    }
}
