using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>SQLite는 제약조건명을 메타데이터로 재작성할 수 없으므로 후속 모델 Migration에서 재구성합니다.</summary>
public partial class ApplyApprovedNamingConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) { }
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
