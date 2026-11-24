using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplyApprovedNamingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_edds_dsetschm_rev_edds_dset_DataSetId",
                table: "edds_dsetschm_rev");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_seenkey",
                table: "edds_dsetvld_seenkey");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_prfrev",
                table: "edds_dsetvld_prfrev");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_prf",
                table: "edds_dsetvld_prf");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_exc",
                table: "edds_dsetvld_exc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetvld_dtl",
                table: "edds_dsetvld_dtl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetstr_usgsnp",
                table: "edds_dsetstr_usgsnp");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetschm_rev",
                table: "edds_dsetschm_rev");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dsetqta_threvt",
                table: "edds_dsetqta_threvt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edds_dset",
                table: "edds_dset");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_seenkey_ExecutionId_RuleCode_KeyHash",
                table: "edds_dsetvld_seenkey",
                newName: "edds_dsetvld_seenkey_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_prfrev_TenantId_ProfileId_Revision",
                table: "edds_dsetvld_prfrev",
                newName: "edds_dsetvld_prfrev_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_prf_TenantId_Name",
                table: "edds_dsetvld_prf",
                newName: "edds_dsetvld_prf_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_exc_TenantId_IdempotencyKey",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_exc_TenantId_DetailReference",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_exc_TenantId_DataSetId_CreatedAtUtc",
                table: "edds_dsetvld_exc",
                newName: "edds_dsetvld_exc_N03");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_dtl_TenantId_ExecutionId_RowNumber",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetvld_dtl_ExecutionId_RowNumber_Code_FieldName",
                table: "edds_dsetvld_dtl",
                newName: "edds_dsetvld_dtl_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetstr_usgsnp_TenantId_PeriodStartUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetstr_usgsnp_TenantId_CapturedAtUtc",
                table: "edds_dsetstr_usgsnp",
                newName: "edds_dsetstr_usgsnp_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetqta_threvt_TenantId_PolicyId_OccurredAtUtc",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dsetqta_threvt_TenantId_OccurredAtUtc",
                table: "edds_dsetqta_threvt",
                newName: "edds_dsetqta_threvt_N01");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dset_TenantId_OrganizationId_ProjectId",
                table: "edds_dset",
                newName: "edds_dset_N02");

            migrationBuilder.RenameIndex(
                name: "IX_edds_dset_Status_CreatedAtUtc",
                table: "edds_dset",
                newName: "edds_dset_N01");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_seenkey_PK",
                table: "edds_dsetvld_seenkey",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_prfrev_PK",
                table: "edds_dsetvld_prfrev",
                columns: new[] { "ProfileId", "Revision" });

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_prf_PK",
                table: "edds_dsetvld_prf",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_exc_PK",
                table: "edds_dsetvld_exc",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetvld_dtl_PK",
                table: "edds_dsetvld_dtl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetstr_usgsnp_PK",
                table: "edds_dsetstr_usgsnp",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetschm_rev_PK",
                table: "edds_dsetschm_rev",
                columns: new[] { "DataSetId", "Version" });

            migrationBuilder.AddPrimaryKey(
                name: "edds_dsetqta_threvt_PK",
                table: "edds_dsetqta_threvt",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "edds_dset_PK",
                table: "edds_dset",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "edds_dsetschm_rev_FKDataSetId",
                table: "edds_dsetschm_rev",
                column: "DataSetId",
                principalTable: "edds_dset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "edds_dsetschm_rev_FKDataSetId",
                table: "edds_dsetschm_rev");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_seenkey_PK",
                table: "edds_dsetvld_seenkey");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_prfrev_PK",
                table: "edds_dsetvld_prfrev");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_prf_PK",
                table: "edds_dsetvld_prf");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_exc_PK",
                table: "edds_dsetvld_exc");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetvld_dtl_PK",
                table: "edds_dsetvld_dtl");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetstr_usgsnp_PK",
                table: "edds_dsetstr_usgsnp");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetschm_rev_PK",
                table: "edds_dsetschm_rev");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dsetqta_threvt_PK",
                table: "edds_dsetqta_threvt");

            migrationBuilder.DropPrimaryKey(
                name: "edds_dset_PK",
                table: "edds_dset");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_seenkey_N01",
                table: "edds_dsetvld_seenkey",
                newName: "IX_edds_dsetvld_seenkey_ExecutionId_RuleCode_KeyHash");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prfrev_N01",
                table: "edds_dsetvld_prfrev",
                newName: "IX_edds_dsetvld_prfrev_TenantId_ProfileId_Revision");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_prf_N01",
                table: "edds_dsetvld_prf",
                newName: "IX_edds_dsetvld_prf_TenantId_Name");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N03",
                table: "edds_dsetvld_exc",
                newName: "IX_edds_dsetvld_exc_TenantId_DataSetId_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N02",
                table: "edds_dsetvld_exc",
                newName: "IX_edds_dsetvld_exc_TenantId_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_exc_N01",
                table: "edds_dsetvld_exc",
                newName: "IX_edds_dsetvld_exc_TenantId_DetailReference");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N02",
                table: "edds_dsetvld_dtl",
                newName: "IX_edds_dsetvld_dtl_ExecutionId_RowNumber_Code_FieldName");

            migrationBuilder.RenameIndex(
                name: "edds_dsetvld_dtl_N01",
                table: "edds_dsetvld_dtl",
                newName: "IX_edds_dsetvld_dtl_TenantId_ExecutionId_RowNumber");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N02",
                table: "edds_dsetstr_usgsnp",
                newName: "IX_edds_dsetstr_usgsnp_TenantId_PeriodStartUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dsetstr_usgsnp_N01",
                table: "edds_dsetstr_usgsnp",
                newName: "IX_edds_dsetstr_usgsnp_TenantId_CapturedAtUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N02",
                table: "edds_dsetqta_threvt",
                newName: "IX_edds_dsetqta_threvt_TenantId_PolicyId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dsetqta_threvt_N01",
                table: "edds_dsetqta_threvt",
                newName: "IX_edds_dsetqta_threvt_TenantId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "edds_dset_N02",
                table: "edds_dset",
                newName: "IX_edds_dset_TenantId_OrganizationId_ProjectId");

            migrationBuilder.RenameIndex(
                name: "edds_dset_N01",
                table: "edds_dset",
                newName: "IX_edds_dset_Status_CreatedAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_seenkey",
                table: "edds_dsetvld_seenkey",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_prfrev",
                table: "edds_dsetvld_prfrev",
                columns: new[] { "ProfileId", "Revision" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_prf",
                table: "edds_dsetvld_prf",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_exc",
                table: "edds_dsetvld_exc",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetvld_dtl",
                table: "edds_dsetvld_dtl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetstr_usgsnp",
                table: "edds_dsetstr_usgsnp",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetschm_rev",
                table: "edds_dsetschm_rev",
                columns: new[] { "DataSetId", "Version" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dsetqta_threvt",
                table: "edds_dsetqta_threvt",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edds_dset",
                table: "edds_dset",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_edds_dsetschm_rev_edds_dset_DataSetId",
                table: "edds_dsetschm_rev",
                column: "DataSetId",
                principalTable: "edds_dset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
