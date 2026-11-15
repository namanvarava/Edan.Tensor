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


## 16. 대표 용어·관계 용어 통합 원칙

### 16.1 전역 표준 사전의 구조

EDAN의 전자사전은 단어 목록이 아니라 **전역 업무 개념 사전(Global Business Concept Dictionary)** 으로 관리한다. 업무 모듈이나 DB 공급자에 따라 같은 개념에 다른 약어를 부여하지 않는다.

대표 용어 한 건에만 대표 약어를 부여하고, 동일 개념을 가리키는 표현은 관계 용어로 연결한다. 관계 용어에는 물리 명명용 약어를 별도로 발급하지 않는다.

| 필드 | 필수 | 설명 |
| --- | --- | --- |
| 대표 용어 ID | 예 | 사전 내부에서 영구적으로 유지하는 개념 식별자 |
| 업무 모듈 | 예 | 공통, ERP, MES, MOM, QMS, PLM, 설비, 데이터 수집 등 |
| 개념 분류 | 예 | 엔터티, 행위, 상태, 수량, 시간, 참조, 품질 등 |
| 대표 영문명 | 예 | 물리 명명 시 기준으로 사용하는 단일 명칭 |
| 대표 한글명 | 예 | 대표 업무 용어 |
| 대표 약어 | 예 | 전역 유일 약어 |
| 관계 용어 | 해당 시 | 동의어·유사어·업계 표현·구 용어·약칭 |
| 관계 유형 | 해당 시 | 동의어, 상위어, 하위어, 문맥 유사어, 혼동 주의어 |
| 개념 정의 | 예 | 포함하는 의미와 업무 경계 |
| 제외 의미 | 예 | 혼동하면 안 되는 인접 개념 |
| 사용 예 | 예 | 대표 약어를 조합한 물리 컬럼명 |
| 상태 | 예 | 표준, 검토, 폐기, 레거시 호환 |
| 적용 버전 | 예 | 표준 변경 이력과 적용 시점 |

### 16.2 관계 용어 통합 판정

- 완전 동의어는 하나의 대표 용어에 연결한다. 대표 용어만 약어를 가진다.
- 표기만 다른 표현(띄어쓰기, 단복수, 영미식 표기, 약칭)은 대표 용어의 별칭으로 관리한다.
- 업무상 유사하지만 책임 주체·방향·생명주기·회계 의미가 다른 개념은 통합하지 않는다.
- 상위어와 하위어는 계층 관계로 연결하되 약어를 공유하지 않는다.
- 문맥에 따라 의미가 달라지는 단어는 관계 용어에 혼동 주의 표시를 두고, 물리 명명에는 반드시 구체적인 대표 용어를 선택한다.
- 업무 모듈은 용어의 소유·사용 범위를 나타낼 뿐, 공통 개념의 약어를 모듈별로 재발급하는 근거가 아니다.

### 16.3 약어 길이와 충돌 처리

1. 대표 약어는 영문 소문자 3자를 기본으로 한다.
2. 3자 후보가 이미 다른 대표 용어에 배정되어 있으면 4자 후보를 검토한다.
3. 4자 후보도 충돌하거나 의미가 불명확하면 자동 생성하지 않고 사전 검토 상태로 둔다.
4. 약어 중복은 업무 모듈을 넘어서 전역 기준으로 검사한다.
5. 관계 용어에는 별도 약어를 부여하지 않는다.
6. 기존 2자·4자 이상 약어는 즉시 삭제하지 않는다. 외부 표준, 기존 DB, 공개 API와의 호환 여부를 확인하여 표준 전환 또는 레거시 전용으로 분류한다.
7. 표준 약어는 한 번 확정하면 의미 변경 없이 유지한다. 변경이 필요하면 새 개념 버전과 레거시 매핑을 남긴다.
8. 기존 2자 약어를 사용하는 물리 컬럼은 마이그레이션 전까지 레거시 이름으로만 인정하며 신규 컬럼에 재사용하지 않는다.

### 16.4 컬럼명 조합

컬럼명은 대표 용어의 약어 토큰만 조합한다. 관계 용어, 임의 축약, CLR 속성명, 화면 표시 문구를 직접 물리 컬럼명에 사용하지 않는다.

- 엔터티 식별자: `{entity_abbr}{identifier_abbr}`
- 엔터티 속성: `{entity_abbr}{attribute_abbr}`
- 의미 단위가 다른 복합 속성: `{concept_abbr}_{attribute_abbr}`
- 두 개 이상의 독립 개념을 조합할 때만 밑줄로 토큰 경계를 표시한다.
- FK는 참조 대상 PK와 같은 대표 약어 조합을 사용한다.
- 모듈 접두어는 테이블 경계에 사용하며, 컬럼 약어에 모듈명을 중복 삽입하지 않는다.

예를 들어 프로젝트 식별자의 대표 약어가 확정되면 Platform·ERP·MES·QMS 어디서 참조하더라도 같은 컬럼 토큰을 사용한다. 단, 실제 프로젝트와 생산 프로젝트 등 서로 다른 개념이면 별도 대표 개념으로 구분한다.

### 16.5 표준 용어와 레거시 용어의 분리

- 표준: 신규 설계에서 사용할 수 있는 대표 용어와 약어
- 검토: 정의·충돌·업무 범위가 확정되지 않아 신규 사용을 금지하는 후보
- 레거시 호환: 기존 DB/API/외부 연계 보존을 위해서만 인식하는 구 약어
- 폐기: 신규 사용 금지, 기존 사용처의 대체 계획을 기록하는 용어

기존 2자 약어를 3자로 바꾸는 경우에도 기존 DB 컬럼을 자동으로 변경하지 않는다. 후속 Migration, API 호환, 데이터 이관, 인덱스 및 외부 SQL 영향 분석을 거친다.

### 16.6 시간·감사 용어의 의미 분리

- Date는 날짜만, Timestamp는 시각을 포함하는 시점, UTC는 시간대 기준, Unix Epoch는 기준 시점으로부터의 수치 표현이다.
- Created/Updated/Deleted는 발생 행위·시점의 접두 개념이다.
- Creator/Updater는 별도 명사 약어를 만들지 않고 행위 주체 개념과 조합한다. 신규 감사 컬럼은 `crt_{actor_id}`, `upd_{actor_id}`, `del_{actor_id}` 형태의 표준 토큰 조합을 우선 검토한다.
- Used, Usage, Utilization은 각각 소비·사용 사실, 사용 활동/실적, 가용량 대비 점유율로 구분한다.
- Expected와 Expiration, Action과 Actor, Policy와 Rule, Row와 Record, Hash와 SHA-256은 서로 대체할 수 없다.

## 17. 업무 모듈 확장과 사전 소유권

EDAN의 업무 사전은 다음 모듈을 기준으로 확장한다. 공통 개념은 공통 사전에서 한 번만 정의하고 각 모듈은 사용 관계만 등록한다.

| 모듈 코드 | 모듈 | 주요 업무 범위 |
| --- | --- | --- |
| COM | 공통 업무 | 조직, 사업장, 공장, 부서, 사용자, 거래처, 공통코드, 통화, 단위, 달력 |
| ERP-SAL | 영업 | 영업기회, 견적, 수주, 계약, 출하 요청, 매출 |
| ERP-PUR | 구매 | 구매요청, 발주, 공급업체, 입고, 검수, 매입 |
| ERP-INV | 재고·창고 | 품목, 재고, 창고, 로케이션, 로트, 시리얼, 재고이동, 실사 |
| ERP-PRD | 생산계획 | 수요계획, 생산계획, MRP, 생산오더, 자재소요 |
| ERP-FIN | 회계·재무 | 전표, 계정, 매출채권, 매입채무, 세금, 원가, 결산 |
| ERP-HR | 인사 | 사원, 직무, 근태, 조직, 급여, 자격 |
| ERP-CST | 원가 | 표준원가, 실제원가, 배부, 원가요소, 원가집계 |
| MES | 제조실행 | 작업지시, 작업실적, 공정실적, 투입·산출, 생산추적, WIP |
| MOM | 제조운영 | 생산운영, 운영모델, 자원조정, 생산성과, 작업자·설비·자재 운영 |
| QMS | 품질 | 검사계획, 검사기준, 검사결과, 부적합, 시정·예방조치, 품질문서 |
| PLM | 제품수명주기 | 제품, 품목구조(BOM), 도면, 문서, 변경요청, 개정, 승인 |
| EAM | 설비·자산관리 | 설비, 자산, 설비계층, 보전계획, 고장, 정비작업, 예비품 |
| IIOT | 설비정보수집 | 수집원, 게이트웨이, 프로토콜, 신호, 측정값, 수집주기, 수집이력 |
| TAG | 설비 태그 | 태그 정의, 태그 인스턴스, 주소, 데이터형, 단위, 수집·보정 규칙 |
| WMS | 창고관리 | 입고, 적치, 피킹, 패킹, 출고, 로케이션, 재고이동 |
| TMS | 운송관리 | 운송요청, 운송계획, 차량, 운송구간, 배송, 인수증 |
| SCM | 공급망 | 수요·공급, 공급업체, 납기, 조달, 물류, 공급망 계획 |
| CRM | 고객관리 | 고객, 접점, 활동, 상담, 서비스요청, 고객관계 |
| SRM | 공급업체관리 | 공급업체 평가, 등록, 계약, 성과, 리스크 |
| DQM | 데이터품질 | 데이터 규칙, 프로파일, 품질측정, 오류, 정제, 중복 |
| DOC | 문서·기록 | 문서, 파일, 버전, 분류, 보존, 승인, 배포 |
| IAM | 인증·권한 | 주체, 신원, 역할, 권한, 정책, 인증, 세션 |
| COML | 상업·라이선스 | 고객, 견적, 수주, 계약, 구독, 청구, 결제, 환불, 라이선스 |

모듈 코드는 분류 및 문서 식별용이다. 모듈 코드를 이유로 공통 대표 약어를 중복 생성하지 않는다.

## 18. 업무 개념 통합 시 주의할 핵심 구분

| 개념군 | 대표 개념 | 관계·혼동 용어 | 통합 판정 |
| --- | --- | --- | --- |
| 주문 | Order | 주문 | 일반 상위 개념. 실제 업무에서는 수주·발주·작업지시를 구분 |
| 영업 주문 | Sales Order | 수주, 고객주문 | 동일한 영업 거래 개념이면 대표 용어에 연결 |
| 구매 주문 | Purchase Order | 발주, 구매오더 | 공급업체에 대한 구매 약정으로 별도 개념 |
| 생산 오더 | Production Order | 생산지시, 생산명령 | 생산계획·실행 대상. 영업 수주와 별도 |
| 작업 오더 | Work Order | 작업지시, 정비오더 | 수행할 작업의 지시 단위. 생산 오더와 업무 경계 확인 |
| 출하 | Shipment | 출하, 선적 | 물류 출발·인계 단위 |
| 배송 | Delivery | 납품, 배송 | 고객/수령지까지 전달하는 이행 단위 |
| 매출 | Revenue | 매출액, 매출실적 | 회계상 수익 인식 개념. 수주금액과 동일하지 않음 |
| 입고 | Goods Receipt | 입고, 자재입고 | 재고로 수령·인식하는 거래 |
| 출고 | Goods Issue | 출고, 자재출고 | 재고에서 차감·불출하는 거래 |
| 재고 | Inventory | 재고자산, 재고수량 | 재고 상태·수량·가치 문맥을 구분 |
| 창고 | Warehouse | 창고, 물류창고 | 재고 보관·운영 장소 |
| 로케이션 | Location | 저장위치, 보관위치 | 창고 내부의 물리·논리 위치 |
| 공장 | Plant | 사업장, 제조사업장 | 생산·계획·재고 책임 단위 |
| 설비 | Equipment | 장비, 기계 | 관리 대상 자산/설비 개체 |
| 기계 | Machine | 기계 | 설비의 하위 유형일 수 있으며 설비와 무조건 동의어 처리 금지 |
| 공정 | Process | 공정, 제조공정 | 제조 변환 단계·흐름 |
| 작업 | Operation | 작업, 공정작업 | 공정 내 실행 가능한 작업 단계 |
| 태그 | Tag | 계측점, 태그명 | 설비 신호를 식별하는 논리 식별자 |
| 신호 | Signal | 계측신호, 센서신호 | 수집되는 물리·논리 신호 |
| 측정값 | Measurement | 계측값, 측정 데이터 | 특정 시점의 관측 값 |
| 품목 | Item | 자재, 제품 | 공통 카탈로그 개념. 자재·제품 속성은 구분 |
| 제품 | Product | 완제품, 판매제품 | 판매·생산 산출물 |
| 자재 | Material | 원자재, 부자재, 소모품 | 생산·운영에 투입되는 자원 |
| BOM | Bill of Materials | 자재명세서, 부품표 | 제품 구성 관계 |
| 개정 | Revision | 리비전, 개정번호 | 문서·설계·기준의 변경 버전 |
| 버전 | Version | 버전, 판 | 기술적·배포 단위. 개정과 문맥 구분 |
| 공통코드 | Common Code | 공통 코드, 코드그룹 | 여러 모듈이 공유하는 코드 체계 |

## 19. 공통 업무 용어 사전의 초기 확장 후보

아래는 모듈 확장을 위한 초기 후보이다. 동일 개념 여부와 대표 약어의 전역 충돌 검사를 마치기 전에는 **검토 후보**이며, 확정 약어로 신규 컬럼에 사용하지 않는다.

| 모듈 | 대표 개념 후보 | 한글 업무명 | 함께 검색할 관계 용어 |
| --- | --- | --- | --- |
| 공통 | Organization | 조직 | 법인, 회사, 사업부 |
| 공통 | Site | 사업장 | 사업장, 사업소, 현장 |
| 공통 | Plant | 공장 | 제조사업장, 생산거점 |
| 공통 | Department | 부서 | 부문, 팀 |
| 공통 | Partner | 거래처 | 거래상대방, 비즈니스 파트너 |
| 공통 | Customer | 고객 | 매출처, 수요처 |
| 공통 | Supplier | 공급업체 | 공급자, 매입처, 협력사 |
| 공통 | Item | 품목 | 품목마스터, 자재·제품의 상위 카탈로그 |
| 공통 | Product | 제품 | 판매제품, 생산제품, 완제품 |
| 공통 | Material | 자재 | 원자재, 부자재, 소모품 |
| 공통 | Unit of Measure | 단위 | 계량단위, 재고단위, 기준단위 |
| 공통 | Common Code | 공통코드 | 코드그룹, 공통코드값 |
| 영업 | Sales Order | 수주 | 고객주문, 판매주문 |
| 영업 | Quotation | 견적 | 견적서, 견적요청 결과 |
| 영업 | Sales | 영업 | 판매활동, 영업실적 |
| 영업 | Revenue | 매출 | 매출액, 수익인식 금액 |
| 구매 | Purchase Request | 구매요청 | 구매 requisition, 자재요청 |
| 구매 | Purchase Order | 발주 | 구매주문, 발주서 |
| 구매 | Goods Receipt | 입고 | 자재입고, 구매입고 |
| 구매 | Supplier Invoice | 매입송장 | 매입청구서, 공급업체 송장 |
| 재고 | Inventory | 재고 | 재고자산, 재고수량 |
| 재고 | Warehouse | 창고 | 물류창고, 보관창고 |
| 재고 | Location | 로케이션 | 저장위치, 보관위치, Bin 상위 위치 |
| 재고 | Lot | 로트 | 배치, 로트번호 |
| 재고 | Serial Number | 일련번호 | 시리얼, 개체번호 |
| 재고 | Stock Transfer | 재고이동 | 창고이동, 위치이동 |
| 재고 | Stock Count | 재고실사 | 실사, 재고조사 |
| 생산 | Production Plan | 생산계획 | 제조계획, 생산일정 |
| 생산 | Production Order | 생산오더 | 생산지시, 제조오더 |
| 생산 | Work Order | 작업오더 | 작업지시, 작업명령 |
| 생산 | Bill of Materials | 자재명세서 | BOM, 부품표 |
| 생산 | Routing | 공정경로 | 공정순서, 제조경로 |
| 생산 | Work Center | 작업장 | 작업센터, 생산작업장 |
| 생산 | Work in Process | 재공 | WIP, 재공품 |
| 제조 | Process | 공정 | 제조공정, 공정단계 |
| 제조 | Operation | 작업 | 공정작업, 세부작업 |
| 제조 | Production Result | 생산실적 | 생산수량, 실적 |
| 제조 | Consumption | 투입·소모 | 자재투입, 소비량 |
| 제조 | Yield | 수율 | 양품률 문맥은 Quality와 연결하되 별도 지표 정의 |
| 품질 | Inspection | 검사 | 검수, 시험, 검사활동 |
| 품질 | Inspection Result | 검사결과 | 측정결과, 판정결과 |
| 품질 | Defect | 결함 | 불량, 결점 |
| 품질 | Nonconformance | 부적합 | 부적합품, 규격이탈 |
| 품질 | Corrective Action | 시정조치 | 시정, 시정대책 |
| 품질 | Preventive Action | 예방조치 | 예방대책 |
| PLM | Engineering Change | 설계변경 | 기술변경, 변경통보 |
| PLM | Document | 문서 | 도면, 사양서, 기술문서 |
| PLM | Revision | 개정 | 리비전, 개정번호 |
| 설비 | Equipment | 설비 | 장비, 생산설비 |
| 설비 | Asset | 자산 | 고정자산, 관리자산 |
| 설비 | Maintenance | 보전 | 유지보수, 정비 |
| 설비 | Maintenance Order | 정비오더 | 보전작업, 정비지시 |
| 설비 | Failure | 고장 | 장애, 고장현상 |
| 설비 | Spare Part | 예비품 | 스페어파트, 보전부품 |
| 수집 | Data Source | 데이터 원천 | 수집원, 데이터소스 |
| 수집 | Gateway | 게이트웨이 | 수집 게이트웨이, 엣지 장치 |
| 수집 | Protocol | 프로토콜 | 통신규약 |
| 수집 | Signal | 신호 | 계측신호, 센서신호 |
| 수집 | Measurement | 측정값 | 계측값, 관측값 |
| 태그 | Tag | 태그 | 태그 포인트, 계측 태그 |
| 태그 | Tag Address | 태그 주소 | PLC 주소, 데이터 주소 |
| 태그 | Sampling Interval | 샘플링 주기 | 수집주기, 폴링주기 |
| 태그 | Calibration | 보정 | 교정, 보정값 |
| 공통 | Approval | 승인 | 결재, 승인처리 |
| 공통 | Attachment | 첨부 | 첨부파일, 첨부문서 |
| 공통 | Comment | 의견 | 코멘트, 비고와 구분 |
| 공통 | Reason | 사유 | 사유코드, 처리사유 |
| 공통 | Evidence | 증적 | 근거자료, 증빙 |
| 공통 | Effective Date | 적용일 | 시행일, 효력발생일 |
| 공통 | Expiration | 만료 | 유효기간 종료, 만료일 |

## 20. 사전 확정 전 필수 검증

- [ ] 대표 용어가 동일 개념의 관계 용어를 모두 수용하는가?
- [ ] 상위어·하위어·문맥 유사어를 동의어로 잘못 합치지 않았는가?
- [ ] 대표 약어가 모든 업무 모듈을 통틀어 유일한가?
- [ ] 3자 약어 충돌 시 4자 확장 규칙을 적용했는가?
- [ ] 4자 충돌은 임의 약어를 만들지 않고 보류했는가?
- [ ] 기존 약어와 신규 약어의 레거시 호환 매핑이 있는가?
- [ ] 테이블명·컬럼명·인덱스명·제약조건명에서 대표 용어만 사용했는가?
- [ ] SQLite를 포함한 모든 공급자에서 같은 물리 이름을 사용하는가?
- [ ] Platform·DataSet·DataSet Content·Worker·Commercial 스키마를 모두 대조했는가?
- [ ] ERP·MES·MOM·QMS·PLM·EAM·IIoT·TAG 업무의 중복 개념을 교차 검토했는가?
- [ ] 표준 사전의 변경 이력과 승인 상태를 기록했는가?
