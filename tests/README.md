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
- API 통합: 14개

2026-11-07 최신 `main` 커밋 `72cf580b1368e3273246a46e80f7801b7c5e14cb` 기준으로는 총 210개 테스트가 실패 0개·건너뜀 0개로 통과했다. 현재 변경은 DataSet Infrastructure의 오래된 Fencing Token 결과 쓰기 회귀 테스트 1개를 추가하므로, 병합 후 총 211개가 된다. 저장소 루트에서 다음 명령으로 동일한 Release 검증을 재현할 수 있다.

```powershell
dotnet restore EDAN.Tensor.sln
dotnet build EDAN.Tensor.sln --no-restore --configuration Release
dotnet test EDAN.Tensor.sln --no-build --no-restore --configuration Release
```

원격 CI는 Windows 자체 호스팅 Runner에서 동일한 명령을 실행하며, 로컬 실행 결과와 원격 CI 결과를 구분해 검증 문서에 기록한다.
