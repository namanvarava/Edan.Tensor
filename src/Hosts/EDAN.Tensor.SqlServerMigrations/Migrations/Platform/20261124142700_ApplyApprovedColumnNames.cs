using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.SqlServerMigrations.Migrations.Platform
{
    /// <inheritdoc />
    public partial class ApplyApprovedColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_strqta_plcychg_FKPolicyId')
    ALTER TABLE [dbo].[edpf_strqta_plcychg] DROP CONSTRAINT [edpf_strqta_plcychg_FKPolicyId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_prjmbr_FKProjectId')
    ALTER TABLE [dbo].[edpf_prjmbr] DROP CONSTRAINT [edpf_prjmbr_FKProjectId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_plcy_FKScopeProjectId')
    ALTER TABLE [dbo].[edpf_plcy] DROP CONSTRAINT [edpf_plcy_FKScopeProjectId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_authplcy_FKProjectId')
    ALTER TABLE [dbo].[edpf_authplcy] DROP CONSTRAINT [edpf_authplcy_FKProjectId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_rsrc_FKScopeProjectId')
    ALTER TABLE [dbo].[edpf_rsrc] DROP CONSTRAINT [edpf_rsrc_FKScopeProjectId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_prj_FKOrganizationId')
    ALTER TABLE [dbo].[edpf_prj] DROP CONSTRAINT [edpf_prj_FKOrganizationId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_authplcy_FKOrganizationId')
    ALTER TABLE [dbo].[edpf_authplcy] DROP CONSTRAINT [edpf_authplcy_FKOrganizationId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_orgmbr_FKOrganizationId')
    ALTER TABLE [dbo].[edpf_orgmbr] DROP CONSTRAINT [edpf_orgmbr_FKOrganizationId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_plcy_FKScopeOrganizationId')
    ALTER TABLE [dbo].[edpf_plcy] DROP CONSTRAINT [edpf_plcy_FKScopeOrganizationId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_rsrc_FKScopeOrganizationId')
    ALTER TABLE [dbo].[edpf_rsrc] DROP CONSTRAINT [edpf_rsrc_FKScopeOrganizationId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'edpf_authaud_arcattm_FKJobId')
    ALTER TABLE [dbo].[edpf_authaud_arcattm] DROP CONSTRAINT [edpf_authaud_arcattm_FKJobId];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'edpf_tntdbnd_N01' AND object_id = OBJECT_ID(N'dbo.edpf_tntdbnd'))
    DROP INDEX [edpf_tntdbnd_N01] ON [dbo].[edpf_tntdbnd];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'edpf_tntdbnd_N02' AND object_id = OBJECT_ID(N'dbo.edpf_tntdbnd'))
    DROP INDEX [edpf_tntdbnd_N02] ON [dbo].[edpf_tntdbnd];
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_platform_authorization_policies_project_requires_organization')
    ALTER TABLE [dbo].[edpf_authplcy] DROP CONSTRAINT [CK_platform_authorization_policies_project_requires_organization];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'edpf_authplcy_N01' AND object_id = OBJECT_ID(N'dbo.edpf_authplcy'))
    DROP INDEX [edpf_authplcy_N01] ON [dbo].[edpf_authplcy];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'edpf_authplcy_N02' AND object_id = OBJECT_ID(N'dbo.edpf_authplcy'))
    DROP INDEX [edpf_authplcy_N02] ON [dbo].[edpf_authplcy];

DECLARE @dropAuthPolicyProjectFks nvarchar(max) = N'';
SELECT @dropAuthPolicyProjectFks +=
    N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE (fk.parent_object_id = OBJECT_ID(N'dbo.edpf_authplcy') AND c.name = N'ProjectId')
   OR (fk.referenced_object_id = OBJECT_ID(N'dbo.edpf_authplcy') AND EXISTS (
       SELECT 1 FROM sys.foreign_key_columns rfkc
       JOIN sys.columns rc ON rc.object_id = rfkc.referenced_object_id AND rc.column_id = rfkc.referenced_column_id
       WHERE rfkc.constraint_object_id = fk.object_id AND rc.name = N'ProjectId'));
IF @dropAuthPolicyProjectFks <> N''
    EXEC sp_executesql @dropAuthPolicyProjectFks;
");

                        migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'AK_edpf_strqta_plcy_Id_TenantId' AND parent_object_id = OBJECT_ID(N'[dbo].[edpf_strqta_plcy]'))
    ALTER TABLE [dbo].[edpf_strqta_plcy] DROP CONSTRAINT [AK_edpf_strqta_plcy_Id_TenantId];
" );

                        migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'AK_edpf_prj_Id_OrganizationId_TenantId' AND parent_object_id = OBJECT_ID(N'[dbo].[edpf_prj]'))
    ALTER TABLE [dbo].[edpf_prj] DROP CONSTRAINT [AK_edpf_prj_Id_OrganizationId_TenantId];
" );

                        migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'AK_edpf_org_Id_TenantId' AND parent_object_id = OBJECT_ID(N'[dbo].[edpf_org]'))
    ALTER TABLE [dbo].[edpf_org] DROP CONSTRAINT [AK_edpf_org_Id_TenantId];
" );

                        migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'AK_edpf_inv_Id_TenantId' AND parent_object_id = OBJECT_ID(N'[dbo].[edpf_inv]'))
    ALTER TABLE [dbo].[edpf_inv] DROP CONSTRAINT [AK_edpf_inv_Id_TenantId];
" );

                        migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'AK_edpf_authaud_arcjob_Id_TenantId' AND parent_object_id = OBJECT_ID(N'[dbo].[edpf_authaud_arcjob]'))
    ALTER TABLE [dbo].[edpf_authaud_arcjob] DROP CONSTRAINT [AK_edpf_authaud_arcjob_Id_TenantId];
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_usr]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_usr]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_usr].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_usr]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_usr]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_usr].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_usr]', N'Email') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_usr]', N'eml') IS NULL
    EXEC sp_rename N'[dbo].[edpf_usr].[Email]', N'eml', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_usr]', N'DisplayName') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_usr]', N'dspnm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_usr].[DisplayName]', N'dspnm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_usr]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_usr]', N'usrid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_usr].[Id]', N'usrid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'UpdatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'upd_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntrtnalw].[UpdatedAtUtc]', N'upd_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntrtnalw].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'SchemaName') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'schm_nm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntrtnalw].[SchemaName]', N'schm_nm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'IsEnabled') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'ena_yn') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntrtnalw].[IsEnabled]', N'ena_yn', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'CreatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'crt_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntrtnalw].[CreatedAtUtc]', N'crt_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntrtnalw]', N'tntrtnalwid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntrtnalw].[Id]', N'tntrtnalwid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmut_aud].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmut_aud].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'IdempotencyKey') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'idem_key') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmut_aud].[IdempotencyKey]', N'idem_key', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'CreatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'crt_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmut_aud].[CreatedAtUtc]', N'crt_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'CompletedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'cmp_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmut_aud].[CompletedAtUtc]', N'cmp_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmut_aud]', N'tntmutaudid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmut_aud].[Id]', N'tntmutaudid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'Role') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'role') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmbr].[Role]', N'role', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmbr].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'SubjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'subid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmbr].[SubjectId]', N'subid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmbr].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntmbr]', N'tntmbrid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntmbr].[Id]', N'tntmbrid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'Port') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'port') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[Port]', N'port', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'Username') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'usrnm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[Username]', N'usrnm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'UpdatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'upd_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[UpdatedAtUtc]', N'upd_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'ServerAddress') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'svr_addr') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[ServerAddress]', N'svr_addr', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'SecretKey') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'scrt_key') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[SecretKey]', N'scrt_key', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'SchemaName') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'schm_nm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[SchemaName]', N'schm_nm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'Provider') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'prvdr') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[Provider]', N'prvdr', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'DatabaseName') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'db_nm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[DatabaseName]', N'db_nm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'CreatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'crt_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[CreatedAtUtc]', N'crt_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tntdbnd]', N'tntdbndid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tntdbnd].[Id]', N'tntdbndid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tnt]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tnt]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tnt].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tnt]', N'Name') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tnt]', N'nm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tnt].[Name]', N'nm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tnt]', N'CreatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tnt]', N'crt_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tnt].[CreatedAtUtc]', N'crt_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tnt]', N'Code') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tnt]', N'cd') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tnt].[Code]', N'cd', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_tnt]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_tnt]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_tnt].[Id]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcychg].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'PolicyId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'plcyid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcychg].[PolicyId]', N'plcyid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'OccurredAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'occr_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcychg].[OccurredAtUtc]', N'occr_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'ChangeType') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'chg_typ') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcychg].[ChangeType]', N'chg_typ', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'BeforeJson') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'bef_json') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcychg].[BeforeJson]', N'bef_json', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'AfterJson') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'aft_json') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcychg].[AfterJson]', N'aft_json', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'ActorSubjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'act_subid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcychg].[ActorSubjectId]', N'act_subid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcychg]', N'strqtaplcychgid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcychg].[Id]', N'strqtaplcychgid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'Version') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'ver') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcy].[Version]', N'ver', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'UpdatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'upd_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcy].[UpdatedAtUtc]', N'upd_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcy].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'IsEnabled') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'ena_yn') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcy].[IsEnabled]', N'ena_yn', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_strqta_plcy]', N'strqtaplcyid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_strqta_plcy].[Id]', N'strqtaplcyid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_rsrc]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_rsrc]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_rsrc].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_rsrc]', N'ResourceType') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_rsrc]', N'rsrc_typ') IS NULL
    EXEC sp_rename N'[dbo].[edpf_rsrc].[ResourceType]', N'rsrc_typ', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_rsrc]', N'Name') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_rsrc]', N'nm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_rsrc].[Name]', N'nm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_rsrc]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_rsrc]', N'rsrcid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_rsrc].[Id]', N'rsrcid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'Role') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'role') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prjmbr].[Role]', N'role', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prjmbr].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'SubjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'subid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prjmbr].[SubjectId]', N'subid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prjmbr].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'ProjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'prjid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prjmbr].[ProjectId]', N'prjid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'OrganizationId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'orgid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prjmbr].[OrganizationId]', N'orgid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prjmbr]', N'prjmbrid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prjmbr].[Id]', N'prjmbrid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prj]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prj]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prj].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prj]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prj]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prj].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prj]', N'OrganizationId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prj]', N'orgid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prj].[OrganizationId]', N'orgid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prj]', N'Name') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prj]', N'nm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prj].[Name]', N'nm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prj]', N'Code') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prj]', N'cd') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prj].[Code]', N'cd', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_prj]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_prj]', N'prjid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_prj].[Id]', N'prjid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_plcy]', N'Version') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_plcy]', N'ver') IS NULL
    EXEC sp_rename N'[dbo].[edpf_plcy].[Version]', N'ver', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_plcy]', N'IsEnabled') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_plcy]', N'ena_yn') IS NULL
    EXEC sp_rename N'[dbo].[edpf_plcy].[IsEnabled]', N'ena_yn', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_plcy]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_plcy]', N'plcyid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_plcy].[Id]', N'plcyid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'Role') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'role') IS NULL
    EXEC sp_rename N'[dbo].[edpf_orgmbr].[Role]', N'role', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_orgmbr].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'SubjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'subid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_orgmbr].[SubjectId]', N'subid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_orgmbr].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'OrganizationId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'orgid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_orgmbr].[OrganizationId]', N'orgid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_orgmbr]', N'orgmbrid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_orgmbr].[Id]', N'orgmbrid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_org]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_org]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_org].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_org]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_org]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_org].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_org]', N'Name') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_org]', N'nm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_org].[Name]', N'nm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_org]', N'Code') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_org]', N'cd') IS NULL
    EXEC sp_rename N'[dbo].[edpf_org].[Code]', N'cd', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_org]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_org]', N'orgid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_org].[Id]', N'orgid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_inv]', N'UserId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_inv]', N'usrid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_inv].[UserId]', N'usrid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_inv]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_inv]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_inv].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_inv]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_inv]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_inv].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_inv]', N'Email') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_inv]', N'eml') IS NULL
    EXEC sp_rename N'[dbo].[edpf_inv].[Email]', N'eml', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_inv]', N'DisplayName') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_inv]', N'dspnm') IS NULL
    EXEC sp_rename N'[dbo].[edpf_inv].[DisplayName]', N'dspnm', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_inv]', N'CreatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_inv]', N'crt_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_inv].[CreatedAtUtc]', N'crt_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_inv]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_inv]', N'invid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_inv].[Id]', N'invid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy_chg].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'PolicyId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'plcyid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy_chg].[PolicyId]', N'plcyid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'OccurredAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'occr_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy_chg].[OccurredAtUtc]', N'occr_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'ChangeType') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'chg_typ') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy_chg].[ChangeType]', N'chg_typ', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'BeforeJson') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'bef_json') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy_chg].[BeforeJson]', N'bef_json', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'AfterJson') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'aft_json') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy_chg].[AfterJson]', N'aft_json', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'ActorSubjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'act_subid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy_chg].[ActorSubjectId]', N'act_subid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy_chg]', N'authplcychgid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy_chg].[Id]', N'authplcychgid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy]', N'SubjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy]', N'subid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy].[SubjectId]', N'subid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy]', N'ResourceType') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy]', N'rsrc_typ') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy].[ResourceType]', N'rsrc_typ', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy]', N'ProjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy]', N'prjid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy].[ProjectId]', N'prjid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy]', N'OrganizationId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy]', N'orgid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy].[OrganizationId]', N'orgid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy]', N'IsEnabled') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy]', N'ena_yn') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy].[IsEnabled]', N'ena_yn', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy]', N'CreatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy]', N'crt_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy].[CreatedAtUtc]', N'crt_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authplcy]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authplcy]', N'authplcyid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authplcy].[Id]', N'authplcyid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtnchg].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'PolicyId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'plcyid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtnchg].[PolicyId]', N'plcyid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'OccurredAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'occr_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtnchg].[OccurredAtUtc]', N'occr_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'ChangeType') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'chg_typ') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtnchg].[ChangeType]', N'chg_typ', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'BeforeJson') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'bef_json') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtnchg].[BeforeJson]', N'bef_json', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'AfterJson') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'aft_json') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtnchg].[AfterJson]', N'aft_json', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'ActorSubjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'act_subid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtnchg].[ActorSubjectId]', N'act_subid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtnchg]', N'authaudrtnchgid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtnchg].[Id]', N'authaudrtnchgid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'Version') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'ver') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtn].[Version]', N'ver', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'UpdatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'upd_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtn].[UpdatedAtUtc]', N'upd_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtn].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'RetentionDays') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'rtn_days') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtn].[RetentionDays]', N'rtn_days', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_rtn]', N'authaudrtnid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_rtn].[Id]', N'authaudrtnid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'Version') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'ver') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcschd].[Version]', N'ver', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcschd].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'NextRunAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'nxt_run_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcschd].[NextRunAtUtc]', N'nxt_run_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'LeaseExpiresAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'lease_exp_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcschd].[LeaseExpiresAtUtc]', N'lease_exp_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'LastRunAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'last_run_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcschd].[LastRunAtUtc]', N'last_run_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'IsEnabled') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'ena_yn') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcschd].[IsEnabled]', N'ena_yn', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcschd]', N'authaudarcschdid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcschd].[Id]', N'authaudarcschdid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'Version') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'ver') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcjob].[Version]', N'ver', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcjob].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'Status') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'st') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcjob].[Status]', N'st', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'CreatedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'crt_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcjob].[CreatedAtUtc]', N'crt_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'CompletedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'cmp_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcjob].[CompletedAtUtc]', N'cmp_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'AttemptCount') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'attm_cnt') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcjob].[AttemptCount]', N'attm_cnt', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcjob]', N'authaudarcjobid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcjob].[Id]', N'authaudarcjobid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'JobId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'jobid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcattm].[JobId]', N'jobid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcattm].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'StartedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'strt_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcattm].[StartedAtUtc]', N'strt_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'FinishedAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'fnsh_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcattm].[FinishedAtUtc]', N'fnsh_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud_arcattm]', N'authaudarcattmid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud_arcattm].[Id]', N'authaudarcattmid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud]', N'TenantId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud]', N'tntid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud].[TenantId]', N'tntid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud]', N'SubjectId') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud]', N'subid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud].[SubjectId]', N'subid', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud]', N'ResourceType') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud]', N'rsrc_typ') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud].[ResourceType]', N'rsrc_typ', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud]', N'OccurredAtUtc') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud]', N'occr_utc') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud].[OccurredAtUtc]', N'occr_utc', 'COLUMN';
" );

                        migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[edpf_authaud]', N'Id') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[edpf_authaud]', N'authaudid') IS NULL
    EXEC sp_rename N'[dbo].[edpf_authaud].[Id]', N'authaudid', 'COLUMN';
" );

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_platform_authorization_policies_project_requires_organization')
    ALTER TABLE [dbo].[edpf_authplcy] ADD CONSTRAINT [CK_platform_authorization_policies_project_requires_organization]
        CHECK ([prjid] IS NULL OR [orgid] IS NOT NULL);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'edpf_authplcy_N01' AND object_id = OBJECT_ID(N'dbo.edpf_authplcy'))
    CREATE INDEX [edpf_authplcy_N01] ON [dbo].[edpf_authplcy] ([orgid], [tntid]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'edpf_authplcy_N02' AND object_id = OBJECT_ID(N'dbo.edpf_authplcy'))
    CREATE INDEX [edpf_authplcy_N02] ON [dbo].[edpf_authplcy] ([prjid], [orgid], [tntid]);
" );

            migrationBuilder.CreateIndex(
                name: "edpf_tntdbnd_N01",
                table: "edpf_tntdbnd",
                columns: new[] { "tntid", "prvdr" });

            migrationBuilder.CreateIndex(
                name: "edpf_tntdbnd_N02",
                table: "edpf_tntdbnd",
                columns: new[] { "tntid", "st" },
                unique: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_strqta_plcy_strqtaplcyid_tntid",
                table: "edpf_strqta_plcy",
                columns: new[] { "strqtaplcyid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_prj_prjid_orgid_tntid",
                table: "edpf_prj",
                columns: new[] { "prjid", "orgid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_org_orgid_tntid",
                table: "edpf_org",
                columns: new[] { "orgid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_inv_invid_tntid",
                table: "edpf_inv",
                columns: new[] { "invid", "tntid" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_authaudarcjobid_tntid",
                table: "edpf_authaud_arcjob",
                columns: new[] { "authaudarcjobid", "tntid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_strqta_plcy_strqtaplcyid_tntid",
                table: "edpf_strqta_plcy");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_prj_prjid_orgid_tntid",
                table: "edpf_prj");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_org_orgid_tntid",
                table: "edpf_org");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_inv_invid_tntid",
                table: "edpf_inv");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_authaudarcjobid_tntid",
                table: "edpf_authaud_arcjob");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_usr",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_usr",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "eml",
                table: "edpf_usr",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "dspnm",
                table: "edpf_usr",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "usrid",
                table: "edpf_usr",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "edpf_tntrtnalw",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tntrtnalw",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "schm_nm",
                table: "edpf_tntrtnalw",
                newName: "SchemaName");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_tntrtnalw",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_tntrtnalw",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntrtnalwid",
                table: "edpf_tntrtnalw",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tntmut_aud",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_tntmut_aud",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "idem_key",
                table: "edpf_tntmut_aud",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_tntmut_aud",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "edpf_tntmut_aud",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntmutaudid",
                table: "edpf_tntmut_aud",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "edpf_tntmbr",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tntmbr",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_tntmbr",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_tntmbr",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "tntmbrid",
                table: "edpf_tntmbr",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "port",
                table: "edpf_tntdbnd",
                newName: "Port");

            migrationBuilder.RenameColumn(
                name: "usrnm",
                table: "edpf_tntdbnd",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "edpf_tntdbnd",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tntdbnd",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "svr_addr",
                table: "edpf_tntdbnd",
                newName: "ServerAddress");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_tntdbnd",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "scrt_key",
                table: "edpf_tntdbnd",
                newName: "SecretKey");

            migrationBuilder.RenameColumn(
                name: "schm_nm",
                table: "edpf_tntdbnd",
                newName: "SchemaName");

            migrationBuilder.RenameColumn(
                name: "prvdr",
                table: "edpf_tntdbnd",
                newName: "Provider");

            migrationBuilder.RenameColumn(
                name: "db_nm",
                table: "edpf_tntdbnd",
                newName: "DatabaseName");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_tntdbnd",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntdbndid",
                table: "edpf_tntdbnd",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_tnt",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_tnt",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_tnt",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edpf_tnt",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_tnt",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_strqta_plcychg",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edpf_strqta_plcychg",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_strqta_plcychg",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "chg_typ",
                table: "edpf_strqta_plcychg",
                newName: "ChangeType");

            migrationBuilder.RenameColumn(
                name: "bef_json",
                table: "edpf_strqta_plcychg",
                newName: "BeforeJson");

            migrationBuilder.RenameColumn(
                name: "aft_json",
                table: "edpf_strqta_plcychg",
                newName: "AfterJson");

            migrationBuilder.RenameColumn(
                name: "act_subid",
                table: "edpf_strqta_plcychg",
                newName: "ActorSubjectId");

            migrationBuilder.RenameColumn(
                name: "strqtaplcychgid",
                table: "edpf_strqta_plcychg",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_strqta_plcy",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "edpf_strqta_plcy",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_strqta_plcy",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_strqta_plcy",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "strqtaplcyid",
                table: "edpf_strqta_plcy",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_rsrc",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "rsrc_typ",
                table: "edpf_rsrc",
                newName: "ResourceType");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_rsrc",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "rsrcid",
                table: "edpf_rsrc",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "edpf_prjmbr",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_prjmbr",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_prjmbr",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_prjmbr",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "prjid",
                table: "edpf_prjmbr",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_prjmbr",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "prjmbrid",
                table: "edpf_prjmbr",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_prj",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_prj",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_prj",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_prj",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edpf_prj",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "prjid",
                table: "edpf_prj",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_plcy",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_plcy",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edpf_plcy",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "edpf_orgmbr",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_orgmbr",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_orgmbr",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_orgmbr",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_orgmbr",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "orgmbrid",
                table: "edpf_orgmbr",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_org",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_org",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "nm",
                table: "edpf_org",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "cd",
                table: "edpf_org",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_org",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "usrid",
                table: "edpf_inv",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_inv",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_inv",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "eml",
                table: "edpf_inv",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "dspnm",
                table: "edpf_inv",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_inv",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "invid",
                table: "edpf_inv",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authplcy_chg",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edpf_authplcy_chg",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_authplcy_chg",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "chg_typ",
                table: "edpf_authplcy_chg",
                newName: "ChangeType");

            migrationBuilder.RenameColumn(
                name: "bef_json",
                table: "edpf_authplcy_chg",
                newName: "BeforeJson");

            migrationBuilder.RenameColumn(
                name: "aft_json",
                table: "edpf_authplcy_chg",
                newName: "AfterJson");

            migrationBuilder.RenameColumn(
                name: "act_subid",
                table: "edpf_authplcy_chg",
                newName: "ActorSubjectId");

            migrationBuilder.RenameColumn(
                name: "authplcychgid",
                table: "edpf_authplcy_chg",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authplcy",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_authplcy",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "rsrc_typ",
                table: "edpf_authplcy",
                newName: "ResourceType");

            migrationBuilder.RenameColumn(
                name: "prjid",
                table: "edpf_authplcy",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "orgid",
                table: "edpf_authplcy",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_authplcy",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_authplcy",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "authplcyid",
                table: "edpf_authplcy",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_rtnchg",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "plcyid",
                table: "edpf_authaud_rtnchg",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_authaud_rtnchg",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "chg_typ",
                table: "edpf_authaud_rtnchg",
                newName: "ChangeType");

            migrationBuilder.RenameColumn(
                name: "bef_json",
                table: "edpf_authaud_rtnchg",
                newName: "BeforeJson");

            migrationBuilder.RenameColumn(
                name: "aft_json",
                table: "edpf_authaud_rtnchg",
                newName: "AfterJson");

            migrationBuilder.RenameColumn(
                name: "act_subid",
                table: "edpf_authaud_rtnchg",
                newName: "ActorSubjectId");

            migrationBuilder.RenameColumn(
                name: "authaudrtnchgid",
                table: "edpf_authaud_rtnchg",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_authaud_rtn",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "upd_utc",
                table: "edpf_authaud_rtn",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_rtn",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "rtn_days",
                table: "edpf_authaud_rtn",
                newName: "RetentionDays");

            migrationBuilder.RenameColumn(
                name: "authaudrtnid",
                table: "edpf_authaud_rtn",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_authaud_arcschd",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_arcschd",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "nxt_run_utc",
                table: "edpf_authaud_arcschd",
                newName: "NextRunAtUtc");

            migrationBuilder.RenameColumn(
                name: "lease_exp_utc",
                table: "edpf_authaud_arcschd",
                newName: "LeaseExpiresAtUtc");

            migrationBuilder.RenameColumn(
                name: "last_run_utc",
                table: "edpf_authaud_arcschd",
                newName: "LastRunAtUtc");

            migrationBuilder.RenameColumn(
                name: "ena_yn",
                table: "edpf_authaud_arcschd",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "authaudarcschdid",
                table: "edpf_authaud_arcschd",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ver",
                table: "edpf_authaud_arcjob",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_arcjob",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "st",
                table: "edpf_authaud_arcjob",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "crt_utc",
                table: "edpf_authaud_arcjob",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "cmp_utc",
                table: "edpf_authaud_arcjob",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "attm_cnt",
                table: "edpf_authaud_arcjob",
                newName: "AttemptCount");

            migrationBuilder.RenameColumn(
                name: "authaudarcjobid",
                table: "edpf_authaud_arcjob",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "jobid",
                table: "edpf_authaud_arcattm",
                newName: "JobId");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud_arcattm",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "strt_utc",
                table: "edpf_authaud_arcattm",
                newName: "StartedAtUtc");

            migrationBuilder.RenameColumn(
                name: "fnsh_utc",
                table: "edpf_authaud_arcattm",
                newName: "FinishedAtUtc");

            migrationBuilder.RenameColumn(
                name: "authaudarcattmid",
                table: "edpf_authaud_arcattm",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tntid",
                table: "edpf_authaud",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "subid",
                table: "edpf_authaud",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "rsrc_typ",
                table: "edpf_authaud",
                newName: "ResourceType");

            migrationBuilder.RenameColumn(
                name: "occr_utc",
                table: "edpf_authaud",
                newName: "OccurredAtUtc");

            migrationBuilder.RenameColumn(
                name: "authaudid",
                table: "edpf_authaud",
                newName: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_strqta_plcy_Id_TenantId",
                table: "edpf_strqta_plcy",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_prj_Id_OrganizationId_TenantId",
                table: "edpf_prj",
                columns: new[] { "Id", "OrganizationId", "TenantId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_org_Id_TenantId",
                table: "edpf_org",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_inv_Id_TenantId",
                table: "edpf_inv",
                columns: new[] { "Id", "TenantId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_edpf_authaud_arcjob_Id_TenantId",
                table: "edpf_authaud_arcjob",
                columns: new[] { "Id", "TenantId" });
        }
    }
}

