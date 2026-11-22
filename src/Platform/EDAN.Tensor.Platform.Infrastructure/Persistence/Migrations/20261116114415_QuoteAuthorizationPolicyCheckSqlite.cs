using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class QuoteAuthorizationPolicyCheckSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_platform_authorization_policies_project_requires_organization",
                table: "platform_authorization_policies");

            migrationBuilder.AddCheckConstraint(
                name: "CK_platform_authorization_policies_project_requires_organization",
                table: "platform_authorization_policies",
                sql: "\"ProjectId\" IS NULL OR \"OrganizationId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_platform_authorization_policies_project_requires_organization",
                table: "platform_authorization_policies");

            migrationBuilder.AddCheckConstraint(
                name: "CK_platform_authorization_policies_project_requires_organization",
                table: "platform_authorization_policies",
                sql: "ProjectId IS NULL OR OrganizationId IS NOT NULL");
        }
    }
}
