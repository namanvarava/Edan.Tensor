# Client API 연동 계약

이 문서는 WPF 화면이 아직 없는 환경에서도 현재 승인된 Platform·DataSet·Worker API 흐름을 안전하게 소비할 수 있도록 추가한 typed API Client와 공유 계약의 사용 범위를 고정합니다.

## 제공 모듈

- `EDAN.Tensor.Client.Contracts`는 Tenant 소속, DataSet, 검증 실행, Worker 작업·DLQ의 요청·응답 계약을 제공합니다.
- `EDAN.Tensor.Client`는 `HttpClient` 기반 `TensorApiClient`를 제공하며, 인증 토큰·Tenant 헤더·Idempotency 헤더·Replay 헤더를 요청 경계에서 일관되게 구성합니다.
- Client는 API Host의 Tenant 경로와 `X-Tenant-Id`를 같은 식별자로 설정하여 경로와 선택 헤더가 어긋나는 요청을 만들지 않습니다.
- Client는 API의 `{ Code, Description }` 오류 계약을 `TensorApiException`의 상태 코드·오류 코드·설명으로 보존합니다. 토큰과 응답 본문을 로그로 남기지 않습니다.

## 사용 예시

```csharp
using EDAN.Tensor.Client;

using var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.example.test")
};
var api = new TensorApiClient(httpClient, new TensorApiClientOptions
{
    DefaultTenantId = tenantId,
    AccessTokenProvider = cancellationToken =>
        ValueTask.FromResult<string?>(accessToken)
});

var memberships = await api.GetMyMembershipsAsync(cancellationToken);
var dataSet = await api.CreateDataSetAsync(
    tenantId,
    new CreateDataSetRequestContract(organizationId, projectId, "고객 데이터"),
    cancellationToken);
var task = await api.EnqueueWorkerTaskAsync(
    tenantId,
    new EnqueueWorkerTaskRequestContract("dataset.validation.v1", payloadReference),
    idempotencyKey,
    cancellationToken);
```

실제 서비스에서는 `AccessTokenProvider`가 운영 인증 공급자에서 만료되지 않은 토큰을 가져오도록 구성해야 합니다. Development Bypass는 이 Client의 인증 계약을 대체하지 않으며, 운영 환경의 토큰·Tenant 권한·라이선스 검증을 우회하는 설정을 제공하지 않습니다.

## 오류와 스트림 규칙

- `GetDataSetAsync`, `GetDataSetValidationAsync`, `GetWorkerTaskAsync`는 Tenant 범위에서 404를 반환하면 `null`을 반환합니다.
- 생성·변경·작업 등록 요청의 400·409·413·500 응답은 `TensorApiException`으로 전달하며 API가 제공한 안정적인 오류 코드를 보존합니다.
- 검증 예약과 Worker 등록에는 호출자가 제공한 `Idempotency-Key`를 그대로 전달하되 200자 제한을 Client에서도 먼저 확인합니다.
- DLQ 재처리에는 호출자가 생성한 `Replay-Request-Id` GUID가 필요합니다.
- 콘텐츠 업로드 스트림은 요청이 끝나면 `HttpClient` 요청 콘텐츠와 함께 해제되므로 호출자는 업로드 완료 후 해당 스트림을 재사용하지 않습니다. 콘텐츠 다운로드 스트림을 `Dispose`하면 HTTP 응답도 함께 해제됩니다.

## 범위 경계

이 모듈은 WPF 화면, 결제 공급자, Commercial entitlement, Licensing 발급·검증 업무를 구현하지 않습니다. 해당 기능은 승인된 업무·화면·보안 계약이 제공된 뒤 이 Client 계약 위에 별도 추가합니다. API가 지원하는 SQLite·CSV·JSON 검증 v1 범위를 임의로 DuckDB·Parquet으로 확장하지 않습니다.
