# 테스트

저장소에는 도메인·응용·인프라·API 통합 테스트 프로젝트가 포함되어 있으며, 보안·복원력·아키텍처 경계는 해당 프로젝트의 테스트에서 검증한다.

현재 확인된 테스트 구성은 다음과 같다.

- SharedKernel: 11개
- Platform Domain: 27개
- Platform Application: 30개
- Platform Infrastructure: 12개
- DataSet Domain: 6개
- DataSet Application: 8개
- DataSet Infrastructure: 8개
- Worker Domain: 21개
- Worker Infrastructure: 60개
- API 통합: 10개

2026-11-06 현재 소스 검증 기준 `main` 커밋 `6b0113fc14c0f3f9f41937bfb24ebaf18fa2e15c`에서 총 193개 테스트가 실패 0개·건너뜀 0개로 통과했다. 같은 결과는 [Actions Run #660](https://github.com/namanvarava/Edan.Tensor/actions/runs/42223986236)에서 확인할 수 있다. 저장소 루트에서 다음 명령으로 동일한 Release 검증을 재현할 수 있다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

원격 CI는 Windows 자체 호스팅 Runner에서 동일한 명령을 실행하며, 로컬 실행 결과와 원격 CI 결과를 구분해 검증 문서에 기록한다.
