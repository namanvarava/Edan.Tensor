# 2026-11-11 연속 개발 재검증 기록

이 문서는 현재 Work에서 EDAN.Tensor 개발을 재개한 뒤 `main`의 승인된 저장소 내부 범위를 다시 점검한 기록이다. 검증 기준은 main 커밋 `2f66ab2bf1b4e35f0b7c5341b5feb4dbfe1740e9`이며, PR #213의 문서-only 병합으로 제품 코드는 변경되지 않았다.

## 현재 상태

- 검증 시작 시 로컬과 원격 `main`은 위 커밋을 가리켰고 작업 트리는 깨끗했다.
- 검증 시작 시 열린 Pull Request는 없었고 이전 작업 브랜치는 로컬·원격에서 삭제되었다. 이 기록을 위한 문서 브랜치는 병합 후 삭제한다.
- 승인된 LicenseAuthority, RBAC, SQLite·Swagger/OpenAPI, Platform·DataSet·Worker, Client 계약은 기존 main에 포함되어 있다.
- 제품 ApiHost에 라이선스를 자동 적용하는 헤더·기능 매핑·fail-closed 계약은 별도 승인 전 상태이므로 추가하지 않았다.

## 이번 Work 실행 결과

제한된 Work 실행 경계에서 필수 명령을 다시 실행한 결과는 다음과 같다.

| 명령 | 결과 | 판정 근거 |
| --- | --- | --- |
| `dotnet restore EDAN.Tensor.sln` | 종료 코드 1 | MSBuild 병렬 Restore가 경고·오류 출력 없이 Work의 IPC 경계에서 중단되었다. |
| `dotnet restore EDAN.Tensor.sln -m:1 --verbosity minimal` | 성공 | 모든 프로젝트가 최신 상태이며 종료 코드 0이다. |
| `dotnet build EDAN.Tensor.sln --no-restore --configuration Release` | 종료 코드 1 | 경고 0개·오류 0개 상태에서 병렬 실행 경계가 중단되었다. |
| `dotnet build EDAN.Tensor.sln --no-restore --configuration Release -m:1` | 성공 | 전체 프로젝트가 빌드되었고 경고 0개·오류 0개이다. |
| `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release` | 종료 코드 1 | 제한된 병렬 testhost 실행이 출력 없이 종료되었다. |
| `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release -m:1` | 환경 중단 | 첫 테스트 프로젝트에서 VSTest가 90초 동안 testhost에 연결하지 못했고 다음 프로젝트에서도 같은 현상이 반복되어 실행을 중단했다. |

위 두 Build 실패와 Testhost 연결 실패는 소스 컴파일·테스트 assertion 실패가 아니라 Work 실행 경계 문제다. 동일 원인을 무한 재시도하지 않고 직렬 Build와 권한이 허용된 자체 호스팅 CI를 근거로 분리했다.

## 권한이 허용된 회귀 증거

- PR #213 CI [Run #1048](https://github.com/namanvarava/Edan.Tensor/actions/runs/42862332589)와 병합 후 main CI [Run #1049](https://github.com/namanvarava/Edan.Tensor/actions/runs/42862682305)가 성공했다.
- 최신 제품 Release 회귀는 258개 통과·실패 0개·건너뜀 0개이며, 별도 LicenseAuthority 회귀는 11개 통과·실패 0개·건너뜀 0개이다.
- LicenseAuthority Smoke는 단일 `EDAN-NONPROD` 5년 개발 라이선스, 공개 JWK, 정상 검증 HTTP 200, Production·폐기 검증 HTTP 403, 토큰 미기록과 임시 경로 삭제를 확인했다.
- Host Smoke는 ApiHost·WorkerHost의 health, database health, OpenAPI, Swagger UI와 Development Bypass Tenant API를 HTTP 200으로 확인했으며 표준 오류 출력과 임시 경계를 정리했다.
- `dotnet list EDAN.Tensor.sln package --vulnerable --include-transitive --no-restore --format json`은 NuGet 소스를 조회한 뒤 취약 항목 없이 모든 프로젝트를 반환했다.
- 저장소 Markdown 상대 링크 점검 결과 누락 링크는 0개였다.

## 인증 Bypass 경계

Development Bypass는 `Development` 환경과 `Authentication:Mode=Bypass`에서만 활성화된다. 고정 프로필은 RFC 예약 Metadata/JWKS 주소, `EDAN-NONPROD`, `edan-nonprod-test-user`, `edan-nonprod-test-subject`, 개발 키 회전 권한 식별자를 사용한다. 이 값은 실제 외부 IdP의 증거가 아니며 운영·External·CI·배포 구성에서는 활성화되지 않는다. Bypass 사용과 실행 주체는 로그에 남기고 Tenant·조직·프로젝트 권한 검사는 유지한다.

## 외부 조건과 재개 조건

실제 비운영 IdP Metadata/JWKS·Issuer·Audience·테스트 Tenant·사용자·안정적인 `sub`·키 회전/폐기 권한, 운영 DB, 외부 수신 시스템은 아직 제공되지 않았다. 저장소는 private이고 현재 main 보호 메타데이터는 `protected=false`이며 필요한 보호 규칙 조회는 HTTP 403으로 거부되었다. 따라서 이 항목들은 Issue [#84](https://github.com/namanvarava/Edan.Tensor/issues/84)의 별도 환경 조건으로 유지한다.

외부 조건이 제공되면 External 모드에서 정상·Issuer/Audience 오류·만료·서명 실패·알 수 없는 `kid`, Tenant 격리, 키 중첩 회전·폐기를 실행하고 GitHub main 보호 규칙을 실제 API 또는 UI에서 확인한다. 해당 검증 전에는 전체 외부 운영 완료로 표시하지 않는다.

DuckDB·Parquet, 실제 결제·Webhook·환불·유료 권리, WPF 화면, 제품 API 라이선스 강제는 각각 포맷·업무·화면·공개 API 계약이 승인되기 전까지 구현하지 않는다. 이는 미처리 결함이 아니라 현재 승인 계약의 경계다.

