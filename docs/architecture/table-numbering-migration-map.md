# EDAN Framework / Edan 일련번호 테이블명 전환 기준

이 문서는 기존 운영 테이블을 `영역번호 + 테이블번호` 기반 물리명으로 전환하기 위한 기준 문서입니다. 기존 Migration과 운영 데이터가 존재하므로, 이 문서의 매핑과 호환성 Migration이 승인되기 전에는 기존 테이블을 삭제하거나 직접 Rename하지 않습니다.

## 1. 명명 규칙

| 데이터베이스 | 형식 | 예시 |
|---|---|---|
| EdanFramework | `edw_{영역 2자리}{테이블 3자리}` | `edw_01001` |
| Edan SYS | `ed_{영역 2자리}{테이블 3자리}` | `ed_00001` |
| Edan BAS | `ed_{영역 2자리}{테이블 3자리}` | `ed_01001` |

영역번호는 재사용하지 않으며, 삭제된 테이블의 테이블번호도 재사용하지 않습니다. 테이블명만으로 의미를 잃지 않도록 논리명과 물리명을 `table_registry`에 함께 보관합니다.

## 2. 현재 Platform 영역의 Framework 1차 매핑

| 현재 물리명 | 제안 물리명 | 논리명 | 영역 | 상태 |
|---|---|---|---|---|
| `platform_tenants` | `edw_01001` | 테넌트 | Framework 01 | 후보 |
| `platform_tenant_memberships` | `edw_01002` | 테넌트 구성원 | Framework 01 | 후보 |
| `platform_organizations` | `edw_01003` | 조직 | Framework 01 | 후보 |
| `platform_users` | `edw_01004` | 플랫폼 사용자 | Framework 01 | 후보 |
| `platform_organization_memberships` | `edw_01005` | 조직 구성원 | Framework 01 | 후보 |
| `platform_project_memberships` | `edw_01006` | 프로젝트 구성원 | Framework 01 | 후보 |
| `platform_projects` | `edw_01007` | 프로젝트 | Framework 01 | 후보 |
| `platform_resources` | `edw_01008` | 리소스 | Framework 01 | 후보 |
| `platform_policies` | `edw_01009` | 플랫폼 정책 | Framework 01 | 후보 |
| `platform_authorization_policies` | `edw_01010` | 권한 정책 | Framework 01 | 후보 |
| `platform_authorization_audit` | `edw_01011` | 권한 감사 | Framework 01 | 후보 |
| `platform_authorization_policy_changes` | `edw_01012` | 권한 정책 변경 | Framework 01 | 후보 |
| `platform_authorization_audit_retention` | `edw_01013` | 감사 보존 정책 | Framework 01 | 후보 |
| `platform_authorization_audit_retention_changes` | `edw_01014` | 감사 보존 변경 | Framework 01 | 후보 |
| `platform_authorization_audit_archive_jobs` | `edw_03001` | 감사 아카이브 작업 | Framework 03 | 후보 |
| `platform_authorization_audit_archive_attempts` | `edw_03002` | 감사 아카이브 시도 | Framework 03 | 후보 |
| `platform_authorization_audit_archive_schedules` | `edw_03003` | 감사 아카이브 일정 | Framework 03 | 후보 |
| `platform_storage_quota_policies` | `edw_02001` | 저장소 한도 정책 | Framework 02 | 후보 |
| `platform_storage_quota_policy_changes` | `edw_02002` | 저장소 한도 변경 | Framework 02 | 후보 |

### Worker·DataSet 번호 배정

| 기존 물리명 | 제안 물리명 | 영역 |
|---|---|---|
| `edwk_wkrdlv` | `edw_03004` | Framework 03 Worker |
| `edwk_wkrtsk` | `edw_03005` | Framework 03 Worker |
| `edwk_wkrrpl` | `edw_03006` | Framework 03 Worker |
| `edwk_wkrtyp` | `edw_03007` | Framework 03 Worker |
| `edds_dset` | `edw_05001` | Framework 05 DataSet |
| `edds_dsetschm_rev` | `edw_05002` | Framework 05 DataSet |
| `edds_dsetstr_usgsnp` | `edw_05003` | Framework 05 DataSet |
| `edds_dsetqta_threvt` | `edw_05004` | Framework 05 DataSet |
| `edds_dsetvld_prf` | `edw_05005` | Framework 05 DataSet |
| `edds_dsetvld_prfrev` | `edw_05006` | Framework 05 DataSet |
| `edds_dsetvld_exc` | `edw_05007` | Framework 05 DataSet |
| `edds_dsetvld_dtl` | `edw_05008` | Framework 05 DataSet |
| `edds_dsetvld_seenkey` | `edw_05009` | Framework 05 DataSet |

## 3. 현재 DataSet·Worker 영역의 처리 원칙

현재 DataSet과 Worker 테이블은 고객사 Edan BAS로 바로 이동하지 않습니다. Tensor 내부 운영 기능인지 고객 업무 데이터인지 확인한 뒤 다음 기준으로 분류합니다.

| 현재 영역 | 1차 분류 | 처리 원칙 |
|---|---|---|
| DataSet 메타데이터 | Framework 또는 별도 DataSet 영역 | 고객 업무 테이블과 직접 합치지 않음 |
| Worker 작업·재시도 | Framework 운영 03 | `edw_030xx`로 별도 번호 부여 |
| DataSet 콘텐츠 | 콘텐츠 저장소 | DB 테이블과 파일/오브젝트 저장을 분리 검토 |
| EF Migration History | Framework 관리 | EF Core가 관리하므로 제품 테이블 규칙으로 Rename하지 않음 |

## 4. 공통 컬럼 기준

| 컬럼 | 규칙 |
|---|---|
| `id` | UUID 기반 PK. Provider별 호환 타입을 사용 |
| `tenant_id` | Framework 테넌트 경계. Edan에 직접 FK로 연결하지 않음 |
| `company_id` | Edan 고객사 또는 사업장 참조 |
| `created_at_utc` | 생성 시각, UTC |
| `updated_at_utc` | 수정 시각, UTC |
| `created_by` | 생성 주체 |
| `updated_by` | 수정 주체 |
| `status` | 수명주기 상태 |
| `is_deleted` | 논리 삭제 여부 |
| `version_no` | 낙관적 동시성 버전 |

컬럼은 의미 기반 이름을 유지하며 `col001`과 같은 컬럼 일련번호는 사용하지 않습니다.

## 5. 제약조건·인덱스 이름

```text
PK_{물리테이블명}
FK_{자식물리테이블}_{부모물리테이블}_{컬럼명}
UX_{물리테이블명}_{컬럼명}
IX_{물리테이블명}_{컬럼명}
```

예시:

```text
PK_edw_01001
FK_edw_02001_edw_01001_tenant_id
UX_edw_01001_tenant_code
IX_edw_02001_tenant_id
```

## 6. 전환 절차

1. 현재 DB별 테이블·컬럼·PK·FK·Index를 추출합니다.
2. 이 문서의 후보 매핑을 실제 모델과 대조합니다.
3. `table_registry`와 기존명-신규명 매핑을 확정합니다.
4. Provider별 Rename/Compatibility Migration을 작성합니다.
5. EF Core `ToTable`, Raw SQL, Repository, Routine, 테스트를 변경합니다.
6. 데이터 행 수·PK·FK·Index·Migration History를 비교합니다.
7. PostgreSQL·SQL Server·Oracle·SQLite에서 각각 검증합니다.
8. 운영 백업과 복구 테스트 후 승인된 배포 창구에서 적용합니다.

## 7. 병합 게이트

다음 항목이 모두 통과하기 전에는 `main` 병합을 수행하지 않습니다.

- 후보 매핑이 승인된 상태
- 실제 컬럼·PK·FK·Index 명세 완료
- Framework/Edan Migration 분리 완료
- Provider별 Migration 성공
- 기존 테이블명 소스 검색 결과 검토 완료
- API·Health·Metadata·Routine 검증 통과
- 데이터 정합성 비교 통과
- 백업·복구 검증 통과
- 보안·라이선스·Secret 검증 통과
- 리뷰와 CI 통과

## 8. 현재 구현 진행 상태

| 항목 | 상태 | 비고 |
|---|---|---|
| Framework Platform EF 매핑 | 진행 완료 | 기존 `edpf_*` 매핑을 `edw_*` 순차명으로 전환 |
| PostgreSQL Platform Migration | 생성 완료 | 기존 테이블·인덱스·제약조건을 보존하는 Rename/Column 전환 Migration |
| SQL Server Platform Migration | 생성 완료 | 실제 적용 전 연결·백업·Row count 검증 필요 |
| Oracle Platform Migration | 생성 완료 | 실제 적용 전 연결·백업·Row count 검증 필요 |
| SQLite Platform Migration | 호환 전환 완료 | 기존 `edpf_*` 단계에서 `edw_*`로 테이블만 안전하게 rename하고 승인 컬럼은 중복 재명명하지 않음 |
| Worker/DataSet Framework 매핑 | 진행 완료 | 03·05 영역 번호와 소스 매핑 및 Provider별 Migration 생성; SQLite 호환 스냅샷 검증 완료 |
| Edan SYS·BAS EF 매핑 | 구현 완료 | `ed_00001`~`ed_00005` SYS, `ed_01001`~`ed_01005` BAS; PK/FK/Index/Comment 포함 |
| Edan SQLite 기준 Migration | 생성 완료 | 런타임 Edan Infrastructure에 기준 Migration 포함, 테스트 2건 통과 |
| Edan PostgreSQL/SQL Server/Oracle Migration | 코드 생성 완료 | provider별 독립 프로젝트·Snapshot·초기 Migration 생성; 실 DB 적용·복구 검증 필요 |
| Edan Migration 적용 스크립트 | 구현 완료 | `scripts/edan/Apply-Migrations.ps1`; Provider에 맞는 독립 Migration 프로젝트를 선택 |
| Edan 구조 자동 점검 | 구현 완료 | `scripts/edan/Verify-EdanSchema.ps1`; SYS/BAS 10개 물리명과 4개 Provider 초기 Migration 존재 확인 |
| Provider Migration 정적 점검 | 구현 완료 | `scripts/verification/Verify-ProviderMigrationArtifacts.ps1`; 7개 Provider Migration 프로젝트와 4개 DbContext/순차 매핑을 자동 확인 |
| Provider 수용 검증 Runbook | 구현 완료 | `docs/verification/provider-acceptance-runbook.md`; 백업·Migration·Health·Metadata·Routine·복구 순서를 고정 |
| 운영 DB 적용 | 미착수 | 코드 검증과 provider별 사전 복구 테스트 후 마지막에 수행 |

이번 단계의 Migration은 코드 저장소에 준비한 것이며, 개발 VM의 실제 PostgreSQL·SQL Server·Oracle에는 아직 자동 적용하지 않습니다. Provider별 적용 시에는 기존 백업, 사전 Row count, PK/FK/Index 목록, 적용 후 동일 검증을 필수로 남깁니다.

기존 SQLite 단계의 테이블명과 새 순차 테이블명이 혼재하던 문제는 Worker/DataSet/Platform 호환 Migration과 모델 스냅샷을 정렬하여 수정했습니다. 현재 확인된 테스트 결과는 Platform 20개, Worker 73개, DataSet Infrastructure 12개, Edan Infrastructure 2개 통과입니다. 전체 솔루션 빌드는 경고 0개·오류 0개로 통과했으며, 비통합 회귀 테스트도 모두 통과했습니다. API 통합 테스트는 별도 종료 결과와 Provider 실 DB 검증이 남아 있으므로 최종 병합 전 게이트는 아직 닫혀 있습니다.

## 9. Edan 연결 설정

Edan 업무 DB는 기존 Framework DB와 분리된 `EdanDbContext`로 등록되며, 다음 설정이 명시된 경우에만 API에 활성화됩니다.

```text
Edan__Enabled=true
Edan__Provider=PostgreSQL|SqlServer|Oracle|Sqlite
ConnectionStrings__EdanDatabase=<Edan 업무 DB 연결 문자열>
```

기본값은 비활성화이므로 기존 설치의 Platform·Worker·DataSet 연결에는 영향을 주지 않습니다. Edan SYS/BAS API와 Provider별 Edan Migration은 독립 프로젝트로 구현되어 있으며, 실제 4개 Provider 적용·복구 검증이 남아 있습니다.

EF Design 도구 패키지는 Edan 런타임 프로젝트에서 제거했습니다. Migration 생성 도구는 Provider별 독립 프로젝트로 분리했고, `System.Security.Cryptography.Pkcs/Xml`은 11.0.0으로 올려 복원 단계의 취약성 경고를 제거했습니다. Migration 도구는 상용 런타임 이미지에 포함하지 않고 CI 전용으로 사용합니다.
