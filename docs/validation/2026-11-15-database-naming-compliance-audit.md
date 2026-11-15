# EDAN Tensor DB 명명 규칙·전자사전 정합성 감사

- 감사 기준일: 2026-11-15
- 저장소: `namanvarava/Edan.Tensor`
- 기준 브랜치: `main` (표준 문서 PR #252 병합 이후)
- 감사 범위: Platform·DataSet·Worker EF Core 모델, 기존 컬럼 검토표, 전자사전, 추가 DB 명명 규칙 Excel
- 판정 원칙: 실제 소스 모델, 제안 검토표, 전자사전 등록 항목을 서로 다른 근거로 구분

## 1. PR #252 병합 및 CI

- PR #252 `docs: 데이터베이스 테이블·컬럼 명명 표준 추가`는 `main`에 Squash 병합 완료.
- CI `build-and-test` 결과: SUCCESS.
- 병합 커밋: `c33db4833a52e282bb387c1c581c62e335f2c205`.
- 기준 문서: [데이터베이스 명명 규칙](../database-naming-conventions.md).

## 2. 감사 대상과 수량

| 대상 | 확인 결과 |
| --- | ---: |
| Platform EF Core 모델 테이블 | 19 |
| DataSet EF Core 모델 테이블 | 9 |
| Worker EF Core 모델 테이블 | 4 |
| EF Core 모델 테이블 합계 | 32 |
| 기존 컬럼 검토표 DB 파일 | 4 |
| 기존 컬럼 검토표 테이블 수 | 33 |
| 기존 컬럼 검토표 행 | 287 |
| 검토표의 고유 권장 컬럼명 | 167 |
| 전자사전 데이터 행 | 116 |
| 전자사전 고유 약어 | 111 |
| 전자사전 약어의 중복 행 | 5 |

검토표의 287행은 현재 소스 코드에 반영된 컬럼 수가 아니라, 4개 DB 파일을 대상으로 만든 검토 후보 행 수입니다. 검토표의 모든 287행 상태가 `검토 필요`입니다. `사용자 확정 컬럼명` 열에 값이 있는 5행도 물리 컬럼 약어가 아니라 한글 논리명이므로, 승인된 물리 컬럼명으로 계산하지 않았습니다.

검토표의 33개 테이블과 현재 EF Core 모델의 32개 테이블은 범위가 완전히 같지 않습니다. 검토표에는 별도 `DataSetContent` 저장소가 포함되어 있으나, 확인한 Platform·DataSet·Worker 세 DbContext에는 이 테이블이 나타나지 않습니다. 별도 콘텐츠 저장소의 실제 DDL·구현을 추가 확인해야 합니다.

## 3. 실제 EF Core 모델의 컬럼 표기 검사

확인한 소스:

- `src/Platform/EDAN.Tensor.Platform.Infrastructure/Persistence/PlatformDbContext.cs`
- `src/DataSet/EDAN.Tensor.DataSet.Infrastructure/Persistence/DataSetDbContext.cs`
- `src/Worker/EDAN.Tensor.Worker.Infrastructure/Persistence/WorkerDbContext.cs`
- 각 DbContext의 Model Snapshot

### 3.1 결과

| 모듈 | 엔터티 속성 매핑 표현식 | 명시적 물리 컬럼명 매핑 | 관찰 결과 |
| --- | ---: | ---: | --- |
| Platform | 151 | 6개 snake_case 매핑 | 대부분 CLR PascalCase 관례 이름 사용 |
| DataSet | 74 | 4개 PascalCase 매핑 | 물리 이름이 `TenantId`, `OrganizationId`, `ProjectId`, `StorageReference` |
| Worker | 46 | 0 | CLR PascalCase 관례 이름 사용 |
| 합계 | 271 | 10개 명시 매핑 | 표준 약어 snake_case와 일치하는 매핑은 확인되지 않음 |

위 271은 소스의 속성 매핑 표현식 수이며 서로 다른 DB 컬럼의 정확한 총계라고 단정하지 않습니다. EF Core 관례로 생성되는 속성, 소유 엔터티, shadow property가 포함될 수 있으므로 실제 생성 DDL과 Migration까지 확인해야 합니다.

Platform의 명시적 snake_case 이름은 `tenant_id`, `organization_id`, `project_id`입니다. 표기 자체는 snake_case지만 전자사전의 약어 조합(`tntid`, `orgid`, `pjtid`)과 다릅니다. DataSet의 명시 매핑은 PascalCase이고, Worker는 명시 매핑이 없습니다. Worker에는 `ReadyAtUtcUnixMilliseconds`, `CreatedAtUtcUnixMilliseconds`, `LeaseExpiresAtUtcUnixMilliseconds` shadow property도 정의되어 있습니다.

**판정:** 현재 EF Core 모델은 신규 표준의 물리 컬럼명 규칙을 전반적으로 준수하지 않습니다. 실제 적용은 아직 완료되지 않았으며, 후속 Migration과 명시적 `HasColumnName` 매핑이 필요합니다.

## 4. 실제 테이블명 검사

| 모듈 | 테이블 수 | 확인한 명명 사례 | 판정 |
| --- | ---: | --- | --- |
| Platform | 19 | `platform_tenants`, `platform_authorization_policies` 등 | 도메인 접두어와 snake_case 대체로 준수 |
| DataSet | 9 | `datasets`, `dataset_schema_revisions` 등 | `datasets`는 도메인 접두어 예외 |
| Worker | 4 | `worker_tasks`, `worker_task_replays`, `worker_task_types`, `external_deliveries` | `external_deliveries`는 도메인 접두어 예외 |

추가 관찰:
- `platform_authorization_audit`는 집합적 의미의 단수형 `audit`를 사용합니다. 복수형 원칙의 명시적 예외로 기록할지 검토해야 합니다.
- 기존 테이블은 이름을 일괄 변경하지 않습니다. 신규 표준과의 차이를 예외 목록으로 관리하고, 변경은 별도 Migration으로 처리합니다.

## 5. 전자사전 내부 정합성 검사

전자사전은 116개 데이터 행, 111개 고유 약어로 확인했습니다.

### 5.1 중복 행

다음 5개 약어가 동일한 영문 풀네임으로 각각 두 번 등록되어 있습니다. 의미 충돌은 아니지만 중복 행 정리가 필요합니다.

| 약어 | 영문 풀네임 | 판정 |
| --- | --- | --- |
| `arc` | Archive | 중복 행 |
| `act` | Action | 중복 행 |
| `eff` | Effect | 중복 행 |
| `pri` | Priority | 중복 행 |
| `ena` | Enabled | 중복 행 |

### 5.2 같은 개념의 약어 충돌

| 영문 풀네임 | 등록 약어 | 판정 및 권고 |
| --- | --- | --- |
| Role | `role`, `rol` | 동일 개념에 두 약어가 존재. 하나로 통일 필요 |

### 5.3 구형 표기와 조합형 표기 혼재

| 기존 항목 | 신규 조합 제안 | 정리 방향 |
| --- | --- | --- |
| `Creator(crtmer)` | `crt_by` | By 항목 등록 후 조합형 사용 |
| `Created DateTime(crtdt)` | `crt_dt` 또는 `crt_utc` | UTC 여부에 따라 선택 |
| `Updater(updmer)` | `upd_by` | By 항목 등록 후 조합형 사용 |
| `Updated DateTime(upddt)` | `upd_dt` 또는 `upd_utc` | UTC 여부에 따라 선택 |
| `Use Yes/No(useyn)` | `use_yn` | 조합형으로 통일 |
| `Delete Yes/No(delyn)` | `del_yn` | 조합형으로 통일 |
| `Sort Number(sortnum)` | `sort_num` | 조합형으로 통일 |

이 표는 전자사전의 구형 항목을 즉시 삭제하라는 의미가 아닙니다. 기존 DB·API 사용처를 조사한 후 Deprecated 또는 Legacy로 표시하고 신규 사용을 제한해야 합니다.

## 6. 컬럼 검토표와 전자사전 조합 검사

검토표 287행에는 167개의 고유 권장 컬럼명이 있습니다. 전자사전의 등록 약어를 토큰 사전으로 사용해 권장 컬럼명을 분해하는 1차 정적 검사를 수행했습니다.

- 167개 중 106개는 등록된 약어 조합만으로 분해 가능했습니다.
- 61개는 등록 약어만으로 완전하게 분해되지 않아 추가 약어 등록 또는 표기 수정이 필요합니다.
- 이는 자동 분해 기준의 1차 검사이며, 한글 업무 의미와 컬럼 맥락을 포함한 최종 의미 검토를 대체하지 않습니다.

### 6.1 대표 불일치

| 검토표 권장명 | 발견 내용 | 정리 방향 |
| --- | --- | --- |
| `prjid`, `prjcd`, `prjnm`, `prjst` | 전자사전은 Project = `pjt` | `pjtid`, `pjtcd`, `pjtnm`, `pjtsta` 후보로 통일 |
| `tntst`, `orgst`, `usrst`, `dsetst` | Status = `sta`와 불일치 | `tntsta`, `orgsta`, `usrsta`, `dsetsta` 후보 |
| `act_subid` | Action(`act`)과 Actor 의미를 혼용 | Actor(`actr`) 항목을 별도 등록하고 `actr_subid` 검토 |
| `int_hr` | Interval 약어가 `intv` | `intv_hr` 후보 |
| `result_code` | 완전한 단어 표기이며 전자사전 조합과 불일치 | `rslt_cd` 후보 |
| `key_hash` | 일반 Hash 항목 미등록 | Hash 약어 등록 또는 `sha`와 구분 후 결정 |
| `prj_lmt_byt` | Project 약어 `prj` 사용 | `pjt_lmt_byt` 후보 |
| `auth_aud_arc_jobid`, `jobid` | Job 항목 미등록 | Job 등록 후 `jobid` 또는 복합 약어 검토 |
| `crt_utc`, `upd_utc` | Created/Updated + UTC 조합 | 전자사전의 조합 규칙과 일치하도록 유지 |
| `ready_utc_ms`, `lease_exp_utc_ms`, `crt_utc_ms` | Ready·Milliseconds 의미 약어 미등록 | 기술 단위 약어 등록 및 컬럼 의미 검토 |

미분해된 권장명 61개는 미등록 단어·구형 약어·전자사전과 다른 철자 조합을 포함합니다. 전체 이름 목록과 각 항목의 의미 검토는 컬럼 검토표를 기준으로 별도 후속 작업에서 수행해야 합니다.

## 7. 명명 표준 문서와 전자사전의 불일치

표준 문서에 사용한 일부 약어가 기존 전자사전에 아직 없습니다. 예: `actr`, `by`, `expir`, `idn`, `rule`, `job`, `stg`, `row`, `hash`, `rdy`, `unix`, `tkn`.

따라서 표준 문서에 이 약어를 제안 형태로 명시하고, 전자사전 등록·승인 전에는 신규 DB에 적용하지 않도록 주의사항을 추가했습니다. `Status(sta)`, `Project(pjt)`, `Interval(intv)`처럼 이미 등록된 약어는 기존 전자사전 표기를 우선합니다.

## 8. 종합 판정

| 검사 항목 | 결과 | 상태 |
| --- | --- | --- |
| PR #252 CI | 성공 | 통과 |
| PR #252 main 병합 | 완료 | 통과 |
| 테이블명 snake_case | 대체로 준수 | 일부 예외 |
| 도메인 접두어 | 일부 누락 | 개선 필요 |
| 실제 EF Core 물리 컬럼명 | PascalCase 및 비표준 snake_case 혼재 | 미준수 |
| 전자사전 내부 중복 | 5개 중복 행 | 정리 필요 |
| 전자사전 의미 약어 충돌 | Role의 `role` / `rol` | 결정 필요 |
| 검토표 287행 승인 상태 | 287행 모두 검토 필요 | 미승인 |
| 고유 권장 컬럼명 전자사전 조합 | 167개 중 61개 미분해 | 추가 검토 필요 |
| 별도 DataSetContent 저장소 | 현재 세 DbContext 밖 | 별도 확인 필요 |

## 9. 권장 후속 작업 순서

1. 전자사전의 중복 행 5개와 Role 약어 충돌을 정리한다.
2. Actor·By·Job·Stage·Hash 등 미등록 개념의 영문 풀네임·한글 의미·권장 약어를 승인한다.
3. Project(`pjt`), Status(`sta`), Interval(`intv`) 등 기존 표준과 다른 검토표 약어를 정정한다.
4. 287개 컬럼 검토표의 모든 항목을 실제 DB·EF 모델·Migration·쿼리와 대조해 승인·수정·보류를 확정한다.
5. 승인된 이름을 기준으로 EF Core `HasColumnName` 매핑과 후속 Migration을 설계한다.
6. DataSetContent 별도 저장소의 실제 DDL과 컬럼 목록을 확보해 같은 검사를 수행한다.
7. DB 공급자별 Migration 생성·적용·롤백 및 API 호환성 테스트로 검증한다.

**이번 감사는 읽기 전용 조사입니다.** DB 테이블·컬럼, EF Core 모델, Migration 파일, 전자사전 원본 파일은 변경하지 않았습니다. GitHub에는 감사 보고서와 표준 문서의 주의사항만 추가 제안하며, 실제 스키마 변경은 별도 승인과 Migration 계획 이후 수행합니다.
