# EDAN Tensor 데이터베이스 테이블명·컬럼명 표준 명명 규칙

- 문서 상태: 표준안
- 기준 저장소: `namanvarava/Edan.Tensor` / `main`
- 작성 기준일: 2026-11-15
- 적용 범위: EDAN Tensor 전체 데이터베이스의 신규 객체 및 표준화 대상 객체
- 객체명: 영문 소문자 / 설명과 주석: 한국어

## 1. 목적과 적용 원칙

이 문서는 EDAN Tensor 전체 DB에서 테이블명·컬럼명·약어를 일관되게 작성하기 위한 단일 기준이다.

1. 신규 DB 객체는 본 규칙과 전자사전의 표준 약어를 적용한다.
2. 같은 업무 개념은 Platform·DataSet·Worker 등 모듈이 달라도 같은 약어를 사용한다.
3. SQLite·Oracle·SQL Server·PostgreSQL에서 객체명을 다르게 만들지 않는다.
4. 기존 스키마는 이 문서 추가만으로 일괄 변경하지 않는다.
5. 기존 이름 변경은 EF Core Migration, 데이터 보존, API·SQL·운영 도구 호환성을 확인한 별도 변경으로 수행한다.
6. 문서의 표준과 현재 구현이 다르면 준수한다고 간주하지 않고 예외 또는 마이그레이션 대상으로 기록한다.

## 2. GitHub 기존 구현과 비교한 결과

확인한 위치: `database/sqlite/README.md`, `docs/README.md`, Platform·DataSet·Worker의 초기 EF Core Migration, `docs/operations/platform-database-local-setup.md`.

| 확인 위치 | 현재 저장소의 사례 | 비교 및 표준 반영 |
| --- | --- | --- |
| SQLite 안내 | Platform·DataSet·Worker가 전용 연결과 DbContext, EF Core Migration 사용 | DB별 독립 경계는 유지하고 명명 규칙은 공통 적용 |
| Platform 테이블 | `platform_tenants`, `platform_organizations`, `platform_projects`, `platform_users`, `platform_resources`, `platform_policies` | 도메인 접두어 + 복수형 테이블명과 일치 |
| DataSet 테이블 | `datasets`, `dataset_schema_revisions` | 복수형은 일치하나 `datasets`는 도메인 접두어가 없는 기존 예외 |
| Worker 테이블 | `worker_tasks` | 도메인 접두어 + 복수형과 일치 |
| Platform 컬럼 | `Id`, `TenantId`, `CreatedAtUtc`와 `tenant_id` 등이 혼재 | PascalCase·snake_case 혼재. 신규 컬럼부터 소문자 snake_case 적용 |
| DataSet·Worker 컬럼 | `DataSetId`, `TenantId`, `CreatedAtUtc`, `AttemptCount` 등 | CLR 속성형 이름이 물리 컬럼명으로 반영된 기존 사례. 신규 컬럼은 명시 매핑 |
| DB 객체 | `PK_`, `FK_`, `IX_`, `AK_`, `CK_` | EF Core 기존 생성 이름은 보존하고 신규 객체는 EDAN 접두어 적용 |

**결론:** 저장소에는 전체 DB 명명 규칙을 한곳에서 정의한 독립 문서가 확인되지 않았다. 기존 테이블명은 대체로 소문자 snake_case·복수형이나, 컬럼명은 대소문자와 구분 방식이 혼재한다. 본 문서를 공통 기준 문서로 추가하며 기존 Migration 파일과 배포된 DB를 소급 수정하지 않는다.

## 3. 공통 표기 규칙

| 항목 | 표준 | 올바른 예 | 지양 예 |
| --- | --- | --- | --- |
| 문자 | 영문 소문자 | `tenant` | `Tenant`, `TENANT` |
| 단어 구분 | 밑줄(`_`) | `tenant_members` | `tenantMembers`, `tenant-members` |
| 공백 | 사용 금지 | `created_utc` | `created utc` |
| 언어 | 영문 | `authorization_policy` | 한글·한영 혼용 |
| 약어 | 전자사전 등록 약어만 | `tntid` | 임의 축약 `tnid` |
| 의미 | 업무 개념 중심 | `storage_quota_policies` | `data_info`, `table1` |
| 예약어 | SQL 예약어 회피 | `order_items` | `order` |
| 자료형 | 이름에 자료형 미포함 | `payload` | `payload_json_text` |

약어 토큰 자체의 표준 표기는 유지하고, 복합 개념의 토큰 경계만 밑줄로 구분한다.

## 4. 테이블명 규칙

### 4.1 기본 형식

```text
{domain}_{business_entity}_{optional_sub_entity}
```

- 소문자 snake_case, 복수형 명사를 사용한다.
- 도메인 접두어는 `platform_`, `dataset_`, `worker_`, `commercial_`, `licensing_` 등 모듈 경계를 표현한다.
- 업무 엔터티는 가능한 한 완전한 영문 단어로 작성한다. 테이블명에는 임의 약어를 사용하지 않는다.
- 불필요한 `tbl_`, `tb_`, `data_`, `info_` 접두어·접미어를 붙이지 않는다.

### 4.2 예시

| 업무 영역 | 권장 테이블명 | 설명 |
| --- | --- | --- |
| Platform | `platform_tenants` | 테넌트 |
| Platform | `platform_organizations` | 조직 |
| Platform | `platform_authorization_policies` | 권한 정책 |
| Platform | `platform_authorization_policy_changes` | 권한 정책 변경 이력 |
| DataSet | `dataset_definitions` | 데이터셋 정의 |
| DataSet | `dataset_schema_revisions` | 데이터셋 스키마 개정 |
| Worker | `worker_tasks` | 작업 |
| Worker | `worker_task_replay_histories` | 작업 재처리 이력 |

예시는 신규 설계 기준이다. 기존 `datasets`를 즉시 `dataset_definitions`로 변경하라는 의미가 아니다.

### 4.3 관계·이력 테이블

- 다대다 관계는 관계 엔터티를 명확히 표현한다. 예: `platform_user_roles`.
- 변경 이력은 `_changes` 또는 `_histories` 중 실제 저장 의미에 맞는 하나를 선택한다.
- 감사 이벤트는 `_audit_events`, 보존 정책은 `_retention_policies`, 아카이브 작업은 `_archive_jobs`처럼 업무 의미를 드러낸다.
- 임시·작업 테이블은 실제 수명주기가 있는 경우에만 목적이 명확한 접미어를 사용한다.

## 5. 컬럼명 규칙

### 5.1 기본 형식

```text
{entity_abbreviation}{attribute_abbreviation}
{entity_abbreviation}_{attribute_abbreviation}
```

- 소문자만 사용하고 약어는 전자사전의 표준 약어를 사용한다.
- 짧은 단일 개념은 불필요한 밑줄 없이 결합한다: `tntid`, `orgcd`, `usrnm`.
- 복합 개념은 토큰 경계를 밑줄로 구분한다: `auth_plcyid`, `warn_thr_pct`, `actr_subid`.
- FK는 참조 대상 PK 컬럼명과 완전히 동일하게 사용한다.
- 의미가 불분명한 약어보다 명확한 복합 약어를 우선한다.

### 5.2 컬럼 예시

| 영문 의미 | 권장 컬럼명 | 설명 |
| --- | --- | --- |
| Tenant ID | `tntid` | 테넌트 식별자 |
| Organization ID | `orgid` | 조직 식별자 |
| Project ID | `pjtid` | 프로젝트 식별자 |
| User ID | `usrid` | 사용자 식별자 |
| DataSet ID | `dsetid` | 데이터셋 식별자 |
| Policy ID | `plcyid` | 정책 식별자 |
| Authorization Policy Change ID | `auth_plcy_chgid` | 권한 정책 변경 식별자 |
| Actor Subject ID | `actr_subid` | 행위 주체 식별자 |
| Change Type | `chg_typ` | 변경 유형 |
| Result Code | `rslt_cd` | 결과 코드 |
| Warning Threshold Percent | `warn_thr_pct` | 경고 임계 비율 |
| Progress Stage | `prgrs_stg` | 진행 단계 |
| Target URI | `tgt_uri` | 대상 URI |
| Created UTC | `crt_utc` | 생성 시각(UTC) |
| Updated UTC | `upd_utc` | 수정 시각(UTC) |
| Enabled Yes/No | `ena_yn` | 활성 여부 |

### 5.3 PK·FK

- PK는 해당 엔터티의 식별자 약어를 사용한다: `tntid`, `orgid`, `auth_plcy_chgid`.
- FK는 참조 대상 PK와 동일한 이름을 사용한다. 참조 대상 PK가 `tntid`이면 하위 테이블의 테넌트 FK도 `tntid`이다.
- 컬럼명에 `_pk`, `_fk`를 추가하지 않는다.
- 복합 FK는 구성 컬럼 각각의 표준명을 유지하고 제약조건 이름으로 관계를 식별한다.

## 6. 표준 속성 약어

| 약어 | 영문 의미 | 한글 의미 | 사용 예 |
| --- | --- | --- | --- |
| `id` | Identifier | 식별자 | `tntid`, `usrid` |
| `cd` | Code | 코드 | `pln_cd`, `rslt_cd` |
| `nm` | Name | 이름 | `usrnm`, `orgnm` |
| `typ` | Type | 유형 | `chg_typ` |
| `sta` | Status | 상태 | `task_sta` |
| `yn` | Yes/No | 여부 | `ena_yn`, `del_yn` |
| `utc` | UTC Timestamp | UTC 시각 | `crt_utc`, `occ_utc` |
| `date` | Date | 날짜 | `exp_date` |
| `cnt` | Count | 건수 | `dset_cnt`, `dtl_cnt` |
| `num` | Number | 번호·수치 | `sort_num` |
| `byt` | Byte | 바이트 수 | `size_byt` |
| `pct` | Percent | 백분율 | `util_pct` |
| `msg` | Message | 메시지 | `err_msg` |
| `json` | JSON | JSON 데이터 | `bef_json`, `aft_json` |
| `uri` | URI | URI | `tgt_uri` |
| `ref` | Reference | 참조 | `str_ref` |
| `ver` | Version | 버전 | `ver` |
| `stg` | Stage | 단계 | `prgrs_stg` |
| `hr` | Hour | 시간(시) | `intv_hr` |

미등록 약어는 사용 전에 전자사전에 추가·검토한다. 같은 영문 개념에 복수 약어를 허용하지 않는다.

## 7. 감사 컬럼 표준

| 컬럼명 | 영문 의미 | 설명 |
| --- | --- | --- |
| `crt_by` | Created By | 생성 주체 식별자 |
| `crt_utc` | Created At UTC | 생성 UTC 시각 |
| `upd_by` | Updated By | 최종 수정 주체 식별자 |
| `upd_utc` | Updated At UTC | 최종 수정 UTC 시각 |
| `del_yn` | Deleted Yes/No | 논리 삭제 여부 |
| `del_by` | Deleted By | 삭제 주체 식별자 |
| `del_utc` | Deleted At UTC | 삭제 UTC 시각 |

- `_utc`는 UTC 시간대가 보장되는 시각에만 사용한다.
- 날짜만 저장하는 값은 `_date`를 사용한다. 시간대가 불명확한 `_dt`와 `_utc`를 혼용하지 않는다.
- 주체 값이 사용자명인지 내부 식별자인지 데이터 사전에 명시하고, 가능하면 내부 주체 식별자를 저장한다.
- 논리 삭제를 사용하지 않는 테이블에 `del_yn`을 일괄 추가하지 않는다.

## 8. 약어의 의미 충돌 방지

| 개념 A | 약어 | 개념 B | 약어 | 구분 |
| --- | --- | --- | --- | --- |
| Actor | `actr` | Action | `act` | 행위 주체 / 수행 행위 |
| Expected | `exp` | Expiration | `expir` | 기대값 / 만료 |
| Identity | `idn` | Identifier | `id` | 정체성 / 식별자 |
| Policy | `plcy` | Rule | `rule` | 정책 / 개별 규칙 |
| Job | `job` | Task | `tsk` | 실행 단위 / 작업 단위 |
| Operation | `op` | Task | `tsk` | 연산 / 실행 작업 |
| Row | `row` | Record | `rec` | 행 위치 / 데이터 레코드 |
| Hash | `hash` | SHA-256 | `sha` | 일반 해시 / 특정 알고리즘 |
| Enabled | `ena` | Ready | `rdy` | 활성화 / 실행 준비 |
| UTC | `utc` | Unix | `unix` | 시간 표준 / Epoch 표현 |

다음 후보는 실제 컬럼 의미와 사용처가 확정되기 전까지 표준 약어로 확정하지 않는다.

- `used`: 사용량·사용 여부·사용 시각 등 의미가 불명확하다.
- `unix`: Unix Epoch 저장 방식이 필요한 컬럼에만 제한적으로 사용한다.
- `seen`: 조회·관찰·수신 등 실제 업무 의미를 먼저 확정한다.

## 9. 인덱스·제약조건 이름

| 객체 | 형식 | 예시 |
| --- | --- | --- |
| 기본 키 | `pk_{table}` | `pk_platform_tenants` |
| 외래 키 | `fk_{table}_{referenced_table}` | `fk_platform_projects_platform_tenants` |
| 고유 제약 | `uq_{table}_{columns}` | `uq_platform_tenants_tntcd` |
| 체크 제약 | `ck_{table}_{meaning}` | `ck_platform_resources_project_requires_org` |
| 일반 인덱스 | `ix_{table}_{columns}` | `ix_worker_tasks_tntid_task_sta` |
| 고유 인덱스 | `ux_{table}_{columns}` | `ux_platform_tenants_tntcd` |

- 객체명은 소문자 snake_case로 작성한다.
- 여러 컬럼은 테이블 내 컬럼 순서와 동일하게 나열한다.
- 기존 EF Core Migration이 생성한 `PK_`, `FK_`, `IX_`, `AK_`, `CK_` 이름은 호환성을 위해 유지한다.

## 10. EF Core 매핑 원칙

CLR 속성명과 DB 물리 컬럼명은 별개다. 신규 모델은 물리 이름을 명시적으로 매핑한다.

```csharp
builder.ToTable("platform_tenants");

builder.Property(x => x.Id)
    .HasColumnName("tntid");

builder.Property(x => x.Code)
    .HasColumnName("tntcd");

builder.Property(x => x.CreatedAtUtc)
    .HasColumnName("crt_utc");
```

- C# 속성명은 .NET 규칙(PascalCase)을 유지한다.
- DB 테이블·컬럼명은 본 문서의 소문자 snake_case와 약어 규칙을 적용한다.
- Migration 생성 전 테이블·컬럼·인덱스·제약조건 이름을 검토한다.
- 모델 설정·Migration·Model Snapshot의 물리 이름이 일치해야 한다.
- 공급자별 자동 명명 결과에 의존하지 말고 명시적 매핑을 사용한다.

## 11. DB 공급자 간 호환성

- 테이블·컬럼명은 지원 DB에서 동일하게 유지한다.
- Oracle·SQL Server·PostgreSQL·SQLite의 예약어와 식별자 길이 제한을 확인한다.
- 호환성을 위해 객체명은 가능한 한 30자 이내를 우선 권장한다. 초과 시 지원 DB 버전의 실제 제한을 검증하고 예외를 기록한다.
- 대소문자 구분·인용 식별자·예약어 처리 차이에 의존하지 않는다.
- 자료형은 공급자별로 매핑하되 객체명은 공급자별로 바꾸지 않는다.

## 12. 금지 및 지양 규칙

| 구분 | 지양 사례 | 권장 |
| --- | --- | --- |
| 대소문자 혼용 | `TenantId`, `TENANT_ID` | `tntid` |
| 임의 축약 | `tnid`, `usrnm1` | 전자사전 표준 약어 |
| 의미 중복 | `user_user_id` | `usrid` |
| 자료형 표현 | `amount_decimal`, `payload_text` | 업무 의미 중심 이름 |
| 불명확한 이름 | `data`, `info`, `value1` | 구체적인 업무 용어 |
| 불필요한 접두어 | `tbl_platform_tenants` | `platform_tenants` |
| DB별 이름 | SQLite `tenant_id`, Oracle `TNT_ID` | 모든 DB에서 같은 이름 |
| 예약어 | `order`, `group`, `user` 단독 사용 | 업무 의미를 포함한 이름 |

## 13. 기존 스키마 정비 절차

1. Platform·DataSet·Worker 및 추가 DB의 실제 테이블·컬럼 목록을 수집한다.
2. 전자사전과 대조해 미등록 약어·중복 약어·의미 충돌·표기 위반을 분류한다.
3. 기존 이름·권장 이름·변경 사유·영향 객체·호환성 위험을 정리한다.
4. API 계약, EF Core 모델, Migration, SQL, 테스트, 운영 스크립트 및 데이터 이관 영향을 분석한다.
5. 승인된 변경만 별도 Migration으로 수행한다.
6. 데이터 보존·롤백·백업·복구 및 호환성 테스트를 수행한다.
7. 적용 결과와 예외를 전자사전 및 검증 기록에 반영한다.

기존 DB 이름을 직접 수정하거나 과거 Migration 파일을 소급 변경하지 않는다. 배포된 Migration은 불변 이력으로 취급하고, 이름 변경은 후속 Migration으로 수행한다.

## 14. 신규 객체 검토 체크리스트

- [ ] 테이블명이 소문자 snake_case와 복수형인가?
- [ ] 테이블명이 도메인 접두어와 업무 엔터티로 구성되었는가?
- [ ] 컬럼명이 전자사전의 표준 약어만 사용하는가?
- [ ] 같은 개념에 같은 약어를 사용하는가?
- [ ] PK·FK 식별자 명칭이 참조 관계에 맞게 일치하는가?
- [ ] 감사 컬럼의 의미와 UTC 기준이 명확한가?
- [ ] 인덱스·제약조건 이름이 표준 형식인가?
- [ ] 예약어 및 공급자별 식별자 길이 제한을 확인했는가?
- [ ] EF Core 매핑·Migration·Model Snapshot이 동일한 물리 이름을 사용하는가?
- [ ] API·SQL·테스트·운영 데이터와의 호환성을 확인했는가?
- [ ] 미등록 약어와 예외를 전자사전에 기록했는가?

## 15. 표준 변경 관리

- 본 문서를 EDAN Tensor 데이터베이스 명명 규칙의 기준 문서로 사용한다.
- 전자사전은 영문 풀네임·한글 의미·표준 약어·사용 예·의미 구분 규칙을 함께 관리한다.
- 약어 추가·변경·폐기는 기존 컬럼과의 충돌을 확인한 뒤 승인한다.
- 표준 변경 시 본 문서와 전자사전을 함께 갱신한다.
- 문서의 표준과 실제 구현이 다르면 차이를 기록하고 예외 또는 Migration 대상으로 관리한다.


## 16. 전자사전 정합성 검증 상태 (2026-11-15)

2026-11-15 기준 기존 전자사전과 DB 컬럼 검토표를 대조한 결과, 본 문서의 일부 예시·약어는 전자사전에 아직 등록되지 않았거나 기존 항목과 표기가 다릅니다. 따라서 **전자사전 정합성 검증이 끝나기 전까지 미등록 약어를 승인된 표준으로 간주하지 않습니다.**

- `Status`의 표준 약어는 전자사전의 `sta`입니다. `st`를 사용하지 않습니다.
- `Project`의 표준 약어는 `pjt`입니다. `prj`를 사용하지 않습니다.
- `Interval`의 표준 약어는 `intv`입니다. `int_hr`가 아니라 `intv_hr`를 사용합니다.
- `Result Code`는 `rslt_cd`로 구성합니다. `result_code`는 전자사전 조합 규칙에 맞지 않습니다.
- `Actor`와 `By`는 현재 전자사전에 별도 항목이 없습니다. `actr_subid`, `crt_by`, `upd_by`, `del_by`는 전자사전 등록·승인 전까지 제안 명칭입니다.
- `Stage`, `Job`, `Identity`, `Expiration`, `Rule`, `Row`, 일반 `Hash`, `Ready`, `Unix`, `Token`도 전자사전 항목의 등록 상태를 확인한 뒤 사용합니다.
- `Role`은 전자사전에 `role`과 `rol`이 함께 존재합니다. 하나의 표준 약어로 통합하는 변경을 먼저 승인해야 합니다.
- `Created DateTime(crtdt)`, `Creator(crtmer)`, `Updater(updmer)`, `Use Yes/No(useyn)`, `Delete Yes/No(delyn)`, `Sort Number(sortnum)`는 구형 결합 표기입니다. 신규 표준은 `crt_utc` 또는 `crt_dt`, `crt_by`, `upd_utc` 또는 `upd_dt`, `upd_by`, `use_yn`, `del_yn`, `sort_num`으로 정리하되 전자사전 개별 항목을 함께 갱신해야 합니다.

상세 결과와 기존 구현의 위반 사례는 [2026-11-15 DB 명명 규칙·전자사전 정합성 감사](validation/2026-11-15-database-naming-compliance-audit.md)를 기준으로 관리합니다.
