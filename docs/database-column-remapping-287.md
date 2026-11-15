# EDAN Tensor 기존 287개 DB 컬럼 전역 표준 재매핑 보고서

- 기준 원본: `붙여넣은 텍스트(1)(6).txt` (287행, 4개 SQLite DB, 33개 테이블)
- 기준 표준: `docs/database-naming-conventions.md` 및 `docs/global-term-abbreviation-audit.md`
- 작업 브랜치: `standard/enterprise-term-dictionary-2026-11-15`
- 판정 성격: 인벤토리의 기존 제안명 재매핑 설계안. 실제 DB 스키마 변경 또는 Migration 실행 결과가 아님.

## 1. 전체 집계

| 항목 | 건수 |
|---|---:|
| 전체 컬럼 | 287 |
| 기존 제안 대비 변경 권장 | 186 |
| 기존 제안 유지 | 101 |
| 의미 확인 후 승인 필요 | 4 |
| 외부 표준 표기 예외 | 1 |
| 신규 컬럼명 고유값 | 165 |
| 동일 테이블 내 신규 컬럼명 중복 | 0 |

## 2. 데이터베이스별 결과

| 데이터베이스 | 테이블 수 | 컬럼 수 | 변경 권장 |
|---|---:|---:|---:|
| edan-tensor-platform.db | 19 | 154 | 106 |
| edan-tensor-dataset.db | 9 | 77 | 47 |
| edan-tensor-dataset-content.db | 1 | 6 | 4 |
| edan-worker.db | 4 | 50 | 29 |

## 3. 표준 적용 원칙

1. 약어 토큰은 대표 용어 사전의 3자 기본·4자 충돌 회피 규칙을 적용한다.
2. 엔터티 식별자는 `{entity_abbr}{idr}` 형태로 구성한다. 예: `tntidr`, `orgidr`, `pjtidr`, `dsetidr`.
3. 엔터티 접두어와 단순 속성 토큰은 연결한다. 예: `tntcod`, `tntnam`, `tntsta`.
4. 의미가 다른 복합 속성은 밑줄로 구분한다. 예: `warn_thr_pct`, `actr_subidr`, `lgl_hld_ynf`.
5. UTC 시각은 `_utc`, Unix Epoch 밀리초 정수는 `_uep_msec`로 구분한다.
6. Project의 기존 `prj` 접두어는 대표 약어 `pjt`로 통일한다.
7. `sha256`은 암호학 알고리즘의 고정 표기 예외로 보존한다.
8. CLR 속성명·API 계약·실제 DB 컬럼명은 별개다. 이번 문서만으로 물리 스키마를 변경하지 않는다.

## 4. 주요 교정 규칙

| 기존 표현 | 신규 표준 | 근거 |
|---|---|---|
| `id` | `idr` | Identifier 2자 레거시 약어 교정 |
| `cd` | `cod` | Code 2자 레거시 약어 교정 |
| `nm` | `nam` | Name 2자 레거시 약어 교정 |
| `st` | `sta` | Status 약어 표준화 |
| `role` | `rol` | Role 대표 약어 |
| `op` | `opr` | Operation 2자 레거시 약어 교정 |
| `prj` | `pjt` | Project 대표 약어 통일 |
| `crtdt` / `upddt` | `crt_utc` / `upd_utc` | UTC 시각 의미 명시 |
| `int_hr` | `intv_hou` | Interval + Hour 의미 명확화 |
| `allow_yn` | `alw_ynf` | Allow + Yes/No 의미 명시 |
| `legal_hold_yn` | `lgl_hld_ynf` | Legal Hold + Yes/No |
| `act_subid` | `actr_subidr` | Actor + Subject + Identifier |
| `req_subid` | `req_actr_idr` | 요청 행위 주체 식별자 후보 |
| `used_bytes` | `usq_byt` | Used Quantity + Byte |
| `ready_utc_ms` | `rdy_uep_msec` | Ready + Unix Epoch + Millisecond |
| `lease_exp_utc_ms` | `lse_exr_uep_msec` | Lease Expiration + Unix Epoch + Millisecond |

## 5. 의미 확인 후 승인해야 하는 컬럼

| 테이블 | 현재 컬럼 | 권장 컬럼 | 확인할 사항 |
|---|---|---|---|
| platform_authorization_audit_retention | UpdatedBy | `upd_actr_idr` | 내부 사용자 ID인지 외부 인증 Subject인지 코드·FK·API 계약 확인 |
| platform_authorization_audit_archive_jobs | RequestedBy | `req_actr_idr` | 내부 사용자 ID인지 외부 인증 Subject인지 코드·FK·API 계약 확인 |
| platform_storage_quota_policies | UpdatedBy | `upd_actr_idr` | 내부 사용자 ID인지 외부 인증 Subject인지 코드·FK·API 계약 확인 |
| worker_task_replays | RequestedBy | `req_actr_idr` | 내부 사용자 ID인지 외부 인증 Subject인지 코드·FK·API 계약 확인 |

`UpdatedBy`와 `RequestedBy`는 이름만으로 저장값의 참조 대상을 확정할 수 없다. 실제 저장값 확인 후 `*_actr_idr` 또는 `*_subidr`를 승인한다.

## 6. 전체 287개 컬럼 재매핑

| DB | 테이블 | 현재 CLR 컬럼 | 기존 권장안 | 현재 제안 | 신규 표준 권장안 | 논리명 | 판정 |
|---|---|---|---|---|---|---|---|
| edan-tensor-platform.db | platform_tenants | `Id` | `tntid` | `tntid` | `tntidr` | 터넌트 식별자 | 표준 전환 권장 |
| edan-tensor-platform.db | platform_tenants | `Code` | `tntcd` | `tntcd` | `tntcod` | 터넌트 코드 | 표준 전환 권장 |
| edan-tensor-platform.db | platform_tenants | `Name` | `tntnm` | `tntnm` | `tntnam` | 터넌트 이름 | 표준 전환 권장 |
| edan-tensor-platform.db | platform_tenants | `Status` | `tntst` | `tntst` | `tntsta` | 터넌트 상태 | 표준 전환 권장 |
| edan-tensor-platform.db | platform_tenants | `CreatedAtUtc` | `crtdt` | `crt_utc` | `crt_utc` | 최초 등록일시 | 기존 후보 유지 |
| edan-tensor-platform.db | platform_organizations | `Id` | `orgid` | `orgid` | `orgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organizations | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organizations | `Code` | `orgcd` | `orgcd` | `orgcod` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organizations | `Name` | `orgnm` | `orgnm` | `orgnam` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organizations | `Status` | `orgst` | `orgst` | `orgsta` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_projects | `Id` | `prjid` | `prjid` | `pjtidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_projects | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_projects | `OrganizationId` | `orgid` | `orgid` | `orgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_projects | `Code` | `prjcd` | `prjcd` | `pjtcod` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_projects | `Name` | `prjnm` | `prjnm` | `pjtnam` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_projects | `Status` | `prjst` | `prjst` | `pjtsta` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_users | `Id` | `usrid` | `usrid` | `usridr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_users | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_users | `Email` | `eml` | `eml` | `eml` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_users | `DisplayName` | `dspnm` | `dspnm` | `dsp_nam` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_users | `Status` | `usrst` | `usrst` | `usrsta` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_tenant_memberships | `Id` | `tnt_mbrid` | `tnt_mbrid` | `tnt_mbridr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_tenant_memberships | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_tenant_memberships | `SubjectId` | `subid` | `subid` | `subidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_tenant_memberships | `Role` | `role` | `role` | `rol` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_tenant_memberships | `Status` | `tnt_mbrst` | `tnt_mbrst` | `tnt_mbrsta` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organization_memberships | `Id` | `org_mbrid` | `org_mbrid` | `org_mbridr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organization_memberships | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organization_memberships | `OrganizationId` | `orgid` | `orgid` | `orgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organization_memberships | `SubjectId` | `subid` | `subid` | `subidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organization_memberships | `Role` | `role` | `role` | `rol` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_organization_memberships | `Status` | `org_mbrst` | `org_mbrst` | `org_mbrsta` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_project_memberships | `Id` | `prj_mbrid` | `prj_mbrid` | `pjt_mbridr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_project_memberships | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_project_memberships | `OrganizationId` | `orgid` | `orgid` | `orgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_project_memberships | `ProjectId` | `prjid` | `prjid` | `pjtidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_project_memberships | `SubjectId` | `subid` | `subid` | `subidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_project_memberships | `Role` | `role` | `role` | `rol` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_project_memberships | `Status` | `prj_mbrst` | `prj_mbrst` | `pjt_mbrsta` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_resources | `Id` | `rsrcid` | `rsrcid` | `rsrcidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_resources | `tenant_id` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_resources | `organization_id` | `orgid` | `orgid` | `orgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_resources | `project_id` | `prjid` | `prjid` | `pjtidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_resources | `ResourceType` | `rsrc_typ` | `rsrc_typ` | `rsrc_typ` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_resources | `Name` | `rsrcnm` | `rsrcnm` | `rsrcnam` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_resources | `Status` | `rsrcst` | `rsrcst` | `rsrcsta` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_policies | `Id` | `plcyid` | `plcyid` | `plcyidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_policies | `tenant_id` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_policies | `organization_id` | `orgid` | `orgid` | `orgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_policies | `project_id` | `prjid` | `prjid` | `pjtidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_policies | `Key` | `plcykey` | `plcykey` | `plcy_key` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_policies | `Value` | `val` | `val` | `val` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_policies | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_policies | `IsEnabled` | `ena_yn` | `ena_yn` | `ena` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policies | `Id` | `auth_plcyid` | `auth_plcyid` | `auth_plcyidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policies | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policies | `OrganizationId` | `orgid` | `orgid` | `orgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policies | `ProjectId` | `prjid` | `prjid` | `pjtidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policies | `SubjectId` | `subid` | `subid` | `subidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policies | `Action` | `act` | `act` | `act` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_policies | `ResourceType` | `rsrc_typ` | `rsrc_typ` | `rsrc_typ` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_policies | `Effect` | `eff` | `eff` | `eff` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_policies | `Priority` | `pri` | `pri` | `pri` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_policies | `IsEnabled` | `ena_yn` | `ena_yn` | `ena` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policies | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit | `Id` | `auth_audid` | `auth_audid` | `auth_audidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit | `SubjectId` | `subid` | `subid` | `subidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit | `Action` | `act` | `act` | `act` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit | `ResourceType` | `rsrc_typ` | `rsrc_typ` | `rsrc_typ` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit | `Scope` | `scp` | `scp` | `scp` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit | `IsAllowed` | `allow_yn` | `allow_yn` | `alw_ynf` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit | `ReasonCode` | `rsn_cd` | `rsn_cd` | `rsn_cod` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit | `MatchedPolicyId` | `plcyid` | `plcyid` | `plcyidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit | `OccurredAtUtc` | `occ_utc` | `occ_utc` | `occ_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_policy_changes | `Id` | `auth_plcy_chgid` | `auth_plcy_chgid` | `auth_plcy_chgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policy_changes | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policy_changes | `PolicyId` | `plcyid` | `plcyid` | `plcyidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policy_changes | `ActorSubjectId` | `act_subid` | `act_subid` | `actr_subidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_policy_changes | `ChangeType` | `chg_typ` | `chg_typ` | `chg_typ` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_policy_changes | `BeforeJson` | `bef_json` | `bef_json` | `bef_json` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_policy_changes | `AfterJson` | `aft_json` | `aft_json` | `aft_json` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_policy_changes | `OccurredAtUtc` | `occ_utc` | `occ_utc` | `occ_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_retention | `Id` | `auth_aud_rtnid` | `auth_aud_rtnid` | `auth_aud_rtnidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_retention | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_retention | `RetentionDays` | `rtn_day` | `rtn_day` | `rtn_day` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_retention | `IsLegalHold` | `legal_hold_yn` | `legal_hold_yn` | `lgl_hld_ynf` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_retention | `LegalHoldReason` | `legal_hold_rsn` | `legal_hold_rsn` | `lgl_hld_rsn` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_retention | `UpdatedBy` | `upd_subid` | `upd_subid` | `upd_actr_idr` | — | 의미 확인 후 승인 |
| edan-tensor-platform.db | platform_authorization_audit_retention | `UpdatedAtUtc` | `upd_utc` | `upd_utc` | `upd_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_retention | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_retention_changes | `Id` | `auth_aud_rtn_chgid` | `auth_aud_rtn_chgid` | `auth_aud_rtn_chgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_retention_changes | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_retention_changes | `PolicyId` | `plcyid` | `plcyid` | `plcyidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_retention_changes | `ActorSubjectId` | `act_subid` | `act_subid` | `actr_subidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_retention_changes | `ChangeType` | `chg_typ` | `chg_typ` | `chg_typ` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_retention_changes | `BeforeJson` | `bef_json` | `bef_json` | `bef_json` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_retention_changes | `AfterJson` | `aft_json` | `aft_json` | `aft_json` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_retention_changes | `OccurredAtUtc` | `occ_utc` | `occ_utc` | `occ_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `Id` | `auth_aud_arc_jobid` | `auth_aud_arc_jobid` | `auth_aud_arc_jobidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `FromUtc` | `from_utc` | `from_utc` | `strt_utc` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `ToUtc` | `to_utc` | `to_utc` | `end_utc` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `RequestedBy` | `req_subid` | `req_subid` | `req_actr_idr` | — | 의미 확인 후 승인 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `Status` | `auth_aud_arc_jobst` | `auth_aud_arc_jobst` | `auth_aud_arc_jobsta` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `ArchiveId` | `arcid` | `arcid` | `arcidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `RecordCount` | `rec_cnt` | `rec_cnt` | `rec_cnt` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `Sha256` | `sha256` | `sha256` | `sha256` | — | 외부 표준 예외 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `SizeBytes` | `size_byt` | `size_byt` | `siz_byt` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `CompletedAtUtc` | `cmp_utc` | `cmp_utc` | `cmp_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `FailureReason` | `fail_rsn` | `fail_rsn` | `fail_rsn` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `AttemptCount` | `attm_cnt` | `attm_cnt` | `attm_cnt` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_jobs | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_attempts | `Id` | `auth_aud_arc_attmid` | `auth_aud_arc_attmid` | `auth_aud_arc_attmidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_attempts | `JobId` | `jobid` | `jobid` | `jobidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_attempts | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_attempts | `AttemptNumber` | `attm_num` | `attm_num` | `attm_num` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_attempts | `Operation` | `op` | `op` | `opr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_attempts | `StartedAtUtc` | `strt_utc` | `strt_utc` | `strt_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_attempts | `FinishedAtUtc` | `fin_utc` | `fin_utc` | `fin_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_attempts | `IsSuccessful` | `succ_yn` | `succ_yn` | `succ_ynf` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_attempts | `ResultCode` | `result_code` | `result_code` | `rslt_cod` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_schedules | `Id` | `auth_aud_arc_schdid` | `auth_aud_arc_schdid` | `auth_aud_arc_schdidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_schedules | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_schedules | `IntervalHours` | `int_hr` | `int_hr` | `intv_hou` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_schedules | `IsEnabled` | `ena_yn` | `ena_yn` | `ena` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_schedules | `NextRunAtUtc` | `nxt_utc` | `nxt_utc` | `nxt_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_authorization_audit_archive_schedules | `LastRunAtUtc` | `last_utc` | `last_utc` | `lst_utc` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_schedules | `LeaseId` | `leaseid` | `leaseid` | `lseidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_schedules | `LeaseExpiresAtUtc` | `lease_exp_utc` | `lease_exp_utc` | `lse_exr_utc` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_authorization_audit_archive_schedules | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policies | `Id` | `str_qta_plcyid` | `str_qta_plcyid` | `str_qta_plcyidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policies | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policies | `Scope` | `scp` | `scp` | `scp` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policies | `TargetId` | `tgtid` | `tgtid` | `tgtidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policies | `TenantLimitBytes` | `tnt_lmt_byt` | `tnt_lmt_byt` | `tnt_lmt_byt` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policies | `OrganizationLimitBytes` | `org_lmt_byt` | `org_lmt_byt` | `org_lmt_byt` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policies | `ProjectLimitBytes` | `prj_lmt_byt` | `prj_lmt_byt` | `pjt_lmt_byt` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policies | `DataSetLimitBytes` | `dset_lmt_byt` | `dset_lmt_byt` | `dset_lmt_byt` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policies | `WarningThresholdPercent` | `warn_thr_pct` | `warn_thr_pct` | `warn_thr_pct` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policies | `PlanCode` | `plan_cd` | `plan_cd` | `pln_cod` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policies | `IsEnabled` | `ena_yn` | `ena_yn` | `ena` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policies | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policies | `UpdatedBy` | `upd_subid` | `upd_subid` | `upd_actr_idr` | — | 의미 확인 후 승인 |
| edan-tensor-platform.db | platform_storage_quota_policies | `UpdatedAtUtc` | `upd_utc` | `upd_utc` | `upd_utc` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policy_changes | `Id` | `str_qta_plcy_chgid` | `str_qta_plcy_chgid` | `str_qta_plcy_chgidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policy_changes | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policy_changes | `PolicyId` | `plcyid` | `plcyid` | `plcyidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policy_changes | `ActorSubjectId` | `act_subid` | `act_subid` | `actr_subidr` | — | 표준 전환 권장 |
| edan-tensor-platform.db | platform_storage_quota_policy_changes | `ChangeType` | `chg_typ` | `chg_typ` | `chg_typ` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policy_changes | `BeforeJson` | `bef_json` | `bef_json` | `bef_json` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policy_changes | `AfterJson` | `aft_json` | `aft_json` | `aft_json` | — | 기존 후보 유지 |
| edan-tensor-platform.db | platform_storage_quota_policy_changes | `OccurredAtUtc` | `occ_utc` | `occ_utc` | `occ_utc` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | datasets | `Id` | `dsetid` | `dsetid` | `dsetidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | datasets | `Name` | `dsetnm` | `dsetnm` | `dsetnam` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | datasets | `StorageAdapter` | `str_adp` | `str_adp` | `str_adp` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | datasets | `Status` | `dsetst` | `dsetst` | `dsetsta` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | datasets | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | datasets | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | datasets | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | datasets | `OrganizationId` | `orgid` | `orgid` | `orgidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | datasets | `ProjectId` | `prjid` | `prjid` | `pjtidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | datasets | `StorageReference` | `str_ref` | `str_ref` | `str_ref` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | datasets | `ContentFormat` | `ctt_fmt` | `ctt_fmt` | `ctt_fmt` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_schema_revisions | `DataSetId` | `dsetid` | `dsetid` | `dsetidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_schema_revisions | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_schema_revisions | `SchemaJson` | `schm_json` | `schm_json` | `schm_json` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_schema_revisions | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_storage_usage_snapshots | `Id` | `dset_str_usg_snpid` | `dset_str_usg_snpid` | `dset_str_usg_snpidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_storage_usage_snapshots | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_storage_usage_snapshots | `PeriodStartUtc` | `period_strt_utc` | `period_strt_utc` | `prd_strt_utc` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_storage_usage_snapshots | `CapturedAtUtc` | `cap_utc` | `cap_utc` | `cap_utc` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_storage_usage_snapshots | `UsedBytes` | `used_bytes` | `used_bytes` | `usq_byt` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_storage_usage_snapshots | `DataSetCount` | `data_set_count` | `data_set_count` | `dset_cnt` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `Id` | `dset_qta_thr_evtid` | `dset_qta_thr_evtid` | `dset_qta_thr_evtidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `PolicyId` | `plcyid` | `plcyid` | `plcyidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `Scope` | `scp` | `scp` | `scp` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `TargetId` | `tgtid` | `tgtid` | `tgtidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `EventType` | `event_type` | `event_type` | `evt_typ` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `UsedBytes` | `used_bytes` | `used_bytes` | `usq_byt` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `LimitBytes` | `limit_bytes` | `limit_bytes` | `lmt_byt` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `UtilizationPercent` | `utilization_percent` | `utilization_percent` | `util_pct` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `PolicyVersion` | `policy_version` | `policy_version` | `plcy_ver` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_quota_threshold_events | `OccurredAtUtc` | `occ_utc` | `occ_utc` | `occ_utc` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_profiles | `Id` | `dset_vld_prfid` | `dset_vld_prfid` | `dset_vld_prfidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_profiles | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_profiles | `Name` | `dset_vld_prfnm` | `dset_vld_prfnm` | `dset_vld_prfnam` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_profiles | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_profile_revisions | `ProfileId` | `prfid` | `prfid` | `prfidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_profile_revisions | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_profile_revisions | `Revision` | `revision` | `revision` | `rev` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_profile_revisions | `DefinitionJson` | `def_json` | `def_json` | `def_json` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_profile_revisions | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `Id` | `dset_vld_excid` | `dset_vld_excid` | `dset_vld_excidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `DataSetId` | `dsetid` | `dsetid` | `dsetidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `ExpectedDataSetVersion` | `exp_dset_ver` | `exp_dset_ver` | `exp_dset_ver` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `SchemaVersion` | `schm_ver` | `schm_ver` | `schm_ver` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `ProfileId` | `prfid` | `prfid` | `prfidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `ProfileRevision` | `prf_rev` | `prf_rev` | `prf_rev` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `Status` | `dset_vld_excst` | `dset_vld_excst` | `dset_vld_excsta` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `DetailReference` | `dtl_ref` | `dtl_ref` | `dtl_ref` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `ProcessedRecords` | `proc_rec_cnt` | `proc_rec_cnt` | `prc_rec_cnt` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `TotalRecords` | `tot_rec_cnt` | `tot_rec_cnt` | `tot_rec_cnt` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `ErrorCount` | `err_cnt` | `err_cnt` | `err_cnt` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `WarningCount` | `warn_cnt` | `warn_cnt` | `warn_cnt` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `DetailItemCount` | `dtl_item_cnt` | `dtl_item_cnt` | `dtl_itm_cnt` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `DetailTruncated` | `dtl_trunc_yn` | `dtl_trunc_yn` | `dtl_trn_ynf` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `RuleCountsJson` | `rule_cnt_json` | `rule_cnt_json` | `rul_cnt_json` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `FailureCode` | `fail_cd` | `fail_cd` | `fail_cod` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `FailureMessage` | `fail_msg` | `fail_msg` | `fail_msg` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `CompletedAtUtc` | `cmp_utc` | `cmp_utc` | `cmp_utc` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_executions | `FencingToken` | `fence_tkn` | `fence_tkn` | `fcn_tkn` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_executions | `IdempotencyKey` | `idem_key` | `idem_key` | `idem_key` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_details | `Id` | `dset_vld_dtlid` | `dset_vld_dtlid` | `dset_vld_dtlidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_details | `ExecutionId` | `excid` | `excid` | `execidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_details | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_details | `DetailReference` | `dtl_ref` | `dtl_ref` | `dtl_ref` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_details | `RowNumber` | `row_num` | `row_num` | `row_num` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_details | `FieldName` | `fldnm` | `fldnm` | `fldnam` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_details | `Code` | `dset_vld_dtlcd` | `dset_vld_dtlcd` | `dset_vld_dtlcod` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_details | `Severity` | `sev` | `sev` | `sev` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_details | `Message` | `msg` | `msg` | `msg` | — | 기존 후보 유지 |
| edan-tensor-dataset.db | dataset_validation_seen_keys | `Id` | `dset_vld_seen_keyid` | `dset_vld_seen_keyid` | `dset_vld_seen_keyidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_seen_keys | `ExecutionId` | `excid` | `excid` | `execidr` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_seen_keys | `RuleCode` | `rule_cd` | `rule_cd` | `rul_cod` | — | 표준 전환 권장 |
| edan-tensor-dataset.db | dataset_validation_seen_keys | `KeyHash` | `key_hash` | `key_hash` | `key_hsh` | — | 표준 전환 권장 |
| edan-tensor-dataset-content.db | DataSetContent | `StorageReference` | `str_ref` | `str_ref` | `str_ref` | — | 기존 후보 유지 |
| edan-tensor-dataset-content.db | DataSetContent | `DataSetId` | `dsetid` | `dsetid` | `dsetidr` | — | 표준 전환 권장 |
| edan-tensor-dataset-content.db | DataSetContent | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-tensor-dataset-content.db | DataSetContent | `OrganizationId` | `orgid` | `orgid` | `orgidr` | — | 표준 전환 권장 |
| edan-tensor-dataset-content.db | DataSetContent | `ProjectId` | `prjid` | `prjid` | `pjtidr` | — | 표준 전환 권장 |
| edan-tensor-dataset-content.db | DataSetContent | `Content` | `ctt` | `ctt` | `ctt` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `Id` | `wkr_tskid` | `wkr_tskid` | `wkr_tskidr` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `TaskType` | `tsktyp` | `tsktyp` | `tsk_typ` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `IdempotencyKey` | `idem_key` | `idem_key` | `idem_key` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `PayloadReference` | `pld_ref` | `pld_ref` | `pld_ref` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `Status` | `wkr_tskst` | `wkr_tskst` | `wkr_tsksta` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `AttemptCount` | `attm_cnt` | `attm_cnt` | `attm_cnt` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `MaxAttempts` | `max_attm_cnt` | `max_attm_cnt` | `max_attm` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `LeaseId` | `leaseid` | `leaseid` | `lseidr` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `WorkerId` | `wkrid` | `wkrid` | `wkridr` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `FencingToken` | `fence_tkn` | `fence_tkn` | `fcn_tkn` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `LeaseExpiresAtUtc` | `lease_exp_utc` | `lease_exp_utc` | `lse_exr_utc` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `Checkpoint` | `ckpt` | `ckpt` | `ckpt` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `CheckpointVersion` | `ckpt_ver` | `ckpt_ver` | `ckpt_ver` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `CompletedAtUtc` | `cmp_utc` | `cmp_utc` | `cmp_utc` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `NextAttemptAtUtc` | `nxt_attm_utc` | `nxt_attm_utc` | `nxt_attm_utc` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `LastErrorCode` | `last_err_cd` | `last_err_cd` | `lst_err_cod` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `ProgressPercent` | `prgrs_pct` | `prgrs_pct` | `prg_pct` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `ProgressStage` | `prgrs_stg` | `prgrs_stg` | `prg_stg` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `ProgressMessage` | `prgrs_msg` | `prgrs_msg` | `prg_msg` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `ResultReference` | `rslt_ref` | `rslt_ref` | `rslt_ref` | — | 기존 후보 유지 |
| edan-worker.db | worker_tasks | `ReadyAtUtcUnixMilliseconds` | `ready_utc_ms` | `ready_utc_ms` | `rdy_uep_msec` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `LeaseExpiresAtUtcUnixMilliseconds` | `lease_exp_utc_ms` | `lease_exp_utc_ms` | `lse_exr_uep_msec` | — | 표준 전환 권장 |
| edan-worker.db | worker_tasks | `CreatedAtUtcUnixMilliseconds` | `crt_utc_ms` | `crt_utc_ms` | `crt_uep_msec` | — | 표준 전환 권장 |
| edan-worker.db | worker_task_replays | `Id` | `wkr_rplid` | `wkr_rplid` | `wkr_rplidr` | — | 표준 전환 권장 |
| edan-worker.db | worker_task_replays | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-worker.db | worker_task_replays | `OriginalTaskId` | `orig_tskid` | `orig_tskid` | `orig_tskidr` | — | 표준 전환 권장 |
| edan-worker.db | worker_task_replays | `ReplayTaskId` | `rpl_tskid` | `rpl_tskid` | `rpl_tskidr` | — | 표준 전환 권장 |
| edan-worker.db | worker_task_replays | `RequestedBy` | `req_subid` | `req_subid` | `req_actr_idr` | — | 의미 확인 후 승인 |
| edan-worker.db | worker_task_replays | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-worker.db | worker_task_replays | `ReplayRequestId` | `rpl_reqid` | `rpl_reqid` | `rpl_reqidr` | — | 표준 전환 권장 |
| edan-worker.db | worker_task_types | `TaskType` | `tsktyp` | `tsktyp` | `tsk_typ` | — | 표준 전환 권장 |
| edan-worker.db | worker_task_types | `Description` | `desc` | `desc` | `desc` | — | 기존 후보 유지 |
| edan-worker.db | worker_task_types | `IsEnabled` | `ena_yn` | `ena_yn` | `ena` | — | 표준 전환 권장 |
| edan-worker.db | worker_task_types | `UpdatedAtUtc` | `upd_utc` | `upd_utc` | `upd_utc` | — | 기존 후보 유지 |
| edan-worker.db | worker_task_types | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |
| edan-worker.db | external_deliveries | `Id` | `wkr_dlvid` | `wkr_dlvid` | `wkr_dlvidr` | — | 표준 전환 권장 |
| edan-worker.db | external_deliveries | `TenantId` | `tntid` | `tntid` | `tntidr` | — | 표준 전환 권장 |
| edan-worker.db | external_deliveries | `TargetUri` | `tgt_uri` | `tgt_uri` | `tgt_uri` | — | 기존 후보 유지 |
| edan-worker.db | external_deliveries | `Payload` | `pld` | `pld` | `pld` | — | 기존 후보 유지 |
| edan-worker.db | external_deliveries | `IdempotencyKey` | `idem_key` | `idem_key` | `idem_key` | — | 기존 후보 유지 |
| edan-worker.db | external_deliveries | `Status` | `wkr_dlvst` | `wkr_dlvst` | `wkr_dlvsta` | — | 표준 전환 권장 |
| edan-worker.db | external_deliveries | `AttemptCount` | `attm_cnt` | `attm_cnt` | `attm_cnt` | — | 기존 후보 유지 |
| edan-worker.db | external_deliveries | `MaxAttempts` | `max_attm_cnt` | `max_attm_cnt` | `max_attm` | — | 표준 전환 권장 |
| edan-worker.db | external_deliveries | `LastErrorCode` | `last_err_cd` | `last_err_cd` | `lst_err_cod` | — | 표준 전환 권장 |
| edan-worker.db | external_deliveries | `CreatedAtUtc` | `crt_utc` | `crt_utc` | `crt_utc` | — | 기존 후보 유지 |
| edan-worker.db | external_deliveries | `CompletedAtUtc` | `cmp_utc` | `cmp_utc` | `cmp_utc` | — | 기존 후보 유지 |
| edan-worker.db | external_deliveries | `Version` | `ver` | `ver` | `ver` | — | 기존 후보 유지 |

## 7. 동일 테이블 내 이름 충돌 검사

- 검사 기준: 데이터베이스 + 테이블 + 신규 표준 컬럼명
- 결과: 동일 테이블 내 신규 컬럼명 중복 0건
- 서로 다른 테이블에서 같은 FK 컬럼명이 반복되는 것은 정상이다.

## 8. 구현 전 필수 확인

1. 실제 `.db` 파일의 `sqlite_master`와 `PRAGMA table_info`로 물리 컬럼명을 수집한다.
2. 각 DbContext의 EF Core `HasColumnName` 및 shadow property를 확인한다.
3. 배포된 Migration·SQL·인덱스·FK·트리거·뷰·외부 쿼리 사용처를 조사한다.
4. `UpdatedBy`, `RequestedBy` 저장값의 실제 참조 대상을 확인한다.
5. `sha256`은 알고리즘 고정 표기 예외로 승인한다.
6. 기존 DB를 직접 Rename하지 말고 호환 기간·데이터 보존·롤백을 포함한 공급자별 Migration을 설계한다.
7. SQLite를 먼저 검증하고 Oracle·SQL Server·PostgreSQL의 예약어·식별자 길이를 교차 확인한다.

## 9. 검증 범위 제한

이번 단계에서 확보한 자료는 287행의 컬럼 인벤토리다. 실제 SQLite 바이너리 DB 4개는 검사하지 않았다. 따라서 본 결과는 287개 인벤토리 항목의 논리적·명명 규칙 재매핑이며, 실제 DB 물리 컬럼과의 일치, 데이터 영향, EF Core 모델 및 Migration 호환은 아직 검증되지 않았다. 이 검증이 끝나기 전에는 `완료` 또는 `배포 가능`으로 판정하지 않는다.

- CSV 상세표: `database-column-remapping-287.csv`