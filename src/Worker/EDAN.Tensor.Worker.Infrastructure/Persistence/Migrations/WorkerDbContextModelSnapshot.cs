using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence.Migrations;

/// <summary>Worker 전용 저장 모델의 마이그레이션 기준입니다.</summary>
[DbContext(typeof(WorkerDbContext))]
public sealed class WorkerDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        WorkerDbContext.ConfigureModel(modelBuilder);
    }
}
