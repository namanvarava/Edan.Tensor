# 초대 lifecycle 및 Worker 결과 저장 계약

- 확정 기준일: 2026-11-16
- 적용 브랜치: `main-auto`
- 범위: `PLT-USR-001`, `WRK-TSK-001`

## Invitation lifecycle

초대는 `User`와 분리된 `Invitation` aggregate로 저장한다. `platform_invitations`에는 raw token이 들어가지 않고 SHA-256 hash만 저장한다.

| 상태 | 전이 |
|---|---|
| `Pending` | 생성, 재전송 |
| `Accepted` | 유효한 token 수락 |
| `Expired` | 만료 시각 경과 |
| `Revoked` | Tenant 관리자 철회 |

생성·재전송 API는 `InvitationView`의 Id, TenantId, UserId, Email, DisplayName, Status, CreatedAtUtc, ExpiresAtUtc, AcceptedAtUtc, RevokedAtUtc, ResendCount만 반환한다. raw token은 API 응답에 넣지 않고 `IInvitationDelivery` Adapter에 전달한다. 운영에서는 이메일/메시지 Adapter를 주입하고, Development에서는 통합 테스트 전용 메모리 Adapter만 사용한다.

인증·인가 경계는 다음과 같다.

- 생성·조회·재전송·철회: Tenant 관리자 + route Tenant + `X-Tenant-Id` + active membership
- 수락: 로그인 전 허용되는 token-only endpoint이며 token hash·만료·상태·연결 User를 검증한 뒤 User를 Active로 전환
- 모든 조회는 Invitation의 TenantId를 조건에 포함한다.

## Worker raw result

작업 DB에는 기존 `WorkerTask.ResultReference`를 유지하고, 실제 bytes는 `IWorkerResultStore`로 분리한다. 현재 개발 구현은 `FileSystemWorkerResultStore`이며 저장 참조는 `worker-file:{tenantId}/{taskId}/{resultId}` 형식이다. 운영 Object Storage는 이 계약을 구현하는 Adapter로 교체한다.

`WorkerResultMetadata`는 TenantId, TaskId, ResultReference, ContentType, Length, Sha256, CreatedAtUtc를 가진다. API는 다음 계약을 제공한다.

- `GET /api/platform/tenants/{tenantId}/worker/tasks/{taskId}/result`: metadata와 opaque reference
- `PUT /api/platform/tenants/{tenantId}/worker/tasks/{taskId}/result/raw`: 성공 작업에 bytes 저장
- `GET /api/platform/tenants/{tenantId}/worker/tasks/{taskId}/result/raw`: Tenant 검증 후 원본 bytes streaming

결과 저장·조회·삭제는 `IWorkerResultStore`를 통해서만 수행하며 TenantId와 TaskId가 참조 경로와 일치하지 않으면 접근하지 않는다. raw result API는 Tenant 관리자 정책을 유지하고 64 MiB 요청 상한을 적용한다.

## 검증 상태

API Host build와 임시 SQLite migration 기동을 확인했고, GitHub Actions run `43736265722`에서 전체 Build/Test, HTTP smoke 2건, WPF STA smoke 1건이 모두 통과했다. 로컬 VSTest testhost는 연결되지 않았지만 원격 Windows runner가 실제 smoke를 실행했으므로 CI 검증 결과를 기준으로 기록한다.
