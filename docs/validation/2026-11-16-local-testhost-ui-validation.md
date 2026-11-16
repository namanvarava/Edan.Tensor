# 로컬 TestHost와 WPF 화면 API 검증 기록

- 검증일: 2026-11-16
- 검증 브랜치: `main-auto`
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

### 로컬 Host smoke

`tools/host-smoke.ps1`를 실행해 다음 응답을 확인했다.

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

## 3. Release 검증 결과

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

`System.Security.Cryptography.Pkcs` 보안 advisory 경고는 고정 버전 `9.0.21`을 중앙 패키지 버전으로 지정하고 ApiHost에 PrivateAssets 참조를 추가한 뒤 사라졌다.

## 4. 아직 수동 확인이 필요한 항목

Release 빌드 산출물로 `EDAN.Tensor Desktop` 프로세스와 `EDAN.Tensor` 창 생성을 확인했다. 다만 이 작업 환경에서는 데스크톱 조작 승인이 없어 창 내부를 클릭하거나 입력할 수 없었으므로 다음은 자동 성공으로 표시하지 않는다.

- 실제 WPF 창에서 API URL·Tenant ID·Bearer Token을 입력하는 조작
- `연결`, `새로 고침`, 생성·수정·삭제·재시도 버튼을 실제로 클릭하는 조작
- 401·403·404·409·422·5xx 응답이 화면 상태와 오류 설명에 반영되는지 확인
- 긴 목록의 페이징과 대상 선택 결과를 실제 화면에서 확인
- 창 재시작 후 입력값·연결 상태·오류 상태가 의도대로 초기화되는지 확인

수동 검증을 재개하려면 저장소 루트에서 다음으로 Desktop을 실행한 뒤, WPF 창에서 15개 화면을 차례로 선택한다.

```powershell
dotnet run --project .\src\Client\EDAN.Tensor.Desktop\EDAN.Tensor.Desktop.csproj --configuration Release
```

그 다음 개발용 API URL, 테스트 Tenant ID, 만료되지 않은 Access Token을 입력하고 각 화면의 조회 명령부터 실행한다. 생성·수정·삭제·재시도는 테스트 데이터와 대상 ID를 확인한 뒤 실행해야 한다.

## 5. 외부 환경 경계

비운영 Keycloak의 HTTPS Metadata·JWKS·Issuer·Audience·키 회전과 API Tenant 경계는 별도 Issue #84 증거를 따른다. 운영 IdP, 운영 DB, 실제 외부 수신 시스템, 배포 환경과 GitHub 보호 규칙은 로컬 TestHost와 WPF 자동 검증의 결과로 성공 처리하지 않는다.
