# Provider 운영 수용 점검표

Status: Approved operational checklist

이 문서는 EDAN.Tensor를 운영 환경에 배포하기 전에 PostgreSQL, SQL Server,
Oracle, SQLite Provider를 실제 환경에서 수용하는 순서를 고정한다. 문서에는
비밀번호, Client Secret, Bearer Token을 기록하지 않는다.

## 1. 사전 조건

- 대상 VM의 API, Worker, PostgreSQL, SQL Server, Oracle 컨테이너가 실행 중이다.
- 테넌트가 Platform DB에 존재하고 테스트 주체가 활성 테넌트 구성원이다.
- 테스트 계정은 각 DB에서 필요한 Migration·Routine 검증 권한만 가진다.
- 운영에서는 TLS와 외부 Secret 저장소를 사용한다. 개발용 `Ssl Mode=Disable`은
  격리된 테스트망에서만 허용한다.

## 2. Provider별 Migration 적용

각 Provider에 대해 아래 순서로 `PlatformDbContext`, `WorkerDbContext`,
`DataSetDbContext`를 적용하고 결과를 기록한다.

1. Migration 적용 전 백업 또는 복구 지점을 생성한다.
2. `scripts/<provider>/Apply-Migrations.ps1 -Apply`를 실행한다.
3. 세 컨텍스트의 `__EFMigrationsHistory_*`가 최신 Migration을 기록하는지 확인한다.
4. 적용 결과가 실패하면 API를 재시작하거나 재시도하지 말고 오류·DB 상태를 보존한다.
5. 동일 명령을 다시 실행해 중복 생성 없이 최신 상태가 유지되는지 확인한다.

SQLite는 EF Core의 idempotent script를 사용하지 않고 일반 Migration Script 또는
애플리케이션의 개발용 Migration 경로를 사용한다.

## 3. 테넌트 바인딩 수용

Provider마다 별도의 바인딩을 생성한다. 복수 바인딩을 검증할 때는 반드시 응답의
`bindingId`를 저장하고 이후 Health, Metadata, Routine, Mutation 요청에 같은
`bindingId`를 전달한다. 기본 바인딩 선택에 의존하지 않는다.

바인딩 수용 기준:

- 생성 응답: `201 Created`, `status=Provisioning`
- 활성화 응답: `200 OK`, `status=Active`
- Health 응답: `200 OK`, `status=Healthy`
- 비밀번호나 Secret이 API 응답·로그에 나타나지 않음

## 4. 자동 검증

```powershell
pwsh -File ./scripts/tenant-routing/Verify-ExternalProviders.ps1 `
  -TenantId '<tenant-guid>' `
  -BindingId '<binding-guid>' `
  -Schema '<provider-schema>' `
  -ProviderHost '<provider-host>' `
  -TokenFile '<short-lived-token-file>'
```

스크립트는 다음을 모두 검사한다.

- Provider TCP 포트
- API·Worker Health
- API·Worker DB 연결 상태
- 지정 바인딩의 테넌트 DB Health
- 테이블·Routine Metadata
- 요청 시 Routine 실행
- 요청 시 복수 바인딩 정합성 결과

HTTP 200이어도 응답 상태가 `Healthy` 또는 `Connected`가 아니면 실패로 판정한다.

## 5. 복수 DB DML 수용

테스트 작업은 PostgreSQL → SQL Server → Oracle 순서로 수행하되, 각 단계에
명시적 `bindingId`와 동일한 멱등성 키를 사용한다.

1. 모든 단계 성공: 각 DB의 결과와 감사 기록을 확인한다.
2. 중간 단계 일시 오류: 재시도 후 같은 결과로 수렴하는지 확인한다.
3. 중간 단계 영구 오류: 앞선 성공 단계의 보상 Routine 실행과 보상 결과를 확인한다.
4. 같은 요청 재전송: 중복 DML 없이 기존 결과를 반환하는지 확인한다.
5. 기본 활성 바인딩을 변경한 뒤 재시도: 기존 `bindingId`로 계속 실행되는지 확인한다.

정합성 결과는 `Consistent`, `Inconsistent`, `Unknown` 중 하나여야 하며,
`Unknown`은 자동 성공으로 취급하지 않는다.

## 6. 보안·운영 수용

- 서비스 계정은 필요한 Audience와 테넌트 구성원 권한만 가진다.
- Access Token은 짧은 만료 시간을 사용하며 자동 갱신 파일은 root 소유·제한 권한으로 보호한다.
- Secret은 `.env`, Git, 명령행, 로그에 저장하지 않는다.
- JWKS·인증서 회전 후 API가 새 키를 수용하는지 확인한다.
- API·Worker·DB 로그에 비밀번호, 토큰, 원문 Routine 파라미터가 노출되지 않는다.
- 백업 복구와 Migration 실패 복구 절차를 실제로 한 번 수행한다.
- 재부팅 후 systemd·Docker Compose가 API, Worker, DB를 자동 기동한다.

## 7. 최종 승인 증적

최종 승인에는 다음 자료를 함께 보관한다.

- Provider별 Migration 실행 결과와 Migration History 조회 결과
- Provider별 `Verify-ExternalProviders.ps1` 전체 PASS 출력
- 복수 DB 성공·재시도·보상·정합성 출력
- 재부팅 후 Health 출력
- CI Run 링크와 병합된 `main` 커밋
- 백업·복구 및 Secret 보안 점검 결과

위 자료가 모두 있으면 해당 환경을 운영 수용 완료로 판정한다. 자료가 하나라도
없거나 `401`, `409`, `500`, `503`, `Unknown`이 남아 있으면 완료로 판정하지 않는다.
