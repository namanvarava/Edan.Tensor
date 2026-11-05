using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>Worker 전용 저장 모델의 마이그레이션 기준입니다.</summary>
[DbContext(typeof(WorkerDbContext))]
public sealed class WorkerDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        modelBuilder.Entity<WorkerTask>(entity =>
        {
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
        });
    }
}
