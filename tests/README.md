# 테스트

저장소에는 도메인·응용·인프라·API 통합 테스트 프로젝트가 포함되어 있으며, 보안·복원력·아키텍처 경계는 해당 프로젝트의 테스트에서 검증한다.

현재 확인된 테스트 구성은 다음과 같다.

- SharedKernel: 11개
- Platform Domain: 29개
- Platform Application: 30개
- Platform Infrastructure: 12개
- DataSet Domain: 6개
- DataSet Application: 14개
- DataSet Infrastructure: 11개
- Worker Domain: 21개
- Worker Infrastructure: 66개
- API 통합: 32개

Development Bypass는 Development 환경에서만 고정된 비운영 IdP 시뮬레이션 프로필을 등록합니다. 프로필은 RFC 예약 도메인, `EDAN-NONPROD` 테스트 Tenant, `edan-nonprod-test-user`, 안정적인 `edan-nonprod-test-subject`를 사용하고 실제 외부 IdP에 연결하지 않으며, 첫 Bypass 인증 요청에 시뮬레이션 사용 경고를 기록합니다. Tenant와 안정적인 `sub`는 설정값으로 재정의할 수 없습니다. 운영·External 인증과 실제 비운영 IdP 검증의 완료 근거로 사용하지 않습니다.

이번 실행의 검증 대상 `main`은 PR #132 병합 커밋 SHA `8a82bf9f2a6546c593e44928d588f12d2fa2126c`이다. PR #132([최신 main CI 실패 상태 기록](https://github.com/namanvarava/Edan.Tensor/pull/132))의 head `451a3db88a93bfbcfbdf72ccb067e55813829ccd`는 squash merge되었다. PR #131 자체 CI [Run #881](https://github.com/namanvarava/Edan.Tensor/actions/runs/42672505068), PR #132 자체 CI [Run #884](https://github.com/namanvarava/Edan.Tensor/actions/runs/42675745493)와 병합 후 main CI [Run #885](https://github.com/namanvarava/Edan.Tensor/actions/runs/42676094148)는 Restore·Build·Test를 성공시켰다. 병합 직후 main CI [Run #882](https://github.com/namanvarava/Edan.Tensor/actions/runs/42672786178)와 PR #132 이전 head의 [Run #883](https://github.com/namanvarava/Edan.Tensor/actions/runs/42673460577)는 Job 생성 전 GitHub Actions `Internal server error`로 실패했지만, Run #884·#885에서 최신 문서 변경을 성공으로 재검증했다. 전체 테스트의 마지막 성공 증거는 main CI Run #885이며, PR #128 변경 브랜치에서 전체 232개·API 통합 32개가 통과했고 실패 0개·건너뜀 0개였다. 현재 main의 최신 SHA는 병합 후 GitHub 기록을 기준으로 확인하며, 외부 IdP·운영 DB·GitHub 보호 규칙 HTTP 403은 별도 환경 미검증으로 유지한다.

## 현재 최신 main 기준

PR #135 병합 후 최신 `main`은 `ce460ec7f3c4efcc54b4da5297879149b2f52b97`이다. PR 자체 CI Run #890과 병합 후 main CI [Run #891](https://github.com/namanvarava/Edan.Tensor/actions/runs/42681139618)이 Restore·Release Build·Release Test를 성공했다. Work에서 다음 구현 순서는 API 계약·멱등성 경쟁 검증 → Tenant·인증 경계 → Worker 장애 복구 → 승인된 비라이선스 기능 → Licensing·LicenseAuthority → 라이선스 적용 검증 → 운영·외부 환경 검증이다. 실제 외부 IdP, 운영 DB, 외부 수신 시스템, GitHub main 보호 규칙 HTTP 403은 이 테스트 결과에 포함하지 않는다.

PR #135는 동일 Tenant·Idempotency Key의 동시 외부 전달 생성에서 DB 유일성 충돌과 API 보상 삭제가 서로의 레코드에 영향을 줄 수 있던 경쟁 조건을 수정했다. `CreateOrGet_ReturnsExistingDeliveryWithoutCreatingDuplicate` 회귀 테스트와 main CI Run #891로 재검증했으며, 승인 계약이 없는 Commercial·DuckDB·Parquet·WPF는 임의 구현하지 않는다.

PR #81 병합 기준 main 문서는 SHA `030d4c91db86b13d2eb198ace1f699989a59318a`였다. 병합 후 [Actions Run #783](https://github.com/namanvarava/Edan.Tensor/actions/runs/42539919996)은 Restore·Release Build·Release Test·정리 단계를 모두 성공시켰고, 전체 테스트는 219개 통과·실패 0개·건너뜀 0개였다. 실제 비운영 외부 IdP 검증은 아직 수행하지 않았으며, 수동 절차는 [외부 IdP 경계 수동 검증 런북](../docs/validation/external-idp-boundary-test-runbook.md)에 기록되어 있다.

이번 Bypass 외부 설정 무시와 로컬 Tenant 초기화 회귀를 포함한 검증 기준에서 자체 호스팅 Windows Release 전체 테스트는 총 218개가 실패 0개·건너뜀 0개로 통과했다. 이 실행에는 개발 Bypass 선택, 외부 IdP 설정 없는 토큰 없는 요청의 인증 성공, 잘못된 외부 IdP 설정을 Bypass에서 무시하는 동작, 고정 Claim 발급, 비개발 환경 Bypass 차단, 로컬 Tenant·관리자 소속의 중복 방지 및 외부 인증 경계 회귀 테스트가 포함된다. PR #63 검증은 [Actions Run #744](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399072656)에서 확인했고, 병합 후 최신 `main`은 [Actions Run #746](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399622276)의 `EDAN-TENSOR-WIN` Runner에서 Restore·Build·Test를 다시 성공시켰다.

2026-11-07 DataSet 검증 Fencing Token 코드 기준 커밋 `2e2a49efdac38417037b7072c1220af17eeb8afb`에서 총 211개 테스트가 실패 0개·건너뜀 0개로 통과했다. 같은 결과는 [Actions Run #692](https://github.com/namanvarava/Edan.Tensor/actions/runs/42326121824)에서 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`으로 확인할 수 있다. 이 기록은 해당 시점의 인수인계 증거이며 최신 테스트 수를 대체하지 않는다. 저장소 루트에서 다음 명령으로 최신 Release 검증을 재현할 수 있다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

원격 CI는 Windows 자체 호스팅 Runner에서 동일한 명령을 실행하며, 로컬 실행 결과와 원격 CI 결과를 구분해 검증 문서에 기록한다.
