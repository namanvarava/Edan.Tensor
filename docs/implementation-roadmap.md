# 전체 구현 진행 방향

이 문서는 EDAN Tensor의 이후 구현 순서와 단계별 완료 기준을 고정합니다. 구현자는 이 순서를 임의로 바꾸지 않으며, 앞 단계의 수용 기준과 증거가 확보된 뒤 다음 단계로 이동합니다. 이 문서는 구현 방향을 기록하는 기준 문서이며, 각 업무 기능의 상세 계약은 해당 단계의 승인 문서와 함께 확정합니다.

## 기준 상태

현재 기준 main은 `c982dbf7c4f01a0b0fc29fd333a304f493713188`입니다. API Host는 Platform·Worker·DataSet 메타데이터·DataSet 콘텐츠를 분리한 SQLite 연결과 EF Core Migration을 사용하고, Development 환경에서 OpenAPI JSON과 Swagger UI를 제공합니다. LicenseAuthority의 실제 발급 업무는 아직 구현하지 않았습니다.

개발 초기에는 별도 DB 서버를 설치하지 않습니다. 현재 구조의 로컬 SQLite 파일과 임시 SQLite 테스트 DB를 사용하며, 운영 DB 공급자는 0단계에서 결정한 뒤 별도 Migration 호환성 검증을 수행합니다.

## 고정된 진행 순서

### 0단계. 전체 범위·수용 기준·운영 DB를 결정합니다.

이 단계에서는 전체 제품의 범위와 완료 판정 기준을 승인 문서로 확정합니다.

- Platform, DataSet, Worker, Commercial, Licensing, WPF, 외부 전달의 기능 목록과 API·데이터·화면 수용 기준을 확정합니다.
- 로컬 개발 DB는 SQLite로 유지하고, 운영 DB 공급자와 배포 방식을 별도로 결정합니다.
- Tenant 격리, 인증·인가, Migration 호환성, Worker Lease/Fencing, 라이선스 경계와 보안 기준을 공통 계약으로 확정합니다.
- 결제·라이선스 발급 규칙, WPF 업무 흐름, DuckDB·Parquet 지원 범위처럼 현재 문서에서 제외한 요구는 승인 전까지 구현 계약으로 간주하지 않습니다.

이 단계의 완료 기준은 승인된 범위 문서, API 계약, 데이터 계약, 운영 DB 결정, 단계별 수용 기준이 저장소에 기록된 상태입니다.

### 1단계. DB 없이 API 계약과 단위 테스트를 구현합니다.

이 단계에서는 외부 DB 서버나 LicenseAuthority 없이 업무 규칙과 API 계약을 검증합니다.

- Domain 규칙, Application 서비스, 요청·응답 DTO, 오류 계약과 OpenAPI 설명을 구현합니다.
- 저장소 경계는 인터페이스로 분리하고, 단위 테스트는 메모리 또는 임시 SQLite를 사용합니다.
- 제품 Migration에 임시 테스트 테이블을 추가하지 않으며, 테스트 데이터는 테스트 DB 수명주기로 관리합니다.
- API가 개발용 인증 없이 실행되더라도 이는 Development 전용 테스트 경계이며 운영 인증·인가나 라이선스 검증을 대체하지 않습니다.

이 단계의 완료 기준은 API 계약 테스트, Domain·Application 단위 테스트, 오류·입력 검증 테스트가 통과하고 저장소 구현에 종속되지 않는 상태입니다.

### 2단계. 로컬 SQLite·Migration·Swagger UI를 연결합니다.

이 단계에서는 실제 API Host를 새 로컬 환경에서 기동하고 Swagger UI로 DB 연결을 확인합니다.

- Platform DB, Worker DB, DataSet 메타데이터 DB, DataSet 콘텐츠 DB를 각각 Migration으로 생성합니다.
- Development 환경에서만 Swagger UI와 OpenAPI JSON을 제공합니다.
- `/health`, `/health/database`, `/openapi/v1.json`, Swagger UI의 HTTP 상태를 확인합니다.
- Swagger UI에 Bearer 인증 방식과 `X-Tenant-Id` 사용법을 표시합니다.
- Development Bypass는 고정된 테스트 주체와 Tenant를 사용하되 Tenant 소속·권한 검사는 유지합니다.
- 운영 환경에서는 Swagger UI와 Bypass가 활성화되지 않도록 회귀 테스트를 유지합니다.

이 단계의 완료 기준은 새 임시 경로에서 Migration이 성공하고, DB 상태가 `Connected/Green`이며, Swagger UI의 `Try it out`으로 보호 API를 호출하고 데이터를 저장·조회하는 상태입니다.

### 3단계. Platform·DataSet·Worker API 전체 흐름을 구현하고 검증합니다.

이 단계에서는 라이선스 없이 핵심 제품 흐름을 끝까지 실행합니다.

```text
Tenant 생성 및 활성화
→ Organization·Project 생성
→ 사용자 소속·권한 등록
→ DataSet 생성
→ Schema revision 등록
→ CSV·JSON 콘텐츠 저장
→ Validation 작업 등록
→ Worker 실행
→ 작업 상태·결과·상세 오류 조회
→ 저장량·quota·감사 범위 확인
```

- 모든 API는 Tenant·Organization·Project 소유 범위와 권한을 확인합니다.
- 중복 요청, 동시 수정, Idempotency, Worker Lease/Fencing, 재시도와 DLQ를 검증합니다.
- API 재시작과 Worker 프로세스 재시작 후 데이터·작업 상태·복구 결과를 확인합니다.
- 다른 Tenant의 데이터가 조회·수정·삭제되지 않는지 부정 테스트를 수행합니다.

이 단계의 완료 기준은 새 DB에서 Swagger 수동 흐름, API 통합 테스트, Worker 실제 프로세스 테스트, Tenant 격리·권한·장애 복구 테스트가 모두 통과하는 상태입니다.

### 4단계. 별도 LicenseAuthority와 개발 라이선스 1개를 구현합니다.

이 단계에서는 기존 Licensing 모듈을 복제하지 않고 별도 솔루션에서 발급 Host를 구성합니다.

- `EDAN.Tensor.LicenseAuthority.sln`은 기존 Licensing Contracts·Domain·Application·Infrastructure 프로젝트를 공유합니다.
- LicenseId, Site 또는 Tenant, 제품, 기능, 발급일, 시작일, 만료일, KeyId, 서명과 상태 규칙을 승인된 계약으로 확정합니다.
- 개인키는 발급 프로그램에만 보관하고 API에는 공개키만 배포합니다.
- 초기에는 규칙에 맞는 Development 라이선스 1개를 발급합니다.
- 발급 결과와 키 자료를 저장소·로그에 비밀값으로 기록하지 않습니다.

이 단계의 완료 기준은 발급 프로그램의 정상·오류 테스트와 서명된 개발 라이선스 1개가 계약에 맞게 생성되는 상태입니다.

### 5단계. 라이선스 적용 API를 검증합니다.

이 단계에서는 4단계의 개발 라이선스를 API에 연결합니다.

- API는 공개키로 서명, 제품, Site 또는 Tenant, 기능, 유효 기간과 KeyId를 검증합니다.
- 정상, 위조, 만료, 시작 전, 다른 Tenant, 폐기, 알 수 없는 KeyId를 각각 거부합니다.
- Development 테스트에서는 명시된 개발 설정으로만 라이선스 발급 Host를 대체할 수 있습니다.
- External·운영 환경에서는 라이선스 검증 실패를 허용하지 않고 명확한 오류를 반환합니다.
- 인증 Bypass와 라이선스 검증 Bypass를 서로 다른 경계로 유지합니다.

이 단계의 완료 기준은 라이선스 적용 API의 통합·보안·회귀 테스트와 Swagger 실행 검증이 통과하는 상태입니다.

### 6단계. 나머지 제품 기능을 구현합니다.

앞 단계의 수직 흐름이 안정된 뒤 다음 기능을 순서대로 구현합니다.

- DuckDB·Parquet 분석 어댑터와 지원 타입·용량·복구 기준을 구현합니다.
- 실제 WorkerHost와 외부 수신 시스템의 응답 유실·중복 전달·재시도 E2E를 검증합니다.
- Commercial의 상품·요금·결제·Webhook·권리·Tenant별 entitlement를 구현합니다.
- WPF 클라이언트의 승인된 화면과 업무 흐름을 구현합니다.
- 역할 변경·폐기, 초대 위임, 작업별 CRUD, 동적 정책 등 승인된 고급 권한 기능을 구현합니다.

이 단계의 완료 기준은 각 기능의 승인된 수용 기준, API·데이터·화면 테스트, 운영 장애 시나리오가 모두 통과하는 상태입니다.

### 7단계. 운영 DB·실제 IdP·외부 시스템·배포를 검증합니다.

이 단계에서는 로컬 SQLite나 Development Bypass를 운영 검증의 대체물로 사용하지 않습니다.

- 결정된 운영 DB에 Migration, 백업·복구, 동시성, 성능과 장애 복구를 적용합니다.
- 실제 비운영 IdP의 Metadata/JWKS, Issuer, Audience, 테스트 Tenant·사용자·안정적인 `sub`, 키 회전을 검증합니다.
- 외부 수신 시스템, 네트워크 오류, 응답 유실, 재시도와 Idempotency를 실제 환경에서 검증합니다.
- 비밀 저장소, HTTPS, 배포 설정, 로그·메트릭·알림, 롤백 절차를 확인합니다.
- GitHub main 보호 규칙은 저장소 권한과 요금제 조건이 충족된 뒤 실제 설정과 HTTP 응답을 검증합니다.

이 단계의 완료 기준은 외부 환경의 실행 로그·응답·복구 결과가 보존되고, 실제 환경 미검증 항목이 0건인 상태입니다.

### 8단계. 최종 보안·성능·장애 복구·회귀 검증을 수행합니다.

이 단계에서는 전체 제품을 하나의 릴리스 후보로 검증합니다.

- Release Restore·Build·Test와 최신 main CI를 실행합니다.
- 인증·인가·Tenant 격리·라이선스·비밀정보 노출·취약 패키지를 감사합니다.
- API 부하, 대용량 DataSet, Worker 장시간 실행, DB 장애, 외부 전달 장애와 복구 시간을 측정합니다.
- Migration 호환성, 백업 복구, 재배포, 롤백과 데이터 무결성을 확인합니다.
- 사용 가이드, 운영 절차, API 문서, 변경 근거와 인수인계 기록을 갱신합니다.
- PR 병합, 최신 main 재검증, 작업 브랜치의 로컬·원격 삭제를 확인합니다.

이 단계에서만 전체 제품의 개발·검증 완료를 판정합니다.

## 단계 공통 이동 조건

각 단계는 다음 조건을 모두 충족해야 다음 단계로 이동합니다.

1. 구현 범위와 수용 기준이 승인되어야 합니다.
2. 대상 단위·통합·경계·회귀 테스트가 통과해야 합니다.
3. DB·인증·권한·Tenant 격리·보안 계약의 회귀가 없어야 합니다.
4. 실제 실행 로그와 테스트 결과를 검증 문서에 기록해야 합니다.
5. Release Build와 CI가 성공해야 합니다.
6. 문서와 공개 API 설명이 실제 동작과 일치해야 합니다.

외부 권한이나 운영 환경이 없는 경우에는 해당 단계만 `환경 미검증`으로 기록하며, 성공으로 표시하지 않습니다. 예약 작업·스케줄러·정기 알림은 생성하지 않습니다.
