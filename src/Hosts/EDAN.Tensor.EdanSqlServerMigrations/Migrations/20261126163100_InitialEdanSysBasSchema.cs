using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.EdanSqlServerMigrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialEdanSysBasSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ed_00001",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Route = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_00001", x => x.Id);
                },
                comment: "시스템 메뉴");

            migrationBuilder.CreateTable(
                name: "ed_00003",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_00003", x => x.Id);
                },
                comment: "고객사 시스템 역할");

            migrationBuilder.CreateTable(
                name: "ed_00005",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_00005", x => x.Id);
                },
                comment: "공통 코드 및 코드 일람표");

            migrationBuilder.CreateTable(
                name: "ed_01001",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_01001", x => x.Id);
                },
                comment: "고객사");

            migrationBuilder.CreateTable(
                name: "ed_00002",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_00002", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ed_00002_00001_menu_id",
                        column: x => x.MenuId,
                        principalTable: "ed_00001",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "메뉴 버튼 및 권한 대상");

            migrationBuilder.CreateTable(
                name: "ed_01002",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_01002", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ed_01002_01001_company_id",
                        column: x => x.CompanyId,
                        principalTable: "ed_01001",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "공장");

            migrationBuilder.CreateTable(
                name: "ed_00004",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ButtonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_00004", x => new { x.RoleId, x.ButtonId });
                    table.ForeignKey(
                        name: "FK_ed_00004_00002_button_id",
                        column: x => x.ButtonId,
                        principalTable: "ed_00002",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ed_00004_00003_role_id",
                        column: x => x.RoleId,
                        principalTable: "ed_00003",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "역할별 메뉴 버튼 권한");

            migrationBuilder.CreateTable(
                name: "ed_01003",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_01003", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ed_01003_01002_factory_id",
                        column: x => x.FactoryId,
                        principalTable: "ed_01002",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "공정");

            migrationBuilder.CreateTable(
                name: "ed_01004",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_01004", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ed_01004_01003_process_id",
                        column: x => x.ProcessId,
                        principalTable: "ed_01003",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "생산 라인");

            migrationBuilder.CreateTable(
                name: "ed_01005",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ed_01005", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ed_01005_01004_line_id",
                        column: x => x.LineId,
                        principalTable: "ed_01004",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "설비");

            migrationBuilder.CreateIndex(
                name: "IX_ed_00001_parent_id",
                table: "ed_00001",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "UX_ed_00001_code",
                table: "ed_00001",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ed_00002_menu_code",
                table: "ed_00002",
                columns: new[] { "MenuId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ed_00003_code",
                table: "ed_00003",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ed_00004_ButtonId",
                table: "ed_00004",
                column: "ButtonId");

            migrationBuilder.CreateIndex(
                name: "UX_ed_00005_group_code",
                table: "ed_00005",
                columns: new[] { "GroupCode", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ed_01001_Code",
                table: "ed_01001",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ed_01002_Code",
                table: "ed_01002",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ed_01002_CompanyId",
                table: "ed_01002",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ed_01003_Code",
                table: "ed_01003",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ed_01003_FactoryId",
                table: "ed_01003",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ed_01004_Code",
                table: "ed_01004",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ed_01004_ProcessId",
                table: "ed_01004",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ed_01005_Code",
                table: "ed_01005",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ed_01005_LineId",
                table: "ed_01005",
                column: "LineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ed_00004");

            migrationBuilder.DropTable(
                name: "ed_00005");

            migrationBuilder.DropTable(
                name: "ed_01005");

            migrationBuilder.DropTable(
                name: "ed_00002");

            migrationBuilder.DropTable(
                name: "ed_00003");

            migrationBuilder.DropTable(
                name: "ed_01004");

            migrationBuilder.DropTable(
                name: "ed_00001");

            migrationBuilder.DropTable(
                name: "ed_01003");

            migrationBuilder.DropTable(
                name: "ed_01002");

            migrationBuilder.DropTable(
                name: "ed_01001");
        }
    }
}
