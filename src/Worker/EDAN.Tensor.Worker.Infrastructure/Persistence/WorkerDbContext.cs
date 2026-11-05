using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>Worker 작업 상태와 실행 소유권을 전용 저장소에 영속화합니다.</summary>
public sealed class WorkerDbContext(DbContextOptions<WorkerDbContext> options) : DbContext(options)
{
    public DbSet<WorkerTask> WorkerTasks => Set<WorkerTask>();

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
        entity.Property(x => x.TenantId).IsRequired();
        entity.Property(x => x.TaskType).HasMaxLength(120).IsRequired();
        entity.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        entity.Property(x => x.PayloadReference).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(x => x.WorkerId).HasMaxLength(200);
        entity.Property(x => x.Checkpoint).HasMaxLength(4000);
        entity.Property(x => x.LastErrorCode).HasMaxLength(200);
        entity.Property(x => x.Version).IsConcurrencyToken();
        entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
        entity.HasIndex(x => new { x.TenantId, x.Status, x.NextAttemptAtUtc });
        entity.HasIndex(x => new { x.TenantId, x.LeaseId });
    }
}
