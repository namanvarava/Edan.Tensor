using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence.Migrations;

/// <summary>DataSet 메타데이터 모델의 EF Core 스냅샷입니다.</summary>
[DbContext(typeof(DataSetDbContext))]
public sealed class DataSetDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        DataSetDbContext.ConfigureModel(modelBuilder);
    }
}