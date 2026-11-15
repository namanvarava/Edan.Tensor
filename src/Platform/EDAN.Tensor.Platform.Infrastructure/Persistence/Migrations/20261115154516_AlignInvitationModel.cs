using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignInvitationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AcceptedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ResendCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_invitations", x => x.Id);
                    table.UniqueConstraint("AK_platform_invitations_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey("FK_platform_invitations_platform_tenants_TenantId", x => x.TenantId,
                        "platform_tenants", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_platform_invitations_platform_users_UserId", x => x.UserId,
                        "platform_users", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_platform_invitations_TokenHash", "platform_invitations", "TokenHash", unique: true);
            migrationBuilder.CreateIndex("IX_platform_invitations_TenantId_Status_CreatedAtUtc", "platform_invitations",
                new[] { "TenantId", "Status", "CreatedAtUtc" });
            migrationBuilder.CreateIndex("IX_platform_invitations_TenantId_UserId", "platform_invitations", new[] { "TenantId", "UserId" });
            migrationBuilder.CreateIndex("IX_platform_invitations_UserId", "platform_invitations", "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("platform_invitations");
        }
    }
}
