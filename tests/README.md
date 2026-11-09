# 테스트

저장소에는 도메인·응용·인프라·API 통합 테스트 프로젝트가 포함되어 있으며, 보안·복원력·아키텍처 경계는 해당 프로젝트의 테스트에서 검증한다.

현재 확인된 테스트 구성은 다음과 같다.

- SharedKernel: 11개
- Platform Domain: 27개
- Platform Application: 30개
- Platform Infrastructure: 12개
- DataSet Domain: 6개
- DataSet Application: 14개
- DataSet Infrastructure: 10개
- Worker Domain: 21개
- Worker Infrastructure: 66개
- API 통합: 26개

Development Bypass는 Development 환경에서만 고정된 비운영 IdP 시뮬레이션 프로필을 등록합니다. 프로필은 RFC 예약 도메인과 `edan-nonprod` 테스트 식별자를 사용하고 실제 외부 IdP에 연결하지 않으며, 첫 Bypass 인증 요청에 시뮬레이션 사용 경고를 기록합니다. 운영·External 인증과 실제 비운영 IdP 검증의 완료 근거로 사용하지 않습니다.

최신 제품 코드 기준은 `main` SHA `eb0765877c48dda06207b027f481f614f3301b9a`이다. 이 기준의 실제 Windows 권한 Release 검증은 전체 테스트 223개 통과·실패 0개·건너뜀 0개였고, API 통합 테스트는 26개였다. 이 문단은 아래의 역사적 실행 기록과 문서 전용 후속 커밋을 구분하는 현재 제품 코드 검증 기준이다.

PR #81 병합 기준 main 문서는 SHA `030d4c91db86b13d2eb198ace1f699989a59318a`였다. 병합 후 [Actions Run #783](https://github.com/namanvarava/Edan.Tensor/actions/runs/42539919996)은 Restore·Release Build·Release Test·정리 단계를 모두 성공시켰고, 전체 테스트는 219개 통과·실패 0개·건너뜀 0개였다. 실제 비운영 외부 IdP 검증은 아직 수행하지 않았으며, 수동 절차는 [외부 IdP 경계 수동 검증 런북](../docs/validation/external-idp-boundary-test-runbook.md)에 기록되어 있다.

이번 Bypass 외부 설정 무시와 로컬 Tenant 초기화 회귀를 포함한 검증 기준에서 자체 호스팅 Windows Release 전체 테스트는 총 218개가 실패 0개·건너뜀 0개로 통과했다. 이 실행에는 개발 Bypass 선택, 외부 IdP 설정 없는 토큰 없는 요청의 인증 성공, 잘못된 외부 IdP 설정을 Bypass에서 무시하는 동작, 고정 Claim 발급, 비개발 환경 Bypass 차단, 로컬 Tenant·관리자 소속의 중복 방지 및 외부 인증 경계 회귀 테스트가 포함된다. PR #63 검증은 [Actions Run #744](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399072656)에서 확인했고, 병합 후 최신 `main`은 [Actions Run #746](https://github.com/namanvarava/Edan.Tensor/actions/runs/42399622276)의 `EDAN-TENSOR-WIN` Runner에서 Restore·Build·Test를 다시 성공시켰다.

2026-11-07 DataSet 검증 Fencing Token 코드 기준 커밋 `2e2a49efdac38417037b7072c1220af17eeb8afb`에서 총 211개 테스트가 실패 0개·건너뜀 0개로 통과했다. 같은 결과는 [Actions Run #692](https://github.com/namanvarava/Edan.Tensor/actions/runs/42326121824)에서 Windows 자체 호스팅 Runner `EDAN-TENSOR-WIN`으로 확인할 수 있다. 이 기록은 해당 시점의 인수인계 증거이며 최신 테스트 수를 대체하지 않는다. 저장소 루트에서 다음 명령으로 최신 Release 검증을 재현할 수 있다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

원격 CI는 Windows 자체 호스팅 Runner에서 동일한 명령을 실행하며, 로컬 실행 결과와 원격 CI 결과를 구분해 검증 문서에 기록한다.
