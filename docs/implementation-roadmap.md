# 전체 구현 진행 방향

이 문서는 EDAN Tensor의 이후 구현 순서와 단계별 완료 기준을 고정합니다. 구현자는 이 순서를 임의로 바꾸지 않으며, 앞 단계의 수용 기준과 증거가 확보된 뒤 다음 단계로 이동합니다. 이 문서는 구현 방향을 기록하는 기준 문서이며, 각 업무 기능의 상세 계약은 해당 단계의 승인 문서와 함께 확정합니다.

## 이전 기준 상태 기록

당시 기준 main은 `99ede108a165764e7f6c57a7dfe80ef678ad5ea3`이며 PR #124 병합 후의 기준입니다. API Host는 Platform·Worker·DataSet 메타데이터·DataSet 콘텐츠를 분리한 SQLite 연결과 EF Core Migration을 사용하고, Development 환경에서 OpenAPI JSON과 Swagger UI를 제공합니다. `/health`와 `/health/database`는 네 개 로컬 데이터베이스의 연결·기본 스키마 읽기를 확인하며, DataSet 검증 예약은 Tenant 범위 Idempotency-Key로 중복 실행을 수렴시킵니다. 공통 예외 처리기는 안정적인 API 오류 코드와 한국어 설명을 반환하고 내부 예외 상세를 외부 응답에서 숨깁니다. Worker 등록 실패 시 External Delivery와 DataSet Validation의 새 미실행 레코드는 내부 보상 조건으로 정리합니다. 저장 용량 정책은 Tenant 관리자 API에서 페이지·변경 이력·낙관적 동시성으로 관리하고, 버전 충돌은 안정적인 도메인 오류 코드로 반환합니다. Development Bypass는 코드에 고정된 비운영 Tenant와 안정적인 `sub`를 사용하며 외부 설정으로 재정의할 수 없습니다. LicenseAuthority의 실제 발급 업무는 아직 구현하지 않았습니다.

### 현재 이동 게이트

API 계약·공통 오류 처리, 고정된 Development Bypass Tenant·주체 경계, Worker Lease/Fencing·체크포인트·재시도·DLQ·재처리·종료 인계·강제 종료 복구·Tenant 격리·외부 전달 멱등성은 main에 반영되어 있습니다. PR #120 병합 후 Windows 자체 호스팅 Release CI에서 전체 231개 테스트가 통과했습니다. 따라서 다음 구현 단위는 새 Worker 기능을 중복 추가하는 것이 아니라 다음 순서로 진행합니다.

1. API 계약·Tenant·인증 경계와 Worker 장애 증거는 문서·운영 가이드에 동기화되었고, 최신 main에서 실제 Host Smoke까지 확인되었습니다.
2. DuckDB·Parquet·Commercial·WPF는 공개 계약과 수용 기준이 승인된 뒤 라이선스보다 먼저 구현합니다.
3. 라이선스는 저장소 내부의 마지막 단계로 둡니다. 승인된 발급 규칙과 별도 LicenseAuthority 솔루션이 준비된 뒤에만 구현하며, 그 전에는 API에 라이선스 우회 경계를 추가하지 않습니다.

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

### 4단계. DuckDB·Parquet·Commercial·WPF 기능을 구현합니다.

- 이 단계에서는 승인된 포맷·데이터·화면 계약을 기준으로 라이선스가 없는 제품 기능을 먼저 완성합니다.
- DuckDB·Parquet 분석 어댑터와 지원 타입·용량·복구 기준을 구현합니다.
- 실제 WorkerHost와 외부 수신 시스템의 응답 유실·중복 전달·재시도 E2E를 검증합니다.
- Commercial의 상품·요금·결제·Webhook·권리·Tenant별 entitlement를 구현합니다.
- WPF 클라이언트의 승인된 화면과 업무 흐름을 구현합니다.
- 역할 변경·폐기, 초대 위임, 작업별 CRUD, 동적 정책 등 승인된 고급 권한 기능을 구현합니다.

이 단계의 완료 기준은 각 기능의 승인된 수용 기준, API·데이터·화면 테스트, 운영 장애 시나리오가 모두 통과하는 상태입니다.

### 5단계. 승인된 계약으로 별도 LicenseAuthority와 개발 라이선스 1개를 구현합니다.

이 단계에서는 승인된 라이선스 계약을 기준으로 기존 Licensing 모듈을 복제하지 않고 별도 솔루션에서 발급 Host를 구성합니다. Commercial 결제 공급자와 유료 권리는 이 단계에서 확정하지 않습니다.

- `EDAN.Tensor.LicenseAuthority.sln`은 기존 Licensing Contracts·Domain·Application·Infrastructure 프로젝트를 공유합니다.
- LicenseId, Site 또는 Tenant, 제품, 기능, 발급일, 시작일, 만료일, KeyId, 서명과 상태 규칙을 승인된 계약으로 적용합니다.
- 개인키는 발급 프로그램에만 보관하고 API에는 공개키만 배포합니다.
- `EDAN-NONPROD` 대상 Development 라이선스 1개를 발급하고 발급일 기준 5년 유효성을 확인합니다.
- 발급 결과와 키 자료를 저장소·로그에 비밀값으로 기록하지 않습니다.

이 단계의 내부 코드 완료 기준은 발급 프로그램의 정상·오류 테스트와 서명된 개발 라이선스 1개가 계약에 맞게 생성되는 상태입니다. 별도 프로세스 HTTP 스모크는 loopback 권한이 허용된 환경에서 추가 확인합니다.

### 6단계. 라이선스 적용 API를 검증하고 내부 범위를 닫습니다.

이 단계에서는 5단계의 개발 라이선스를 LicenseAuthority 검증 API에 연결하고 핵심 제품 API에 적용할 최종 경계를 확인합니다.

- API는 공개키로 서명, 제품, Site 또는 Tenant, 기능, 유효 기간과 KeyId를 검증합니다.
- 정상, 위조, 만료, 시작 전, 다른 Tenant, 폐기, 알 수 없는 KeyId를 각각 거부합니다.
- Development 테스트에서는 명시된 개발 설정으로만 라이선스 발급 Host를 대체할 수 있습니다.
- External·운영 환경에서는 라이선스 검증 실패를 허용하지 않고 명확한 오류를 반환합니다.
- 인증 Bypass와 라이선스 검증 Bypass를 서로 다른 경계로 유지합니다.

이 단계의 완료 기준은 라이선스 적용 API의 통합·보안·회귀 테스트와 Swagger 실행 검증이 통과하는 상태입니다. 현재 Work 환경에서는 loopback 소켓 권한으로 HTTP 실행이 막혀 서비스·암호·상태 전이 테스트까지 확인했으며 HTTP·Swagger 증거는 환경 미검증으로 남깁니다.

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
- SQLite 개발·검증 환경은 네 데이터베이스를 함께 백업·복구하는 런북과 실제 콘텐츠 복구 회귀 테스트를 유지하며, 운영 DB 전환 후 같은 절차를 공급자별 방식으로 재검증합니다.
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

## 2026-11-10 최신 실행 기준

이번 실행의 검증 대상 `main`은 PR #132 병합 커밋 `8a82bf9f2a6546c593e44928d588f12d2fa2126c`입니다. PR #132 `docs: 최신 main CI 실패 상태 기록`의 head `451a3db88a93bfbcfbdf72ccb067e55813829ccd`는 해당 커밋으로 squash merge되었습니다. PR #128 병합 후 main CI Run #876, PR #129 병합 후 main CI Run #878, PR #130 병합 후 main CI Run #880, PR #131 자체 CI Run #881, PR #132 자체 CI Run #884와 병합 후 main CI Run #885는 Restore·Build·Test를 성공했습니다. PR #131 병합 직후의 Run #882와 PR #132 이전 head의 Run #883은 Job 생성 전 GitHub Actions `Internal server error`였지만, Run #884·#885에서 동일 변경의 검증을 성공으로 재확인했습니다. PR #132의 로컬·원격 작업 브랜치는 삭제되었고, 해당 검증 시점의 원격 작업 브랜치는 `main`만 남았습니다. PR #128 변경 브랜치에서 전체 Test 232개 통과·실패 0개·건너뜀 0개를 확인했으며, Run #885도 동일한 Restore·Build·Test 단계를 성공했습니다. 이 문서 앞부분의 이전 SHA·테스트 수·CI 번호는 당시의 역사적 실행 기록이며, 현재 main의 최신 SHA는 병합 후 GitHub 기록을 기준으로 확인합니다.

이번 검증으로 로컬 SQLite·Migration·Swagger/OpenAPI 계약, Platform·DataSet·Worker API 통합 흐름, Tenant·인증 경계와 Worker 장애·복구 자동 검증을 현재 내부 범위의 기준으로 고정합니다. 다음 구현은 승인된 DuckDB·Parquet·Commercial·WPF 계약을 먼저 처리하고, 그 뒤 승인된 발급 규칙으로 LicenseAuthority와 개발 라이선스 1개를 구현합니다. 운영 DB·실제 비운영 IdP·외부 시스템·GitHub main 보호 규칙은 해당 환경과 권한이 제공된 뒤 검증합니다.

## 2026-11-10 이전 실행 기준 기록

최신 `main`의 기준 SHA는 `99ede108a165764e7f6c57a7dfe80ef678ad5ea3`입니다. PR #124 병합 후 열린 PR은 없고, 원격 작업 브랜치는 `main`만 남아 있습니다. 병합 후 자체 호스팅 Windows CI Run #867은 Restore·Build·Test를 모두 성공시켰습니다.

```text
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release --logger "trx;LogFileName=test-results-42661262730.trx"
```

현재 Work에서도 최신 main 기준으로 같은 대상 명령을 실제 실행하여 Restore 성공, Release Build 경고 0개·오류 0개, 전체 Test 231개 통과·실패 0개·건너뜀 0개를 재확인했습니다. 별도 임시 SQLite 네 개로 실제 ApiHost를 기동하여 `/health`, `/health/database`, `/openapi/v1.json`, `/swagger/index.html`, Development Bypass 보호 API가 모두 HTTP 200을 반환했고 stderr는 0바이트였습니다. 실제 WorkerHost도 Migration·Handler catalog 동기화 후 `/health` HTTP 200과 stderr 0바이트를 반환했습니다. 이 결과로 로컬 SQLite·Migration·Swagger/OpenAPI 계약, Platform·DataSet·Worker API 통합 흐름, Tenant·인증 경계와 Worker 장애·복구 자동 검증을 현재 내부 범위의 기준으로 고정합니다.

다음 진행은 아래 순서를 유지합니다.

1. DuckDB·Parquet·Commercial·WPF의 승인된 계약과 수용 기준이 제공되면 먼저 해당 비라이선스 기능을 구현하고 검증합니다.
2. 앞 단계가 안정된 뒤 승인된 Licensing 발급 규칙으로 별도 `LicenseAuthority` 솔루션과 개발 라이선스 1개를 구현하고, 발급·서명·만료·폐기·Tenant 범위 테스트를 추가합니다.
3. 개발 라이선스를 API에 연결하여 정상·위조·만료·시작 전·다른 Tenant·폐기·알 수 없는 `KeyId`를 검증합니다. 이 단계로 저장소 내부 구현을 닫습니다.
4. 운영 DB, 실제 비운영 IdP, 외부 시스템, 배포·백업·성능·보호 규칙은 해당 환경과 권한이 제공된 뒤 별도로 실행합니다.

실제 비운영 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한과 GitHub main 보호 규칙 상세 HTTP 403은 이 문서의 내부 성공 증거로 대체하지 않습니다. 이 외부 조건은 Issue #84와 Milestone #1에서 별도 추적하며, 제공되기 전에는 개발 완료로 판정하지 않습니다.

## 2026-11-10 현재 Work 연속 실행 순서

현재 최신 `main`은 `d973499f8f888bd36862ed7935ac93fb4a79ee21`이며, 병합 후 main CI Run #889가 Restore·Release Build·Release Test를 성공했습니다. 이후 Work에서는 다음 순서를 하나의 연속 작업으로 유지합니다.

1. API 계약을 고정합니다. 요청·응답 DTO, 공통 오류 코드, 입력 상한, 페이징, Idempotency-Key, OpenAPI의 Bearer·`X-Tenant-Id`·재처리 헤더를 실제 엔드포인트와 대조하고, 경쟁 요청까지 통합 테스트합니다.
2. Tenant·인증 경계를 검증합니다. 경로 Tenant, `X-Tenant-Id`, 활성 소속, 관리자·조직·프로젝트 범위를 교차 검사하고, Development Bypass 고정 프로필과 External·운영 fail-closed 회귀를 확인합니다.
3. Worker 장애 흐름을 검증합니다. Lease/Fencing, 체크포인트, 재시도, DLQ, 재처리, 정상 종료 인계, 강제 종료 복구, 외부 전달 응답 유실·중복 방지를 실제 저장소와 프로세스 테스트로 확인합니다.
4. 승인된 비라이선스 계약이 있는 경우에만 DuckDB·Parquet·Commercial·WPF를 구현합니다. 승인 문서가 없으면 제안 문서를 계약으로 승격하지 않고 해당 범위를 환경·요구 승인 대기로 기록합니다.
5. 앞 단계가 모두 통과한 뒤 Licensing 규칙을 확정하고 별도 LicenseAuthority와 개발 라이선스 1개를 구현합니다. 라이선스는 마지막 내부 구현 단계로 유지합니다.
6. 개발 라이선스 적용 후 운영 DB, 실제 비운영 IdP, 외부 수신 시스템, 배포·백업·성능·GitHub 보호 규칙을 별도 환경에서 검증합니다.

각 단계에서 중단되면 원인 재현, 로그·코드·설정 근거, 근본 원인과 대안, 최소 수정, 대상·전체 회귀 테스트, 재발 방지책, 증거 기록을 끝낸 뒤 다음 단계로 복귀합니다. 외부 권한·비용·서비스 계정이 필요한 항목은 성공으로 표시하지 않고 재개 조건만 기록합니다.

## 2026-11-10 현재 main과 연속 실행 기준 갱신

PR #135 `fix: 외부 전달 멱등성 경쟁 조건 보강`이 `ce460ec7f3c4efcc54b4da5297879149b2f52b97`로 `main`에 Squash Merge되었다. PR 자체 CI Run #890과 병합 후 main CI [Run #891](https://github.com/namanvarava/Edan.Tensor/actions/runs/42681139618)은 자체 호스팅 Windows Runner에서 Restore·Release Build·Release Test를 모두 성공했다.

현재 Work에서는 다음 순서를 한 번의 연속 작업으로 유지한다.

1. API 계약, 오류 코드, 입력 상한, 페이징, Idempotency-Key, OpenAPI 헤더와 경쟁 요청을 고정하고 회귀 검증한다.
2. Tenant 경로·`X-Tenant-Id`·활성 소속·조직·프로젝트·관리자 권한을 교차 검증하고, Development Bypass와 External·운영 fail-closed를 분리 검증한다.
3. Worker Lease/Fencing, 체크포인트, 재시도, DLQ, 재처리, 종료 복구, 외부 전달의 응답 유실·중복·DB 유일성 경쟁을 검증한다.
4. 승인된 DuckDB·Parquet·Commercial·WPF 계약과 수용 기준이 제공된 경우에만 구현한다. 현재 저장소에는 해당 업무 계약이 없으므로 임의 구현하지 않고 승인 대기로 유지한다.
5. 비라이선스 내부 범위가 안정된 뒤에만 Licensing 규칙을 확정하고 별도 LicenseAuthority와 개발 라이선스 1개를 구현한다. 라이선스는 마지막 내부 구현 단계다.
6. 개발 라이선스 적용 후 운영 DB·실제 비운영 IdP·외부 수신 시스템·배포·백업·성능·GitHub 보호 규칙을 제공된 환경에서 검증한다.

PR #135에서 보강한 저장 경계는 동일 Tenant와 Idempotency Key의 동시 생성 시 커밋된 기존 전달을 반환하고, 요청이 실제로 생성한 레코드에만 API 보상 삭제를 적용한다. 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한과 GitHub main 보호 규칙 HTTP 403은 Issue #84 및 Milestone #1의 외부 조건으로 남아 있으며 이 내부 CI 성공으로 대체하지 않는다.

## 2026-11-10 현재 main 증거 갱신

PR #137은 API 사용 가이드와 실제 Bypass 고정 프로필의 정합성을 맞췄고, PR 자체 CI Run #894와 병합 후 main CI Run #895를 성공시켰다. PR #138은 외부 IdP 검증 런북에서 폐기된 Bypass 설정명을 제거했고, PR 자체 CI Run #896과 병합 후 main CI Run #897을 성공시켰다. 최신 main은 `6a5ff9e7227ace12e074a9d13759ac2403f59aa6`이며 작업 브랜치는 로컬·원격에서 삭제되어 `main`만 남았다.

이 변경은 제품 실행 경계를 확장하지 않는다. Development Bypass의 고정 시뮬레이션 프로필은 실제 외부 IdP 검증을 대체하지 않으며, External·운영 환경은 실제 Metadata·JWKS·Issuer·Audience·HTTPS 검증을 계속 요구한다. 다음 구현은 승인된 비라이선스 계약이 제공될 때만 진행하고 Licensing은 마지막 내부 단계로 유지한다.

## 2026-11-10 PR #139 병합 후 Release 재검증

PR #139는 `a0c990aca0ee3466701e4ccee21bc1c61815e956`로 `main`에 병합되었고, PR CI Run #898과 main CI Run #899가 Restore·Release Build·Release Test를 성공시켰다. 최신 main에서 동일한 세 명령을 권한 확장 환경으로 재현하여 Restore 성공, Release Build 경고 0개·오류 0개, 전체 Release Test 233개 통과·실패 0개·건너뜀 0개를 확인했다. 최초 제한 환경의 Restore 중단은 .NET SDK 10.0.402가 Service Control Manager를 열지 못한 Win32 오류 5였으며, 소스 오류가 아니므로 권한 확장 재검증으로 해결하고 근거를 기록했다. 다음 단계는 API 계약·Tenant/인증·Worker 장애 복구 순서를 유지하며, 승인되지 않은 기능을 임의로 추가하지 않는다.

최신 main의 직접 Host Smoke도 임시 SQLite 4개로 ApiHost와 WorkerHost를 기동하여 health·database health·OpenAPI·Swagger·Development Bypass Tenant API의 HTTP 200을 확인했다. 종료 후 프로세스·포트·임시 데이터가 남지 않았고, `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`는 알려진 취약 패키지 없음을 반환했다. 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 이 내부 증거에 포함하지 않는다.

GitHub API의 main 보호 규칙 조회는 HTTP 403 `Resource not accessible by integration`으로 거부되었다. 따라서 보호 규칙은 성공으로 표시하지 않고 Issue #84의 외부 권한 재개 조건으로 유지한다.

## 2026-11-10 외부 전달 loopback 보안 경계 보강

현재 Work의 내부 보안 감사에서 `ExternalDelivery:AllowLoopback=true`가 문서상 Development 전용이지만 운영 설정으로도 전달될 수 있는 경로를 확인했다. 두 Host가 공통 옵션 생성 검증을 사용하도록 수정하여 Development가 아닌 환경에서는 호스트 시작을 중단하고, Development의 격리된 로컬 수신기 장애 검증은 유지한다. 대상 테스트 7개와 전체 Release Test 235개가 통과했으며, PR Check와 main 병합 후 CI 성공 전에는 통합 완료로 판정하지 않는다.

## 2026-11-10 PR #143 병합 후 완료 증거

PR #143은 `9c6322dc4782b45d8eb7d25a1890d6aeceb08c33`으로 main에 squash merge되었고, PR CI Run #906과 병합 후 main CI Run #907이 성공했다. 병합된 main에서 Release restore·build·test를 다시 실행하여 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 235개 통과·실패 0개·건너뜀 0개를 확인했다. 실제 Development Smoke에서도 임시 SQLite와 Swagger/OpenAPI를 포함한 ApiHost·WorkerHost 흐름이 HTTP 200을 반환했고, Bypass는 격리된 개발 환경에서만 사용했다. Smoke 종료 후 stderr·프로세스·포트·임시 경로가 남지 않았다.

PR #143 작업 브랜치는 원격 삭제와 GitHub API 404를 확인한 뒤 로컬에서도 삭제했으며, 로컬·원격 `main`은 동일한 `9c6322dc4782b45d8eb7d25a1890d6aeceb08c33`을 가리킨다. 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 이 내부 증거로 완료 처리하지 않고 Issue #84와 Milestone #1의 외부 조건으로 유지한다.

## 2026-11-10 Client API 연동 기반 추가

WPF 화면이 없는 현재 저장소에서도 승인된 Platform·DataSet·Worker 흐름을 실제 소비할 수 있도록 `EDAN.Tensor.Client.Contracts`와 `EDAN.Tensor.Client`를 추가했다. Client는 API Host의 Tenant 경로와 `X-Tenant-Id`를 일치시키고, Bearer 토큰·`Idempotency-Key`·`Replay-Request-Id`를 요청 경계에서 구성하며, 표준 `{ Code, Description }` 오류를 `TensorApiException`으로 보존한다. 콘텐츠 업로드·다운로드 스트림과 404 선택 조회도 계약으로 고정했다.

Client 대상 회귀 6개와 실제 임시 SQLite API Host 통합 1개를 포함한 솔루션 전체 Release Test 242개가 실패 0개·건너뜀 0개로 통과했다. 이 구현은 WPF 화면, 결제·Commercial entitlement, Licensing 발급·검증, DuckDB·Parquet 어댑터를 임의로 확장하지 않으며 해당 계약이 제공될 때 같은 Client 경계 위에 추가한다.

## 2026-11-10 DataSet Validation Worker 실행 경계 보강

기존 API 통합 테스트는 DataSet Validation 실행을 Queue 상태까지 확인했으므로, 실제 `WorkerTaskExecutionCoordinator`와 `DataSetValidationTaskHandler`가 API가 예약한 불투명 Payload Reference를 처리하는 경계를 같은 임시 SQLite Host 구성에서 추가 검증했다. 테스트는 Lease/Fencing Token으로 작업을 시작하고, 중복 행이 포함된 CSV를 검증하여 Worker 작업은 `Succeeded`, 검증 결과는 `Invalid`, 처리 행 수는 2건으로 저장되는지 확인한다.

첫 통합 시도에서는 테스트 프로젝트에 WorkerHost를 직접 참조하면서 API Host와 WorkerHost의 전역 `Program` 타입이 충돌하는 CS0433 컴파일 오류가 발생했다. 별도 테스트 프로젝트로 분리하는 대안보다 변경 범위가 작은 프로젝트 참조 별칭을 선택하여 WorkerHost Handler만 명시적으로 참조했고, 대상 통합 테스트 33개와 독립 경계 테스트 1개가 통과했다. 솔루션 전체 Release Build는 경고 0개·오류 0개, 전체 Release Test는 242개 통과·실패 0개·건너뜀 0개로 통과했다.

## 2026-11-10 API·Tenant·Worker 내부 범위 최종 증거 갱신

이 Work에서 승인된 현재 내부 범위의 남은 경계를 다음 순서로 확인했다.

1. PR #148 `test: DataSet 검증 결과 경계 회귀 보강`은 `a736bbb2399ac1e7ee6a12d8e11cea87b6a420c5`로 병합했다. 실제 Worker가 완료한 검증의 상세 조회가 200이며 원문 CSV 값이 오류 메시지에 노출되지 않고, 관리자의 명시적 삭제 뒤 조회가 404가 되는 경계를 통합 테스트로 확인했다. PR CI Run #916과 병합 후 main CI Run #917이 성공했다.
2. PR #149 `fix: DataSet 검증 Handler 취소 경계 연결`은 `76f3f54f652bec71431c56ed8daf5a6448245752`로 병합했다. 조정기의 취소 토큰을 실제 Handler 실행에 연결하고 시작 전 취소를 검사하도록 최소 수정했으며, 실제 WorkerHost Handler의 취소 회귀 테스트를 추가했다. PR CI Run #918과 병합 후 main CI Run #919가 성공했다.
3. PR #150 `test: DataSet 입력 revision 변경 경계 검증`은 `2eb02fa46aae48eb5c10c97abdcaf2161c5c1c13`으로 병합했다. 예약 뒤 Schema revision이 변경되면 실제 Worker 실행이 `dataset.validation.input_changed`로 검증을 `Failed`에 닫는 경계를 확인했다. 첫 실패는 제품 오류가 아니라 후속 요청이 변경된 DataSet 버전 4 대신 버전 3을 기대한 테스트 시나리오 결함이었고, 원인 확인 후 기대값만 최소 수정했다. PR CI Run #920과 병합 후 main CI Run #921이 성공했다.

PR #150 병합 후 main에서 다음 명령을 다시 실행했다.

```text
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

Restore는 성공했고, Release Build는 경고 0개·오류 0개, 전체 Release Test는 242개 통과·실패 0개·건너뜀 0개였다. `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`도 알려진 취약 패키지가 없음을 반환했다.

실제 Release Host Smoke는 샌드박스 외부의 loopback 권한을 허용한 격리 실행에서 임시 SQLite 네 개로 확인했다. ApiHost의 `/health`, `/health/database`, `/openapi/v1.json`, `/swagger/index.html`, `/api/platform/tenants/mine`과 WorkerHost의 `/health`가 모두 HTTP 200을 반환했고, 두 Host의 표준 오류 출력은 0바이트였다. 첫 실행의 Worker `worker_task_types` 누락은 WorkerHost에 `Database:ApplyMigrationsInDevelopment=true`를 전달하지 않은 검증 설정 결함으로 재현·분류했으며, 설정을 보정한 재실행에서 해결했다. 검증 종료 후 프로세스·포트·임시 SQLite·키·아카이브 경로가 남지 않았다.

따라서 현재 main `2eb02fa46aae48eb5c10c97abdcaf2161c5c1c13`에서 로컬 SQLite·Migration·Swagger/OpenAPI, Platform·DataSet·Worker API 흐름, Tenant·개발 인증 경계, Worker Lease/Fencing·취소·입력 revision 변경·결과 보존 경계의 내부 증거를 확보했다. Licensing·LicenseAuthority는 사용자의 지시에 따라 마지막 내부 단계로 남겨 두었으며, DuckDB·Parquet·Commercial·WPF는 승인된 계약과 수용 기준이 없으므로 임의 구현하지 않는다.

실제 비운영 외부 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전 권한, 운영 DB·외부 수신 시스템·배포 환경과 GitHub main 보호 규칙 HTTP 403은 이 내부 증거로 대체하지 않는다. 해당 항목은 외부 조건이 제공될 때 별도 런북과 권한으로 재개한다.

## 2026-11-10 최종 main과 Client 삭제 계약 증거

PR #153은 `7b1ca7a7fdaa1bd2fc6a760e153f53b8896d27b5`로 Squash Merge되었고, 기존 승인된 DataSet 검증 삭제 API를 `TensorApiClient.DeleteDataSetValidationAsync`와 단위·실제 API Host 통합 회귀에 연결했다. PR CI [Run #926](https://github.com/namanvarava/Edan.Tensor/actions/runs/42715584623)과 병합 후 main CI [Run #927](https://github.com/namanvarava/Edan.Tensor/actions/runs/42715896376)이 성공했다.

PR #154는 최신 검증 증거를 문서에 반영하여 `5fe4b343928edd70f3319d31bba6d430e13b6a3d`로 Squash Merge되었다. PR CI [Run #928](https://github.com/namanvarava/Edan.Tensor/actions/runs/42716886955)과 병합 후 main CI [Run #929](https://github.com/namanvarava/Edan.Tensor/actions/runs/42717234264)이 자체 호스팅 Windows Runner에서 성공했다. 최종 main에서 `dotnet restore EDAN.Tensor.sln`, `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`, `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`를 다시 실행하여 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 243개 통과·실패 0개·건너뜀 0개를 확인했다.

이 증거는 현재 승인된 로컬 SQLite·Swagger/OpenAPI, Platform·DataSet·Worker, Tenant·인증 경계, Worker 복구, typed Client 기반 범위에 한정한다. DuckDB·Parquet·Commercial·WPF와 Licensing·LicenseAuthority 업무 기능은 승인된 계약과 수용 기준이 제공되기 전까지 구현 완료로 표시하지 않으며, 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 Issue #84의 외부 조건으로 유지한다.

## 2026-11-10 PR #155 병합 후 최신 기준

PR #155는 문서의 테스트 구성과 최신 main 증거가 이전 SHA를 현재 기준처럼 가리키던 정합성 결함을 수정하여 `8139a07ebdbe85bb42acdabbc8a58ad1bb4983e9`로 Squash Merge되었다. PR CI [Run #930](https://github.com/namanvarava/Edan.Tensor/actions/runs/42718223926)과 병합 후 main CI [Run #931](https://github.com/namanvarava/Edan.Tensor/actions/runs/42718607611)이 성공했다. 병합 후 로컬에서 Release Restore·Build·Test를 다시 실행하여 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 243개 통과·실패 0개·건너뜀 0개를 확인했다. PR #155 작업 브랜치는 병합 후 로컬·원격에서 삭제했고, 현재 내부 구현 범위와 외부 미검증 조건은 변경하지 않았다.

## 2026-11-10 PR #157 OpenAPI 계약 회귀 보강

PR #157은 승인된 Platform·DataSet·Validation·사용량·Worker·외부 전달 핵심 경로가 OpenAPI 문서에 계속 노출되는지와 Tenant 헤더, `Idempotency-Key`, `Replay-Request-Id`, Bearer 보안 요구가 유지되는지를 통합 테스트로 보강하여 `a379c6077b671e27f74dda267ae3715dfc374e0c`로 Squash Merge되었다. PR CI [Run #934](https://github.com/namanvarava/Edan.Tensor/actions/runs/42721451802)와 병합 후 main CI [Run #935](https://github.com/namanvarava/Edan.Tensor/actions/runs/42721752528)이 성공했다. 병합 후 로컬 Release Restore·Build·Test는 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 243개 통과·실패 0개·건너뜀 0개를 기록했다. 이 변경은 제품 실행 경계를 확장하지 않으며 승인되지 않은 DuckDB·Parquet·Commercial·Licensing·WPF와 외부 환경 검증은 계속 별도 조건으로 남긴다.

PR #159는 외부 전달 Handler가 조정기의 취소 토큰을 실제 HTTP 실행에 연결하도록 수정하고 시작 전 취소·진행 중 호출 취소 전파 회귀 테스트를 추가하여 `52fa640cb833caff05947b35f3239ad71d11f4a4`로 Squash Merge되었다. PR CI [Run #938](https://github.com/namanvarava/Edan.Tensor/actions/runs/42724689384)와 병합 후 main CI [Run #939](https://github.com/namanvarava/Edan.Tensor/actions/runs/42724934985)이 성공했다. PR #160은 이 수정의 근본 원인·수용 범위·최종 검증 결과를 문서에 기록하여 `b789c4740ec289fd4cf7ce404c222ae1e6cde8a1`로 Squash Merge되었고, PR CI [Run #940](https://github.com/namanvarava/Edan.Tensor/actions/runs/42725467593)와 병합 후 main CI [Run #941](https://github.com/namanvarava/Edan.Tensor/actions/runs/42725743165)이 성공했다. 최신 main의 Release Restore·Build·Test는 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 245개 통과·실패 0개·건너뜀 0개를 기록했다. 실제 외부 수신 시스템·외부 IdP·운영 DB·GitHub 보호 규칙 검증은 이 내부 증거에 포함하지 않는다.

## 2026-11-10 PR #162 저장량 조회 개수 계약 보강

PR #162는 저장량 스냅샷 이력과 quota 임계치 이벤트 API가 범위를 벗어난 `take` 값을 조용히 보정하던 계약 불일치를 수정했다. 저장량 이력은 1~744, quota 이벤트는 1~500 범위로 명시적으로 검증하고, 범위를 벗어나면 HTTP 400 `api.invalid_request`를 반환한다. Tenant 관리자 경계에서 네 가지 잘못된 입력을 확인하는 API 통합 회귀 테스트와 공개 API·운영 가이드를 추가했다.

PR #162의 head `61483289fd4319aba1ad69c7f482d31b30aab9e7`에 대한 [PR CI Run #944](https://github.com/namanvarava/Edan.Tensor/actions/runs/42730316819)와 Squash Merge 후 main 커밋 `21ecd234e2dda4e34ab896bd36f3b96be2e07085`의 [main CI Run #945](https://github.com/namanvarava/Edan.Tensor/actions/runs/42730617911)이 Restore·Release Build·Release Test를 성공했다. 병합 후 로컬에서도 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 확인했다. PR #162 작업 브랜치는 병합 후 로컬·원격에서 삭제했다.

이 변경은 승인된 로컬 SQLite·Swagger/OpenAPI·Platform·DataSet·Worker API 계약의 입력 경계를 보강한 범위다. DuckDB·Parquet·Commercial·Licensing·LicenseAuthority·WPF 업무 기능은 승인된 계약과 수용 기준이 없으므로 구현 완료로 표시하지 않는다. 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 Issue #84의 외부 조건으로 유지한다.

## 2026-11-10 PR #164 Client 페이지 계약 정합성 보강

서버의 DataSet 검증 상세 `take` 상한 500과 Worker DLQ `pageSize` 상한 100을 typed Client의 선행 검증과 일치시켰다. `take=501`과 `pageSize=101`의 API HTTP 400 오류 코드, Client의 잘못된 입력 사전 차단, 한국어 XML·운영 가이드 설명을 회귀 검증했다.

PR #164의 head `89a62cd526ab98667f4d75f4c340499b96bbdb21`에 대한 [PR CI Run #948](https://github.com/namanvarava/Edan.Tensor/actions/runs/42733699062)와 Squash Merge 후 main 커밋 `f2d303c607b21b418e98f688000262441f48d837`의 [main CI Run #949](https://github.com/namanvarava/Edan.Tensor/actions/runs/42733986905)이 Restore·Release Build·Release Test를 성공했다. 병합 후 로컬 Release Restore·Build·Test도 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 기록했고, 작업 브랜치는 로컬·원격에서 삭제했다. 이 증거는 승인된 내부 SQLite·Swagger/OpenAPI·Platform·DataSet·Worker 범위에 한정하며 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 검증을 포함하지 않는다.

## 2026-11-10 PR #166 DLQ 페이지 오프셋 정합성 보강

Worker 서버의 checked 페이지 오프셋 계산과 typed Client의 입력 검증을 일치시켰다. `page=int.MaxValue`와 기본 `pageSize=50`처럼 서버가 `worker.dead_letter.invalid_page`로 거부할 조합을 Client가 HTTP 요청 전에 차단하며, XML 문서·운영 가이드와 회귀 테스트를 함께 갱신했다.

PR #166의 head `c57952206fca5e161a8c392911f45cac82e1bc57`에 대한 [PR CI Run #952](https://github.com/namanvarava/Edan.Tensor/actions/runs/42736302956)와 Squash Merge 후 main 커밋 `7be3d93fbfc56ed2767ea985ad5b59ece30af2e7`의 [main CI Run #953](https://github.com/namanvarava/Edan.Tensor/actions/runs/42736549387)이 Restore·Release Build·Release Test를 성공했다. 병합 후 로컬 Release Restore·Build·Test는 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 기록했다. 이 증거는 승인된 내부 범위에 한정하며 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 검증을 포함하지 않는다.

## 2026-11-10 PR #168 DLQ 페이지 오버플로 API 회귀

API Host 통합 테스트에서 `page=int.MaxValue&pageSize=100`을 호출하여 Worker 서버의 checked 페이지 오프셋 오버플로가 HTTP 400 `worker.dead_letter.invalid_page`로 반환되는지 확인했다. Client 단위 검증과 서버 통합 검증을 함께 유지하여 동일 입력이 어느 경계에서도 정상 데이터 조회로 처리되지 않도록 했다.

PR #168의 head `cdf1d7f34b8af7679bd1fd62b81d8f3c8d8a3639`에 대한 [PR CI Run #956](https://github.com/namanvarava/Edan.Tensor/actions/runs/42738402094)와 Squash Merge 후 main 커밋 `bd4753f0c4ba41d906d6ca8556257eb3765db04d`의 [main CI Run #957](https://github.com/namanvarava/Edan.Tensor/actions/runs/42738642780)이 Restore·Release Build·Release Test를 성공했다. 병합 후 로컬 Release Restore·Build·Test는 Restore 성공, Build 경고 0개·오류 0개, 전체 Test 246개 통과·실패 0개·건너뜀 0개를 기록했다. 이 증거는 승인된 내부 범위에 한정하며 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 검증을 포함하지 않는다.

## 2026-11-10 최신 제품 코드 기준 Host Smoke·의존성·GitHub 상태 재검증

PR #169가 병합한 제품 코드 기준 `5033716067358350f3f86cc1910c8135f1c81951`의 Release ApiHost와 WorkerHost를 임시 SQLite 네 개로 실제 기동했다. ApiHost의 health·database health·OpenAPI·Swagger UI·Development Bypass Tenant API와 WorkerHost health는 모두 HTTP 200을 반환했고, 두 Host의 표준 오류 출력은 0바이트였다. 검증 후 임시 프로세스와 데이터 경계를 정리했다. 이후 PR #170은 이 증거 문서만 변경하여 `8b96528c58be3b8fcbe0e3fc3abfcd1b4856e11c`로 병합했으며 제품 코드는 변경하지 않았다.

`dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive`는 모든 프로젝트에 알려진 취약 패키지가 없음을 반환했다. 열린 Pull Request는 0개이고 원격 브랜치는 `main`만 남았다. Issue #84의 실제 외부 IdP·GitHub main 보호 규칙 HTTP 403은 외부 조건으로 계속 미검증이다.

## 2026-11-10 재현 가능한 Host Smoke 도구와 중단 복구

수동 Host Smoke 절차의 재현성과 정리 증거를 고정하기 위해 `tools/host-smoke.ps1`을 추가했다. Windows PowerShell 5.1에서 발생한 `ArgumentList` 부재와 `Kill(bool)` 오버로드 부재를 각각 `ProcessStartInfo.Arguments`와 `Process.Kill()`로 수정했으며, 구문 분석·실제 Host Smoke·Release 전체 검증으로 재확인했다. 최종 Smoke는 ApiHost·WorkerHost의 필수 HTTP 경계를 모두 200으로 확인하고 stderr 0바이트와 임시 경계 삭제를 확인한다.

## 2026-11-10 WorkerHost 데이터베이스 health 경계 보강

기존 WorkerHost `/health`가 프로세스 생존만 보고하던 공백을 확인했다. 공통 `SqliteDatabaseHealthProbe`를 HostServices로 이동하고 WorkerHost에 `/health/database`를 추가하여 Platform·Worker·DataSet·DataSetContent 네 저장소의 연결과 기본 스키마 읽기를 함께 확인한다. 하나라도 실패하면 `Disconnected/Red`와 HTTP 503을 반환하며, 작업 재개 전에 저장소 상태를 조사할 수 있도록 운영 가이드와 Host Smoke에 이 경계를 연결했다.

WorkerHost 전용 임시 SQLite 통합 테스트 2개는 정상 연결과 콘텐츠 DB 열기 실패를 각각 확인했고, 솔루션 Release Restore·Build·Test(전체 248개, 실패 0개·건너뜀 0개)와 실제 Host Smoke를 재실행했다. Smoke는 두 Host의 health·database health, OpenAPI, Swagger UI, Development Bypass Tenant API에서 HTTP 200을 반환했고 stderr 0바이트와 임시 디렉터리 정리를 확인했다. 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403과 Licensing은 이 단계의 완료 범위에 포함하지 않는다.
PR #175 자체 CI [Run #42748810618](https://github.com/namanvarava/Edan.Tensor/actions/runs/42748810618)와 병합 후 main CI [Run #42749039001](https://github.com/namanvarava/Edan.Tensor/actions/runs/42749039001)이 성공했고, main 병합 커밋은 `212f5b64a6d27ecd4ea21cb5f3597b1a461a5cf2`다. 병합 후 동일한 Release Restore·Build·Test와 Host Smoke를 재실행해 같은 결과를 확인했다.

WorkerHost health 통합 테스트의 임시 DB 정리 순서도 보강했다. `using var` 자원이 `finally` 이후까지 살아 있을 수 있어 Windows 파일 잠금으로 삭제가 실패할 가능성이 있었으며, 지연·재시도 대신 테스트 블록 안에서 Host·Client·응답을 먼저 폐기하도록 수정했다. 정상·장애 경계 2개를 다시 통과시켜 중단 원인과 정리 경계를 고정했다.

## 2026-11-10 PR #177 병합 후 최신 내부 검증 기준

PR #177은 위 테스트 정리 순서를 보강한 변경이며, PR CI [Run #42750885076](https://github.com/namanvarava/Edan.Tensor/actions/runs/42750885076) 성공 후 `f4c54b8f294b0845dabf62f689a3a985c4b567af`로 `main`에 Squash Merge되었다. 병합 후 main CI [Run #42751232084](https://github.com/namanvarava/Edan.Tensor/actions/runs/42751232084)도 성공했다.

실제 호스트의 Release Restore·Build·Test는 각각 성공, 경고 0개·오류 0개, 248개 통과·실패 0개·건너뜀 0개를 기록했다. Host Smoke는 임시 SQLite 네 개와 Development Bypass를 사용하여 ApiHost·WorkerHost의 health·database health, OpenAPI, Swagger UI, Tenant API를 HTTP 200으로 확인하고 stderr와 임시 데이터를 정리했다.

제한된 Work 실행 경계의 병렬 MSBuild와 VSTest testhost 연결 실패는 외부 실행 경계에서 동일 커밋을 성공시켜 환경 문제로 분리했다. 이 현상은 제품 결함이 아니며, 복원·빌드가 같은 환경에서 중단되면 직렬 MSBuild 또는 외부 실행 경계에서 원인을 재확인한다. 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403과 Licensing은 Issue #84 및 Milestone #1의 별도 외부 조건으로 남긴다.

## 2026-11-10 DataSet 검증 Idempotency 보상 경계 보강

동시 DataSet 검증 요청에서 저장 전 Idempotency-Key 조회가 비어 있어도 저장소가 유일 키 충돌을 기존 실행으로 수렴시킬 수 있다. 기존 API는 저장 전 조회 결과만 기준으로 새 실행을 표시했기 때문에 Worker 작업 등록이 실패하면 다른 요청이 생성한 실행을 내부 보상 삭제할 가능성이 있었다.

저장소 생성 결과에 `Created` 여부를 추가하고 기존 Snapshot 메서드는 호환 래퍼로 유지했다. API는 실제 생성한 실행만 `DeleteUnqueuedExecutionAsync`의 보상 대상으로 지정한다. 대상 회귀 1개와 전체 Release Test 249개, Build 경고 0개·오류 0개, Host Smoke를 통과했다. 이 변경은 기존 Idempotency-Key·Tenant·Worker 계약을 유지하면서 동시 보상 삭제의 근본 원인을 제거한다.

## 2026-11-10 PR #180 병합 후 내부 범위 기준 갱신

PR #180은 `da405a5b9874e2df57f88f829cd38c7c5e172fca`로 main에 Squash Merge되었고, PR CI [Run #42756936154](https://github.com/namanvarava/Edan.Tensor/actions/runs/42756936154)와 병합 후 main CI [Run #42757143488](https://github.com/namanvarava/Edan.Tensor/actions/runs/42757143488)가 성공했다. 병합 후 최신 main에서 지정된 Release Restore·Build·Test를 실제 실행하여 Restore 성공, Build 경고 0개·오류 0개, Test 249개 통과·실패 0개·건너뜀 0개를 확인했다. Host Smoke는 ApiHost·WorkerHost의 health·database health, OpenAPI, Swagger UI, Development Bypass Tenant API를 모두 HTTP 200으로 반환했고 stderr 0바이트와 임시 DB 정리를 확인했다. 직접·전이 패키지 취약점 감사도 취약 패키지 없음으로 끝났다.

현재 내부 범위는 로컬 SQLite·Migration·Swagger/OpenAPI, Platform·DataSet·Worker API 흐름, Tenant·개발 인증 경계, Worker Lease/Fencing·장애 복구와 typed Client 기반까지 병합·검증되었다. PR #180은 동시 Idempotency-Key 충돌에서 실제 생성한 실행만 내부 보상 삭제하도록 저장소 결과 계약을 보강했다. DuckDB·Parquet·Commercial·WPF는 승인된 업무 계약이 없고, Licensing·LicenseAuthority는 사용자가 지정한 마지막 내부 단계이지만 발급 규칙·서명 포맷·키 보관·폐기 계약이 없어 아직 구현하지 않았다. 실제 비운영 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403도 Issue #84와 Milestone #1의 후속 조건으로 유지한다.

## 2026-11-10 승인 라이선스 구현 재개 기준

사용자가 개발·테스트 라이선스 계약을 승인하여 5단계와 6단계의 내부 구현을 진행했다. 별도 `EDAN.Tensor.LicenseAuthority.sln`은 `EDAN-NONPROD`·`EDAN.Tensor`·`Platform/DataSet/Worker`·5년·JWS ES256·RBAC·기본 거부·폐기 규칙을 적용한다. 운영 환경에서 개인키를 자동 생성하지 않고 개발 Tenant 라이선스를 검증하지 않도록 정책을 추가했다.

별도 솔루션 Release Restore·Build·Test는 각각 성공, 경고 0개·오류 0개, 9개 통과·실패 0개·건너뜀 0개를 기록했다. 전체 솔루션의 최종 Release 검증은 Restore 성공, Build 경고 0개·오류 0개, 258개 통과·실패 0개·건너뜀 0개를 기록했다. 권한이 허용된 실행 환경에서 LicenseAuthority HTTP 스모크와 기존 Host Smoke도 성공했으며, 제한된 Work 실행 경계에서만 loopback·testhost 접근 제한이 재현되었다.

## 2026-11-10 Commercial 결제 경계 보강

승인된 계약은 결제 공급자를 아직 선정하지 않았으므로 상품 가격, 실제 결제 호출, Webhook, 환불, 유료 권리를 성공으로 처리하는 업무 구현을 허용하지 않는다. 기존 Commercial 프로젝트가 AssemblyMarker만 제공하여 이 fail-closed 규칙을 코드로 보장하지 못하던 원인을 확인했다.

`IPaymentProvider`와 `PaymentProviderGateway`를 추가하여 공급자를 교체 가능한 경계로 분리했다. 공급자가 없으면 `commercial.payment_provider_not_selected` 오류를 반환하고, 공급자가 선정된 경우에만 공급자 중립 요청을 전달한다. 미선정 거부와 선정 공급자 위임을 각각 회귀 테스트로 고정했으며, 가격·실제 결제·Webhook·환불·entitlement는 구현하지 않았다.

현재 내부 개발 순서는 DuckDB·Parquet 계약 승인 및 구현, Commercial 업무 계약·공급자 선정 후 어댑터 연결, WPF 화면·업무 흐름 계약 후 클라이언트 구현, 마지막으로 실제 외부 IdP·운영 DB·외부 수신 시스템·배포·GitHub 보호 규칙 검증이다. LicenseAuthority 개발 라이선스와 내부 검증은 이미 별도 승인 계약에 따라 완료했지만, 운영 라이선스와 외부 환경 검증 완료를 의미하지 않는다.

## 2026-11-10 최신 main 문서·테스트 기준

PR #190은 최신 main 검증 증거를 갱신했고 `c46b16e43eee8e7b94653329efbec41d44c2727e`로 main에 병합되었다. PR CI #42805377218와 병합 후 main CI #42805670430은 Restore·Build·Test를 성공했다. 최신 main의 통합 Release Test는 261개 통과·실패 0개·건너뜀 0개이며, 별도 LicenseAuthority 테스트는 10개 통과·실패 0개·건너뜀 0개다.

현재 저장소 내부에서 계약이 확정된 범위는 로컬 SQLite·Migration·Swagger/OpenAPI, Platform·DataSet·Worker API 흐름, Tenant·인증·인가·Development Bypass 경계, Worker 장애 복구, typed Client, 개발 LicenseAuthority의 서비스·HTTP 검증과 공급자 중립 Commercial 결제 경계다. DuckDB·Parquet의 포맷·스키마·quota·동시성·복구, Commercial 상품·가격·Webhook·환불·유료 권리, WPF 화면·업무 흐름, 제품 API의 라이선스 전달 헤더·검증 실패 응답 계약은 승인 전 구현하지 않는다. 실제 외부 IdP·운영 DB·외부 수신 시스템·GitHub 보호 규칙 HTTP 403은 Issue #84와 Milestone #1의 별도 조건이다.
