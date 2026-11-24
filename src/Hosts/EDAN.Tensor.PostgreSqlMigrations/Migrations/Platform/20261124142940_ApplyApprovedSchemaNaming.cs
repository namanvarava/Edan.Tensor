using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.PostgreSqlMigrations.Migrations.Platform;

/// <summary>
/// 기존 PostgreSQL Platform 스키마를 승인된 전자사전 이름으로 보존형 변경합니다.
/// 테이블·컬럼이 실제로 존재할 때만 Rename을 수행하며 Drop/Create는 수행하지 않습니다.
/// </summary>
public partial class ApplyApprovedSchemaNaming : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DO $$
DECLARE r record;
BEGIN
  FOR r IN SELECT * FROM (VALUES
    ('platform_users','edpf_usr'),('platform_tenants','edpf_tnt'),
    ('platform_tenant_memberships','edpf_tntmbr'),
    ('platform_tenant_database_routine_allowlists','edpf_tntrtnalw'),
    ('platform_tenant_database_mutation_audits','edpf_tntmut_aud'),
    ('platform_tenant_database_bindings','edpf_tntdbnd'),
    ('platform_storage_quota_policy_changes','edpf_strqta_plcychg'),
    ('platform_storage_quota_policies','edpf_strqta_plcy'),
    ('platform_resources','edpf_rsrc'),('platform_projects','edpf_prj'),
    ('platform_project_memberships','edpf_prjmbr'),('platform_policies','edpf_plcy'),
    ('platform_organizations','edpf_org'),('platform_organization_memberships','edpf_orgmbr'),
    ('platform_invitations','edpf_inv'),
    ('platform_authorization_policy_changes','edpf_authplcy_chg'),
    ('platform_authorization_policies','edpf_authplcy'),
    ('platform_authorization_audit_retention_changes','edpf_authaud_rtnchg'),
    ('platform_authorization_audit_retention','edpf_authaud_rtn'),
    ('platform_authorization_audit_archive_schedules','edpf_authaud_arcschd'),
    ('platform_authorization_audit_archive_jobs','edpf_authaud_arcjob'),
    ('platform_authorization_audit_archive_attempts','edpf_authaud_arcattm'),
    ('platform_authorization_audit','edpf_authaud')
  ) AS x(old_name,new_name)
  LOOP
    IF to_regclass(format('public.%I', r.old_name)) IS NOT NULL
       AND to_regclass(format('public.%I', r.new_name)) IS NULL THEN
      EXECUTE format('ALTER TABLE %I RENAME TO %I', r.old_name, r.new_name);
    END IF;
  END LOOP;
END $$;");

        migrationBuilder.Sql(@"
DO $$
DECLARE r record;
BEGIN
  FOR r IN SELECT * FROM (VALUES
    ('edpf_tnt','Id','tntid'),('edpf_tnt','Code','tntcd'),('edpf_tnt','Name','tntnm'),('edpf_tnt','Status','st'),('edpf_tnt','CreatedAtUtc','crt_utc'),
    ('edpf_usr','Id','usrid'),('edpf_usr','TenantId','tntid'),('edpf_usr','Email','eml'),('edpf_usr','DisplayName','dspnm'),('edpf_usr','Status','st'),
    ('edpf_org','Id','orgid'),('edpf_org','TenantId','tntid'),('edpf_org','Code','orgcd'),('edpf_org','Name','orgnm'),('edpf_org','Status','st'),
    ('edpf_prj','Id','prjid'),('edpf_prj','TenantId','tntid'),('edpf_prj','OrganizationId','orgid'),('edpf_prj','Code','prjcd'),('edpf_prj','Name','prjnm'),('edpf_prj','Status','st'),
    ('edpf_tntmbr','Id','tnt_mbrid'),('edpf_tntmbr','TenantId','tntid'),('edpf_tntmbr','SubjectId','subid'),('edpf_tntmbr','Role','role'),('edpf_tntmbr','Status','st'),
    ('edpf_orgmbr','Id','org_mbrid'),('edpf_orgmbr','TenantId','tntid'),('edpf_orgmbr','OrganizationId','orgid'),('edpf_orgmbr','SubjectId','subid'),('edpf_orgmbr','Role','role'),('edpf_orgmbr','Status','st'),
    ('edpf_prjmbr','Id','prj_mbrid'),('edpf_prjmbr','TenantId','tntid'),('edpf_prjmbr','OrganizationId','orgid'),('edpf_prjmbr','ProjectId','prjid'),('edpf_prjmbr','SubjectId','subid'),('edpf_prjmbr','Role','role'),('edpf_prjmbr','Status','st'),
    ('edpf_rsrc','Id','rsrcid'),('edpf_rsrc','Name','rsrcnm'),('edpf_rsrc','ResourceType','rsrc_typ'),('edpf_rsrc','Status','st'),
    ('edpf_plcy','Id','plcyid'),('edpf_plcy','Version','ver'),('edpf_plcy','IsEnabled','ena_yn'),
    ('edpf_authplcy','Id','auth_plcyid'),('edpf_authplcy','TenantId','tntid'),('edpf_authplcy','SubjectId','subid'),('edpf_authplcy','Action','act'),('edpf_authplcy','Effect','eff'),('edpf_authplcy','Priority','pri'),('edpf_authplcy','ResourceType','rsrc_typ'),('edpf_authplcy','OrganizationId','orgid'),('edpf_authplcy','ProjectId','prjid'),('edpf_authplcy','IsEnabled','ena_yn'),('edpf_authplcy','CreatedAtUtc','crt_utc'),
    ('edpf_authaud','Id','auth_audid'),('edpf_authaud','TenantId','tntid'),('edpf_authaud','SubjectId','subid'),('edpf_authaud','Action','act'),('edpf_authaud','ResourceType','rsrc_typ'),('edpf_authaud','Scope','scp'),('edpf_authaud','IsAllowed','allow_yn'),('edpf_authaud','ReasonCode','rsn_cd'),('edpf_authaud','MatchedPolicyId','plcyid'),('edpf_authaud','OccurredAtUtc','occ_utc'),
    ('edpf_authplcy_chg','Id','auth_plcy_chgid'),('edpf_authplcy_chg','TenantId','tntid'),('edpf_authplcy_chg','PolicyId','plcyid'),('edpf_authplcy_chg','OccurredAtUtc','occ_utc'),('edpf_authplcy_chg','ActorSubjectId','act_subid'),('edpf_authplcy_chg','ChangeType','chg_typ'),('edpf_authplcy_chg','BeforeJson','bef_json'),('edpf_authplcy_chg','AfterJson','aft_json'),
    ('edpf_authaud_rtn','Id','authaudrtnid'),('edpf_authaud_rtn','TenantId','tntid'),('edpf_authaud_rtn','Version','ver'),('edpf_authaud_rtn','UpdatedAtUtc','upd_utc'),('edpf_authaud_rtn','RetentionDays','rtn_days'),
    ('edpf_authaud_rtnchg','Id','authaudrtnchgid'),('edpf_authaud_rtnchg','TenantId','tntid'),('edpf_authaud_rtnchg','PolicyId','plcyid'),('edpf_authaud_rtnchg','OccurredAtUtc','occ_utc'),('edpf_authaud_rtnchg','ActorSubjectId','act_subid'),('edpf_authaud_rtnchg','ChangeType','chg_typ'),('edpf_authaud_rtnchg','BeforeJson','bef_json'),('edpf_authaud_rtnchg','AfterJson','aft_json'),
    ('edpf_authaud_arcschd','Id','auth_aud_arc_schdid'),('edpf_authaud_arcschd','TenantId','tntid'),('edpf_authaud_arcschd','Version','ver'),('edpf_authaud_arcschd','IntervalHours','int_hr'),('edpf_authaud_arcschd','IsEnabled','ena_yn'),('edpf_authaud_arcschd','NextRunAtUtc','nxt_utc'),('edpf_authaud_arcschd','LastRunAtUtc','last_utc'),('edpf_authaud_arcschd','LeaseExpiresAtUtc','lease_exp_utc'),
    ('edpf_authaud_arcjob','Id','authaudarcjobid'),('edpf_authaud_arcjob','TenantId','tntid'),('edpf_authaud_arcjob','Version','ver'),('edpf_authaud_arcjob','Status','st'),('edpf_authaud_arcjob','CreatedAtUtc','crt_utc'),('edpf_authaud_arcjob','CompletedAtUtc','cmp_utc'),('edpf_authaud_arcjob','AttemptCount','attm_cnt'),
    ('edpf_authaud_arcattm','Id','authaudarcattmid'),('edpf_authaud_arcattm','JobId','jobid'),('edpf_authaud_arcattm','TenantId','tntid'),('edpf_authaud_arcattm','StartedAtUtc','strt_utc'),('edpf_authaud_arcattm','FinishedAtUtc','fnsh_utc'),
    ('edpf_strqta_plcy','Id','strqtaplcyid'),('edpf_strqta_plcy','TenantId','tntid'),('edpf_strqta_plcy','Version','ver'),('edpf_strqta_plcy','UpdatedAtUtc','upd_utc'),('edpf_strqta_plcy','IsEnabled','ena_yn'),
    ('edpf_strqta_plcychg','Id','strqtaplcychgid'),('edpf_strqta_plcychg','TenantId','tntid'),('edpf_strqta_plcychg','PolicyId','plcyid'),('edpf_strqta_plcychg','OccurredAtUtc','occ_utc'),('edpf_strqta_plcychg','ActorSubjectId','act_subid'),('edpf_strqta_plcychg','ChangeType','chg_typ'),('edpf_strqta_plcychg','BeforeJson','bef_json'),('edpf_strqta_plcychg','AfterJson','aft_json'),
    ('edpf_tntdbnd','Id','tnt_dbndid'),('edpf_tntdbnd','TenantId','tntid'),('edpf_tntdbnd','Provider','prvdr'),('edpf_tntdbnd','Username','usrnm'),('edpf_tntdbnd','SecretKey','scrt_key'),('edpf_tntdbnd','ServerAddress','svr_addr'),('edpf_tntdbnd','Port','port'),('edpf_tntdbnd','DatabaseName','db_nm'),('edpf_tntdbnd','SchemaName','schm_nm'),('edpf_tntdbnd','Status','st'),('edpf_tntdbnd','CreatedAtUtc','crt_utc'),('edpf_tntdbnd','UpdatedAtUtc','upd_utc'),
    ('edpf_tntrtnalw','Id','tntrtnalwid'),('edpf_tntrtnalw','TenantId','tntid'),('edpf_tntrtnalw','SchemaName','schm_nm'),('edpf_tntrtnalw','IsEnabled','ena_yn'),('edpf_tntrtnalw','CreatedAtUtc','crt_utc'),('edpf_tntrtnalw','UpdatedAtUtc','upd_utc'),
    ('edpf_tntmut_aud','Id','tnt_mut_audid'),('edpf_tntmut_aud','TenantId','tntid'),('edpf_tntmut_aud','Status','st'),('edpf_tntmut_aud','IdempotencyKey','idem_key'),('edpf_tntmut_aud','CreatedAtUtc','crt_utc'),('edpf_tntmut_aud','CompletedAtUtc','cmp_utc')
  ) AS x(table_name,old_name,new_name)
  LOOP
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name=r.table_name AND column_name=r.old_name)
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name=r.table_name AND column_name=r.new_name) THEN
      EXECUTE format('ALTER TABLE %I RENAME COLUMN %I TO %I', r.table_name, r.old_name, r.new_name);
    END IF;
  END LOOP;
END $$;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Down migration is intentionally empty; production rollback uses the pre-apply backup.
    }
}
