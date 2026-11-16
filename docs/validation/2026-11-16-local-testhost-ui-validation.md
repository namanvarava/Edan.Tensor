# 로컬 TestHost와 WPF 화면 API 검증 기록

- 검증일: 2026-11-16
- 검증 브랜치: `main`
- 검증 범위: ApiHost·WorkerHost 로컬 실행, OpenAPI·Swagger 응답, WPF Registry 화면 구성과 navigation, Release 전체 회귀
- 판단 원칙: 실제 실행 결과가 확인된 항목만 통과로 기록하고, 실행하지 못한 수동 조작은 미검증으로 유지한다.

## 1. 화면 API 아키텍처

EDAN.Tensor Desktop은 화면이 저장소나 Application Service를 직접 호출하지 않고, 다음 경계를 순서대로 통과한다.

`MainWindow` → `MainWindowViewModel` → `RegistryScreenViewModel` → `IDesktopScreenService` → `DesktopApiSession` → `TensorApiClient` → `ApiHost`

`MainWindowViewModel`은 15개 Screen ID를 Registry로 등록한다. `MainWindow.xaml`은 각 ViewModel을 `DataTemplate`으로 실제 WPF 화면에 연결한다. 사용자가 화면을 선택하면 선택된 ViewModel의 새로 고침이 호출되고, 화면의 API 명령은 `DesktopApiSession`이 구성한 Bearer 토큰·Tenant 헤더·멱등성 헤더를 사용해 typed Client로 전송된다.

따라서 화면 검증은 다음 세 단계로 구분한다.

1. Registry와 DataTemplate이 15개 화면을 모두 연결하는지 확인한다.
2. 모든 Screen ID를 순서대로 선택해 선택 상태와 화면 명령이 유지되는지 확인한다.
3. 실제 WPF 창에서 토큰·Tenant·대상 ID·Request DTO를 입력하고 조회·생성·수정·재시도 같은 버튼을 눌러 HTTP 응답과 화면 상태를 확인한다.

## 2. 자동으로 완료한 검증

### 로컬 Host smoke 기준 결과

기준 커밋 `a59ff87c4eaa5757924d7eb0fd6ba0dd7ca9d109`에서 `tools/host-smoke.ps1`를 실행해 다음 응답을 확인했다.

- ApiHost health: HTTP 200
- ApiHost database health: HTTP 200
- ApiHost OpenAPI: HTTP 200
- ApiHost Swagger UI: HTTP 200
- Development Bypass Tenant API: HTTP 200
- WorkerHost health: HTTP 200
- WorkerHost database health: HTTP 200
- ApiHost stderr: 0 bytes
- WorkerHost stderr: 0 bytes
- 임시 Smoke 디렉터리: 실행 후 제거됨

Development Bypass 결과는 격리된 개발 검증의 증거이며, 운영 IdP 인증 검증을 대신하지 않는다.

### WPF Registry와 navigation

`tests/EDAN.Tensor.Desktop.Tests/DesktopUiSmokeTests.cs`에서 STA 스레드 하나로 WPF `Application`과 `MainWindow`를 생성해 다음을 확인했다.

- Registry 화면 수: 15개
- 중복 Screen ID: 0개
- 첫 화면: `PLT-TNT-001`
- 마지막 화면: `WRK-XDL-001`
- DataTemplate 수: 15개
- 15개 화면을 순서대로 선택할 때 선택 상태 유지
- 각 화면의 Screen ID·제목·API 명령 목록 존재

처음에는 두 테스트가 각각 WPF `Application`을 생성해 두 번째 테스트가 실패했다. 근본 원인은 같은 테스트 프로세스에서 WPF `Application`을 두 번 만들 수 없는 실행 제약이었다. 두 검증을 하나의 STA Application 수명 안으로 합쳐 재실행했고 WPF 테스트는 통과했다.

## 3. 이전 Release 검증 결과

다음 명령을 저장소 루트에서 순서대로 실행했다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

- Restore: 성공
- Release Build: 경고 0건, 오류 0건
- 전체 테스트: 290개 통과, 실패 0개, 건너뜀 0개
- WPF Desktop 테스트: 1개 통과, 실패 0개
- ApiHost 통합 테스트: 48개 통과, 실패 0개

위 수치는 이전 검증 시점의 기준 결과이다. 현재 병합된 로컬 `main`의 최신 결과는 4.4절에 기록한다.

`System.Security.Cryptography.Pkcs` 보안 advisory 경고는 고정 버전 `9.0.21`을 중앙 패키지 버전으로 지정하고 ApiHost에 PrivateAssets 참조를 추가한 뒤 사라졌다.

## 4. 2026-11-16 재검증과 환경 경계

기준 결과 이후 smoke 정리 단계에서 다음 결함을 재현했다.

- Host가 종료되는 순간에 `HasExited` 확인과 `Kill()` 호출 사이의 경쟁 상태가 발생했다.
- 아직 시작되지 않은 Worker 프로세스의 `StandardError`를 정리 단계에서 읽으면서 null 스트림 예외가 발생했다.
- 새 SQLite 네 개의 Migration 시간이 기존 15초 준비 제한을 넘을 수 있었다.

원인은 Host 계약이나 인증 코드가 아니라 smoke 스크립트의 프로세스 생명주기와 초기화 대기 처리였다. `tools/host-smoke.ps1`에 다음 최소 수정을 반영했다.

- 프로세스 상태를 새로 고친 뒤 이미 종료된 Host에는 `Kill()`을 호출하지 않는다.
- 종료 경쟁으로 발생한 `InvalidOperationException`을 정리 성공으로 처리한다.
- 시작되지 않은 프로세스의 표준 오류 스트림을 안전하게 읽는다.
- SQLite Migration을 고려해 Host 준비 대기를 15초에서 60초로 늘린다.

수정 후 새 임시 포트에서 ApiHost 프로세스는 표준 오류 없이 시작되었지만, 현재 Codex 실행 셸에서는 자식 프로세스가 연 `127.0.0.1` 포트에 부모 셸의 HTTP 요청이 도달하지 않았다. 따라서 이번 재검증의 Host HTTP 결과는 성공으로 표시하지 않는다. 동일한 스크립트를 사용자가 직접 실행하는 Windows PowerShell 또는 승인된 로컬 데스크톱 환경에서 다시 실행해야 한다.

WPF 전용 테스트도 다음 경계가 확인되었다.

- `NU1900`은 NuGet 취약성 조회 서비스 접근 경고이며 코드 테스트 실패가 아니다.
- `--no-build --no-restore` 실행에서는 WPF `testhost`가 60초 안에 연결되지 않아 실행된 테스트 수가 0개였다.
- 데스크톱 자동화 승인과 실제 WPF 창이 없어 화면 클릭·입력 검증을 수행하지 못했다.

따라서 WPF의 실제 입력·버튼 조작과 현재 실행 셸에서의 Host HTTP 재검증은 미검증으로 유지한다. 이를 통과로 바꾸려면 사용자가 데스크톱 접근이 가능한 Windows 환경에서 WPF 창과 TestHost를 실행하고, 아래 수동 절차의 결과를 증거로 남겨야 한다.

### 4.1 2026-11-16 재실행 증거

이번 작업에서 같은 저장소 상태를 다시 확인한 결과는 다음과 같다.

- 사용자가 지정한 병렬 `dotnet restore EDAN.Tensor.sln`은 현재 실행 환경에서 종료 코드 1과 진단 없는 실패를 반환했다. 따라서 이 명령은 성공으로 기록하지 않는다.
- 병렬 복원 실패 원인을 분리하기 위해 `dotnet restore .\EDAN.Tensor.sln --verbosity normal -m:1 -p:RestoreDisableParallel=true`를 실행했다. 이 명령은 종료 코드 0으로 완료했지만, NuGet 취약성 조회 서비스에 접근하지 못해 `NU1900` 경고 23건이 발생했다. 이 경고는 코드 테스트 실패가 아니지만, 네트워크 기반 취약성 조회가 완료되지 않았다는 운영 증거로 남긴다.
- 복원 결과를 사용해 `dotnet build .\EDAN.Tensor.sln --no-restore --configuration Release -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false -v:minimal`을 실행했다. 40개 프로젝트가 오류 없이 빌드되었고 종료 코드는 0이었다. 같은 빌드에서 `NU1900` 경고 23건이 표시되었으므로 경고 0건으로 기록하지 않는다.
- 사용자가 지정한 `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`는 종료 코드 1을 반환했다. 상세 실행에서는 첫 테스트 DLL을 읽은 뒤 `vstest`가 90초 동안 `testhost`에 연결하지 못했고, 테스트 어설션 결과는 실행되지 않았다.
- SharedKernel 테스트 DLL을 `dotnet vstest`로 직접 실행해도 같은 `testhost` 연결 시간 초과가 재현되었다. 따라서 현재 결과는 테스트 실패가 아니라 이 실행 환경의 TestHost 프로세스 통신 실패로 분류한다.
- 수정된 `tools/host-smoke.ps1`를 새 포트로 실행하면 ApiHost·WorkerHost 표준 오류는 0바이트이고 임시 디렉터리는 제거되지만, 현재 실행 셸에서 자식 프로세스가 연 `127.0.0.1` 포트에 부모 셸의 HTTP 요청이 도달하지 않아 health 확인이 실패한다. 이 결과도 API 계약 실패가 아니라 현재 실행 환경의 로컬 자식 프로세스 네트워크 경계로 분류한다.

따라서 현재 브랜치에서 확인된 코드 상태는 Release 빌드 성공까지이며, 전체 테스트와 현재 셸의 Host HTTP smoke는 환경 미검증으로 남는다. 이 둘을 완료로 바꾸려면 동일한 Windows PowerShell 세션에서 복원·Host·테스트를 실행하거나, 로컬 TestHost와 WPF 실행을 허용하는 데스크톱 환경에서 다시 실행해야 한다.

### 4.2 최신 원격 main 전용 작업공간 재검증

이전 4.1의 실행 환경 경계를 분리하기 위해 원격 `main`의 최신 커밋 `e19a38af94e4780dfa373a8754089025f96262be`를 별도 작업공간에서 다시 검증했다. 사용자 작업공간의 변경과 섞지 않았으며, 다음 결과를 실제 명령 종료 코드와 출력으로 확인했다.

- `dotnet restore .\EDAN.Tensor.sln --verbosity minimal -m:1 -p:RestoreDisableParallel=true -p:NuGetAudit=false`: 종료 코드 0.
- `dotnet build .\EDAN.Tensor.sln --no-restore --configuration Release -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false -v:minimal`: 40개 프로젝트, 경고 0건, 오류 0건, 종료 코드 0.
- `dotnet test .\EDAN.Tensor.sln --no-build --no-restore --configuration Release --logger "console;verbosity=minimal"`: 290개 통과, 실패 0개, 건너뜀 0개, 종료 코드 0.
- `dotnet test .\tests\EDAN.Tensor.Desktop.Tests\EDAN.Tensor.Desktop.Tests.csproj --no-build --no-restore --configuration Release --logger "console;verbosity=normal"`: WPF STA Registry smoke 1개 통과, 실패 0개, 종료 코드 0.
- `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\host-smoke.ps1 -ApiPort 54517 -WorkerPort 54518`: ApiHost health·database health·OpenAPI·Swagger UI·Development Bypass Tenant API와 WorkerHost health·database health가 모두 HTTP 200을 반환했다. 두 Host 표준 오류는 각각 0바이트였고 임시 Smoke 디렉터리는 실행 후 제거되었다.

이 재검증으로 이전 4.1에서 기록한 부모 셸의 TestHost 통신 실패와 Host HTTP 미검증은 최신 원격 `main` 자체의 재현 결함이 아니라 해당 실행 경계에서 발생한 것으로 분리했다. 따라서 자동화 가능한 Release·통합·WPF Registry·Host smoke 범위는 통과 증거를 확보했다.

다만 실제 WPF 창의 마우스 클릭·키보드 입력, 생성·수정·삭제·재시도 조작, 401·403·404·409·422·5xx 화면 표시 확인은 별도 데스크톱 조작 권한이 필요한 수동 범위이므로 여전히 자동 통과로 판정하지 않는다. 이 범위는 6절의 수동 절차로만 완료 처리한다.

### 4.3 실제 WPF 창 조작 재검증

이 절의 결과는 이전 검증 환경에서 확보한 WPF 화면 조작 증거이다. 현재 Codex 실행 환경에서는 데스크톱 조작 권한이 승인되지 않아 동일 조작을 다시 수행하지 못했으므로, 현재 실행의 통과 결과로 중복 집계하지 않는다.

최신 `origin/main` 커밋 `b21b1b2ff9e6abe2d7d5a62c205227bc2c2cb3b1`을 별도 작업공간에서 `EDAN.Tensor.Desktop`으로 실행하고 Windows UI Automation으로 실제 창을 조작했다. 토큰 원문과 비밀번호는 출력하거나 기록하지 않았다.

- 실제 `EDAN.Tensor` 창이 생성되었고 프로세스가 응답 상태였다.
- `API URL`에 `http://localhost:5000`, 테스트 Tenant ID를 입력하고 `API 연결`을 눌렀다. 화면 상태가 `연결됨`으로 변경되었다.
- Keycloak 테스트 Access Token을 화면의 `Bearer Token` 입력란에 주입한 뒤 조회를 수행했다.
- `PLT-TNT-001`, `PLT-ORG-001`, `PLT-PJT-001`, `PLT-USR-001`, `PLT-MBR-001`, `PLT-AUTH-001`, `PLT-AUD-001`, `PLT-QTA-001`, `DSET-DSM-001`, `DSET-SCHM-001`, `DSET-STO-001`, `DSET-VLD-001`, `WRK-TSK-001`, `WRK-DLQ-001`, `WRK-XDL-001`의 15개 화면을 모두 선택하고 각 화면의 `새로 고침`을 실행했다. 각 화면은 실패 코드나 인증 오류 없이 완료되었다.
- Access Token을 의도적으로 `invalid-token`으로 바꾸고 Tenant 화면을 새로 고쳤다. 화면에 `API 요청을 처리하지 못했습니다.`와 `api.http_error`가 표시되어 인증 실패가 조용히 성공으로 처리되지 않음을 확인했다.
- 검증 후 정상 테스트 토큰을 복원하고 WPF 프로세스와 임시 작업공간을 종료·삭제했다.

이 검증으로 실제 WPF의 연결·입력·화면 선택·조회·인증 오류 표시 범위를 통과 처리한다. 운영 데이터에 영향을 주는 생성·수정·삭제·수동 재시도와 403·404·409·422·5xx별 화면 검증은 격리된 테스트 데이터와 각 오류를 의도적으로 만들 수 있는 외부 조건이 필요하므로 별도 운영 시나리오 검증으로 남긴다.

### 4.4 현재 병합된 로컬 `main` 재검증

현재 로컬 `main`은 원격 `main`을 병합한 커밋 `e0143d0b1b5cdabfa437d9eedda3e0ac2fd6cec7`에서 다음 명령을 실제 실행했다.

- `dotnet restore EDAN.Tensor.sln`: 종료 코드 0. NuGet 취약성 조회 서비스에 연결하지 못한 `NU1900` 경고가 발생했으므로 네트워크 기반 취약성 조회는 미완료로 기록한다.
- `dotnet build EDAN.Tensor.sln --no-restore --configuration Release`: 종료 코드 0, 오류 0개. 동일한 `NU1900` 경고가 남아 경고 0건으로 기록하지 않는다.
- `dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release`: 종료 코드 0, 성공 283개, 실패 0개, 건너뜀 0개.
- `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\host-smoke.ps1 -ApiPort 54517 -WorkerPort 54518`: ApiHost health·database health·OpenAPI·Swagger UI·개발 우회 Tenant API와 WorkerHost health·database health가 모두 HTTP 200을 반환했다. 두 Host 표준 오류는 0바이트였고 임시 Smoke 디렉터리는 제거되었다.

현재 실행 환경에서는 데스크톱 조작 권한이 승인되지 않아 WPF 창의 실제 클릭·입력·생성·수정·삭제·재시도 조작은 다시 수행하지 못했다. 4.3절은 이전 환경의 증거이고, 현재 환경의 WPF 조작 검증은 미완료로 유지한다.

## 5. 동시 라이선스 발급 회귀 수정

Windows CI에서 개발 라이선스를 동시에 발급하는 테스트가 같은 JSON 파일을 서로 다른 저장소 인스턴스로 읽는 순간에 파일 잠금 예외를 재현했다. 근본 원인은 저장소 인스턴스마다 별도의 잠금을 사용해 같은 경로의 읽기·쓰기를 조정하지 못한 것이었다.

파일의 전체 경로를 기준으로 프로세스 내부의 `SemaphoreSlim`을 공유하도록 수정했다. 이 수정은 저장소 공개 API와 파일 형식을 바꾸지 않으며, 같은 개발 라이선스 파일을 사용하는 동시 발급 요청만 순서대로 처리한다. 대상 회귀 테스트는 실제 Windows 실행에서 1개 통과했다.

## 6. 아직 별도 시나리오 확인이 필요한 항목

실제 WPF 창의 연결·입력·15개 화면 선택·조회·인증 오류 표시는 4.3절의 이전 환경 증거로 확인되었다. 현재 환경에서 다시 확인해야 하는 조작과 운영 데이터 변경 위험 또는 별도 오류 재현 조건이 필요한 항목은 다음과 같다.

- 생성·수정·삭제·재시도 버튼을 격리된 테스트 데이터로 실제 실행하는 조작
- 403·404·409·422·5xx 응답이 화면 상태와 오류 설명에 반영되는지 확인
- 긴 목록의 페이징과 대상 선택 결과를 실제 화면에서 확인
- 창 재시작 후 입력값·연결 상태·오류 상태가 의도대로 초기화되는지 확인

수동 검증을 재개하려면 저장소 루트에서 다음으로 Desktop을 실행한 뒤, WPF 창에서 15개 화면을 차례로 선택한다.

```powershell
dotnet run --project .\src\Client\EDAN.Tensor.Desktop\EDAN.Tensor.Desktop.csproj --configuration Release
```

창이 열리면 왼쪽 입력란을 다음 순서로 채운다.

1. `API URL`에는 TestHost가 실제로 열어 둔 주소를 입력한다. 기본값은 `https://localhost:5001`이며, HTTP smoke 스크립트를 별도 포트로 실행한 경우에는 그 실행 포트의 `http://127.0.0.1:<ApiPort>` 주소를 입력한다.
2. `Tenant ID (선택)`에는 테스트 Tenant GUID를 입력한다. Tenant 범위 API를 검증할 때만 입력하고, GUID가 아니면 연결이 거부된다.
3. `Bearer Token (선택)`에는 만료되지 않은 Access Token 원문을 입력한다. `Bearer ` 접두사는 입력하지 않는다. 화면이 요청을 보낼 때 클라이언트가 접두사를 자동으로 추가한다.
4. `API 연결`을 누르고 상태 문구가 `연결됨`으로 바뀌는지 확인한다.
5. 왼쪽 Registry에서 `PLT-TNT-001`부터 `WRK-XDL-001`까지 15개 화면을 차례로 선택한다. 선택할 때마다 화면이 바뀌고 조회 명령이 실패하면 상태·오류 설명이 화면에 표시되는지 기록한다.
6. 각 화면에서 먼저 조회만 실행한다. 생성·수정·삭제·재시도는 격리된 테스트 Tenant와 테스트 데이터의 식별자를 확인한 뒤 한 번씩 실행하고, 응답 코드·화면 상태·재조회 결과를 기록한다.
7. 같은 화면에서 만료 토큰, 잘못된 Tenant, 존재하지 않는 대상 ID를 차례로 사용해 401·403·404 응답이 안전한 오류로 표시되는지 확인한다. 운영 데이터나 운영 토큰은 사용하지 않는다.

TestHost와 WPF를 실제로 검증할 때는 다음 순서로 별도 PowerShell 창을 사용한다.

```powershell
Set-Location 'C:\Users\naman\OneDrive\문서\ChatGPT\Edan.Tensor'
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\host-smoke.ps1 -ApiPort 54517 -WorkerPort 54518
```

smoke가 HTTP 200 결과를 모두 출력하고 종료된 뒤에 Desktop을 실행한다. smoke가 실패하면 WPF 조작 결과와 섞지 말고, `ApiHost`·`WorkerHost` 로그와 사용한 포트·종료 코드를 함께 기록한다.

## 7. 외부 환경 경계

비운영 Keycloak의 HTTPS Metadata·JWKS·Issuer·Audience·키 회전과 API Tenant 경계는 별도 Issue #84 증거를 따른다. 운영 IdP, 운영 DB, 실제 외부 수신 시스템, 배포 환경과 GitHub 보호 규칙은 로컬 TestHost와 WPF 자동 검증의 결과로 성공 처리하지 않는다.

## 8. 2026-11-16 현재 실행 환경 재시도 기록

이 기록을 남기기 전에 현재 로컬 `main`에서 Release 회귀 검증을 다시 실행했다. 전체 솔루션 테스트는 실패 0개·건너뜀 0개로 종료되었고, WPF 테스트는 1개 통과했다. `host-smoke.ps1 -ApiPort 54517 -WorkerPort 54518`도 ApiHost health·database health·OpenAPI·Swagger UI·Development Bypass Tenant API와 WorkerHost health·database health 모두 HTTP 200을 반환했으며 두 Host 표준 오류는 0바이트이고 임시 Smoke 디렉터리는 제거되었다.

현재 로컬 `main`의 Release WPF 실행 파일을 실제로 시작하고 `EDAN.Tensor` 창을 하나 확인했다. 그러나 창 상태를 읽고 클릭·입력하기 직전에 데스크톱 조작 권한이 다시 `Computer Use was not approved to use desktop`으로 거부되었다.

따라서 이번 실행에서는 WPF 창에 입력하거나 버튼을 누르지 않았으며, 토큰·비밀번호·운영 데이터도 전송하지 않았다. 이 결과는 WPF 코드 결함이나 TestHost 실패의 증거가 아니라, 현재 Codex 실행 환경의 데스크톱 조작 권한 미승인 증거이다. 권한이 승인된 Windows 세션에서 6절의 절차를 다시 실행해야 실제 WPF 조작 검증을 완료할 수 있다.

### 9. WPF 화면과 API 연결 정적 대조

실제 클릭을 대신하는 통과 판정은 아니지만, 현재 소스의 화면 등록·화면별 입력·명령 매핑을 대조해 수동 검증 대상의 범위를 고정했다. 아래 표는 `MainWindowViewModel`, `ScreenModels`, `ScreenViewModels`, `DesktopApiSession`의 현재 구현을 기준으로 작성했다.

| 화면 ID | 화면 기능 | 기본 조회 또는 직접 연결된 API | 주요 추가 명령 | 현재 판정 |
|---|---|---|---|---|
| PLT-TNT-001 | 테넌트 관리 | 내 멤버십·테넌트 목록·생성·수정·활성화 | 디렉터리 조회 | 정적 연결 확인, 실제 조작 미검증 |
| PLT-ORG-001 | 조직 관리 | 조직 목록·생성·수정 | 조직 수정 | 정적 연결 확인, 실제 조작 미검증 |
| PLT-PJT-001 | 프로젝트 관리 | 프로젝트 목록·생성·수정 | 프로젝트 수정 | 정적 연결 확인, 실제 조작 미검증 |
| PLT-USR-001 | 사용자 초대 관리 | 사용자·초대 목록 | 사용자 수정·초대 조회·재전송·철회 | 정적 연결 확인, 실제 조작 미검증 |
| PLT-MBR-001 | 멤버십 관리 | 테넌트·조직·프로젝트 멤버십 조회 | 멤버십 부여·역할 변경·회수 | 정적 연결 확인, 실제 조작 미검증 |
| PLT-AUTH-001 | 권한 정책 관리 | 정책 목록 | 정책 활성화 변경·권한 평가 | 정적 연결 확인, 실제 조작 미검증 |
| PLT-AUD-001 | 권한 감사 관리 | 감사 목록·무결성 조회 | 아카이브·작업 생성·조회·재시도·검증·스케줄·보존 정책 | 정적 연결 확인, 실제 조작 미검증 |
| PLT-QTA-001 | 저장 용량 할당량 관리 | 할당량 정책·이력 조회 | 정책 수정·활성화 변경 | 정적 연결 확인, 실제 조작 미검증 |
| DSET-DSM-001 | 데이터셋 관리 | 데이터셋 목록·상세 | 이름 변경·보관·복원·삭제 | 정적 연결 확인, 실제 조작 미검증 |
| DSET-SCHM-001 | 데이터셋 스키마 관리 | 스키마 revision 목록·상세 | 스키마 비교·rollback | 정적 연결 확인, 실제 조작 미검증 |
| DSET-STO-001 | 데이터셋 저장소 관리 | 저장 사용량·이력 조회 | 콘텐츠 업로드·다운로드·초기화·Quota 이벤트 조회 | 정적 연결 확인, 실제 조작 미검증 |
| DSET-VLD-001 | 데이터셋 검증 관리 | 검증 profile·revision·execution 조회 | profile 생성·revision 추가·수정·삭제·실행 삭제 | 정적 연결 확인, 실제 조작 미검증 |
| WRK-TSK-001 | Worker 작업 모니터링 | Worker 작업 목록·상세 | 취소·강제 재시도·결과 참조 | 정적 연결 확인, 실제 조작 미검증 |
| WRK-DLQ-001 | 실패 작업 관리 | Dead Letter Queue 목록 | Replay 이력 조회 | 정적 연결 확인, 실제 조작 미검증 |
| WRK-XDL-001 | 외부 전달 모니터링 | 외부 전달 목록·상세 | 외부 전달 수동 재시도 | 정적 연결 확인, 실제 조작 미검증 |

정적 대조에서 화면 ID 15개가 `MainWindowViewModel`에 등록되고, 각 화면의 추가 명령이 `ScreenModels`에 정의되며, 화면별 조회·상세 API 호출이 `ScreenViewModels`에서 `DesktopApiSession.Client`를 통해 호출되는 것을 확인했다. 따라서 현재 남은 작업은 화면 등록이나 API 매핑 누락을 추측해 수정하는 일이 아니라, 격리된 TestHost·테스트 Tenant·만료 토큰·잘못된 Tenant·존재하지 않는 ID를 사용한 실제 클릭 결과를 기록하는 검증이다.

다음 수동 검증에서는 각 행에 대해 `연결 → 조회 → 정상 명령 1회 → 재조회 → 오류 입력 1회` 순서로 수행한다. 생성·수정·삭제·재시도 명령은 테스트 Tenant에서만 수행하고, 각 명령의 HTTP 상태·화면 상태 문구·재조회 결과·오류 설명을 기록해야 한다. 당시 정적 검토 시점에는 데스크톱 조작 권한이 승인되지 않아 이 표를 통과 증거로 승격하지 않았으며, 이후 14절에서 실제 WPF 창의 입력·버튼 클릭·화면 전환·인증 오류 표시를 별도로 확인했다.

## 10. 2026-11-16 목표 재개 후 재검증

목표 재개 후 `main`의 현재 작업 디렉터리에서 Release 전체 테스트와 Host Smoke를 다시 실행했다. 전체 솔루션 테스트는 실패 0개·통과 292개·건너뜀 0개로 종료되었고, Host Smoke는 ApiHost와 WorkerHost의 health·database health, ApiHost OpenAPI·Swagger UI·개발 Tenant API를 모두 HTTP 200으로 확인했다. 두 Host 표준 오류는 0바이트였고 임시 Smoke 디렉터리는 제거되었다.

초기 WPF 실행 시도에서는 데스크톱 조작 권한이 없어 실제 조작을 수행하지 못했지만, 이후 권한이 확보된 Windows 세션에서 실제 WPF 창의 입력·버튼 클릭·화면 전환·인증 오류 표시를 확인했다. 상세 결과는 14절에 기록했다. 인증된 성공 조회와 변경 명령은 별도 외부 IdP 토큰을 입력해야 하므로 아직 완료 판정에 포함하지 않는다.

## 11. 15개 WPF 화면 선택·렌더링 보고서 반영

`docs/wpf-15-screen-validation-report.md`에 기록된 사용자 제공 검증 결과를 확인 기록으로 연결한다. 해당 보고서는 15개 Screen ID를 실제 WPF 창에서 선택하고 화면 코드·제목·Request DTO 입력 영역·액션 영역이 렌더링되는 결과를 화면별로 기록한다.

이 보고서가 증명하는 범위는 화면 레지스트리 탐색과 화면 렌더링이다. 보고서 자체에 API가 미연결 상태였고 서버 조회·생성·수정·재시도·외부 전달 및 실제 데이터 변경을 수행하지 않았다고 명시되어 있으므로, 15개 화면의 UI 렌더링은 확인 완료로 기록하되 화면별 API 동작 검증은 미완료로 유지한다.

현재 검증 상태는 다음과 같다.

- 15개 화면 등록·선택·렌더링: 확인 완료
- TestHost·ApiHost·WorkerHost·Swagger·DB health: 자동 검증 완료
- 15개 화면의 TestHost 인증 조회 경로: 자동 검증 완료
- 실제 외부 IdP 토큰을 사용한 WPF 인증 조회: 미검증
- 생성·수정·삭제·재시도·외부 전달: 미수행
- 401·403·404·409·422·5xx의 화면 표시: 미검증
- 실제 WPF 입력·버튼 클릭·화면 전환·인증 오류 표시: 확인 완료
- 유효한 외부 IdP 토큰을 사용한 WPF 성공 조회와 변경 명령: 미검증

## 12. WPF ViewModel과 실제 TestHost API 통합 검증

화면 렌더링 검증과 서버 호출 경계를 분리하기 위해 `DesktopApiIntegrationTests`를 추가했다. 테스트는 임시 SQLite 네 개를 사용하는 실제 `ApiHost`를 생성하고, `DesktopApiSession`에 해당 HTTP 클라이언트를 주입한 뒤 `TenantManagementViewModel.RefreshAsync`를 실행한다.

다음 결과를 확인했다.

- WPF 세션이 실제 `TensorApiClient`를 생성하고 API 요청을 전송했다.
- 실제 ApiHost의 Development Bypass 인증이 요청을 승인했다.
- 실제 SQLite Platform 데이터베이스에서 Tenant 멤버십 1건을 조회했다.
- 조회된 Tenant ID가 비어 있지 않고 역할이 `Administrator`로 표시되었다.
- 화면 상태가 `1개의 활성 Tenant 소속을 조회했습니다.`로 갱신되었다.
- 테스트는 임시 데이터베이스와 HTTP 클라이언트를 사용하며 검증 후 임시 디렉터리를 삭제한다.

이번 검증은 15개 화면 중 테넌트 화면의 실제 ViewModel→Client→ApiHost→SQLite 경로를 대표 검증한 결과다. 따라서 15개 화면의 모든 변경 명령과 실제 독립 WPF 클릭·입력 검증을 완료했다고 판정하지 않는다. 나머지 화면의 조회·생성·수정·삭제·재시도 및 오류 화면 표시는 별도 검증 범위로 유지한다.

## 13. 15개 WPF 화면 조회 경로 통합 검증

`DesktopApiIntegrationTests.AllRegisteredScreensRefreshAgainstRealApiHost`를 추가하여 15개 화면 ViewModel의 `RefreshAsync` 조회 경로를 하나의 격리된 실제 ApiHost에서 순차 검증했다. 테스트는 임시 SQLite 네 개와 Development Bypass 테스트 인증을 사용하고, 먼저 Tenant 목록을 조회한 뒤 선택한 Tenant ID를 공통 세션에 설정하여 나머지 화면을 조회한다.

검증 결과는 다음과 같다.

- 15개 화면 ViewModel이 모두 실제 ApiHost에 연결되었다.
- 각 화면의 `RefreshAsync`가 예외 없이 완료되었다.
- 각 화면이 최소 하나 이상의 입력·상태 필드를 구성했다.
- 각 화면이 연결 전 상태 문구에 머물지 않았다.
- 테스트 결과는 실패 0개·통과 2개로 확인했다. 통과 2개는 기존 Tenant 대표 통합 테스트와 신규 15개 화면 조회 통합 테스트다.

이 결과는 15개 화면의 인증된 조회 경로가 실제 TestHost에서 연결된다는 증거다. 그러나 화면 버튼을 실제로 클릭하여 생성·수정·삭제·재시도하는 동작, 입력 오류·권한 오류·충돌 오류의 화면 표시, 운영 외부 환경 검증까지 완료했다는 의미는 아니다. 실제 외부 IdP 토큰을 사용한 WPF 성공 조회와 변경 명령은 별도 검증 범위로 유지한다.

## 14. 실제 WPF 창 조작과 인증 오류 표시 검증

병합된 `main`의 Release WPF 실행 파일을 실제 Windows 창으로 시작하고, API URL에 `http://localhost:5000`, Tenant ID에 테스트 Tenant ID를 입력했다. Bearer Token을 입력하지 않은 상태에서 `API 연결` 버튼을 클릭한 결과를 다음과 같이 확인했다.

- WPF 창이 정상적으로 시작되고 API URL·Tenant ID 입력값을 유지했다.
- `API 연결` 버튼을 실제로 클릭할 수 있었고, 화면 상태가 연결 시도 결과로 갱신되었다.
- 인증 토큰이 없는 요청은 `api.http_error`와 요청 ID를 화면에 표시했다.
- API 연결 상태는 `http://localhost:5000/`와 테스트 Tenant를 표시하여 세션 설정 결과를 반영했다.
- `조직 관리(PLT-ORG-001)` 항목을 실제로 선택했고, 화면 제목·설명·입력 영역·액션 영역이 조직 화면으로 전환되었다.
- 조직 화면에서도 인증 오류 코드와 요청 ID가 표시되어 화면별 오류 표시 경로를 확인했다.

이번 검증은 실제 WPF 창의 입력·버튼 클릭·화면 전환·인증 오류 표시를 확인한 결과다. 다만 유효한 외부 IdP Bearer Token을 입력한 성공 조회와 생성·수정·삭제·재시도 명령은 아직 수행하지 않았다. 토큰은 만료 시간이 짧고 비밀번호를 저장소나 로그에 기록하지 않으므로, 성공 경로를 검증할 때는 사용자가 새 토큰을 발급하여 WPF의 Bearer Token 입력란에 직접 입력해야 한다.
