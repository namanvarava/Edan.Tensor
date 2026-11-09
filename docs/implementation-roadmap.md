# 전체 구현 진행 방향

이 문서는 EDAN Tensor의 이후 구현 순서와 단계별 완료 기준을 고정합니다. 구현자는 이 순서를 임의로 바꾸지 않으며, 앞 단계의 수용 기준과 증거가 확보된 뒤 다음 단계로 이동합니다. 이 문서는 구현 방향을 기록하는 기준 문서이며, 각 업무 기능의 상세 계약은 해당 단계의 승인 문서와 함께 확정합니다.

## 이전 기준 상태 기록

현재 main은 `99ede108a165764e7f6c57a7dfe80ef678ad5ea3`이며 PR #124 병합 후의 기준입니다. API Host는 Platform·Worker·DataSet 메타데이터·DataSet 콘텐츠를 분리한 SQLite 연결과 EF Core Migration을 사용하고, Development 환경에서 OpenAPI JSON과 Swagger UI를 제공합니다. `/health`와 `/health/database`는 네 개 로컬 데이터베이스의 연결·기본 스키마 읽기를 확인하며, DataSet 검증 예약은 Tenant 범위 Idempotency-Key로 중복 실행을 수렴시킵니다. 공통 예외 처리기는 안정적인 API 오류 코드와 한국어 설명을 반환하고 내부 예외 상세를 외부 응답에서 숨깁니다. Worker 등록 실패 시 External Delivery와 DataSet Validation의 새 미실행 레코드는 내부 보상 조건으로 정리합니다. 저장 용량 정책은 Tenant 관리자 API에서 페이지·변경 이력·낙관적 동시성으로 관리하고, 버전 충돌은 안정적인 도메인 오류 코드로 반환합니다. Development Bypass는 코드에 고정된 비운영 Tenant와 안정적인 `sub`를 사용하며 외부 설정으로 재정의할 수 없습니다. LicenseAuthority의 실제 발급 업무는 아직 구현하지 않았습니다.

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

### 5단계. 별도 LicenseAuthority와 개발 라이선스 1개를 구현합니다.

이 단계에서는 앞 단계의 비라이선스 제품 기능이 안정된 뒤 기존 Licensing 모듈을 복제하지 않고 별도 솔루션에서 발급 Host를 구성합니다.

- `EDAN.Tensor.LicenseAuthority.sln`은 기존 Licensing Contracts·Domain·Application·Infrastructure 프로젝트를 공유합니다.
- LicenseId, Site 또는 Tenant, 제품, 기능, 발급일, 시작일, 만료일, KeyId, 서명과 상태 규칙을 승인된 계약으로 확정합니다.
- 개인키는 발급 프로그램에만 보관하고 API에는 공개키만 배포합니다.
- 초기에는 규칙에 맞는 Development 라이선스 1개를 발급합니다.
- 발급 결과와 키 자료를 저장소·로그에 비밀값으로 기록하지 않습니다.

이 단계의 완료 기준은 발급 프로그램의 정상·오류 테스트와 서명된 개발 라이선스 1개가 계약에 맞게 생성되는 상태입니다.

### 6단계. 라이선스 적용 API를 검증하고 내부 범위를 닫습니다.

이 단계에서는 5단계의 개발 라이선스를 API에 연결합니다.

- API는 공개키로 서명, 제품, Site 또는 Tenant, 기능, 유효 기간과 KeyId를 검증합니다.
- 정상, 위조, 만료, 시작 전, 다른 Tenant, 폐기, 알 수 없는 KeyId를 각각 거부합니다.
- Development 테스트에서는 명시된 개발 설정으로만 라이선스 발급 Host를 대체할 수 있습니다.
- External·운영 환경에서는 라이선스 검증 실패를 허용하지 않고 명확한 오류를 반환합니다.
- 인증 Bypass와 라이선스 검증 Bypass를 서로 다른 경계로 유지합니다.

이 단계의 완료 기준은 라이선스 적용 API의 통합·보안·회귀 테스트와 Swagger 실행 검증이 통과하는 상태입니다.

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
