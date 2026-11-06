# 외부 전달 계약

## 범위

이번 릴리스는 로컬 테스트 수신기와 허용된 HTTP 또는 HTTPS 수신기로 JSON 이벤트를 전달하는 기반을 제공합니다. 전달은 업무 결제·권리·만료 흐름과 연결하지 않으며, Worker의 작업·Lease·재시도·DLQ 경계를 사용합니다.

## 요청과 보안 경계

- API 요청은 `TargetUri`, `Payload`, `MaxAttempts`를 받으며 `Idempotency-Key` 헤더가 필수입니다.
- API는 Tenant 관리자만 전달을 생성하고 Tenant 구성원은 상태만 조회합니다.
- 대상 URI는 사용자 정보가 없는 HTTP 또는 HTTPS 절대 URI여야 합니다. 운영 설정의 `ExternalDelivery:AllowedHosts`에 등록된 호스트만 허용합니다.
- `ExternalDelivery:AllowLoopback=true`는 로컬 테스트 수신기에서만 사용합니다. 운영에서는 loopback 허용을 끕니다.
- Worker 작업 payload와 API 응답에는 전달 본문을 다시 넣지 않고 불투명한 전달 식별자 참조만 사용합니다.
- 저장소에는 Idempotency Key와 상태·시도 횟수·오류 코드만 보관하며 수신기 응답 본문은 보관하지 않습니다.

## 실행 계약

1. 같은 Tenant와 Idempotency Key가 같은 대상·본문으로 다시 들어오면 기존 전달을 반환하고 중복 작업을 만들지 않습니다.
2. Worker는 모든 시도에 같은 `Idempotency-Key` HTTP 헤더를 보냅니다.
3. 2xx 응답은 `Succeeded`로 종료합니다.
4. 4xx 응답은 재시도하지 않는 `Failed`로 종료합니다.
5. 5xx, timeout, 연결 종료로 응답을 확인하지 못한 경우는 `Pending`과 재시도 가능한 오류 코드로 남깁니다. Worker 재시도 한도를 소진하면 기존 DLQ 정책에 따라 격리합니다.
6. Lease 인계 또는 취소 시 실행 중 HTTP 요청은 취소를 전달하고 종료를 기다린 뒤 작업 참조만 반환합니다. 인계 후 이전 실행이 결과 상태를 덮어쓰지 않습니다.

## 검증

Worker 인프라 테스트는 실제 Windows loopback TCP 수신기에서 첫 요청의 응답 유실 후 재시도를 실행하고 동일 Idempotency Key를 확인합니다. 같은 테스트 묶음에서 HTTP 5xx, timeout, 응답 본문 미보관 및 Tenant 격리를 확인합니다. API 통합 테스트는 opaque Worker 참조와 동일 Key의 멱등 경계를 확인합니다.

운영 수신기 검증은 이번 로컬 범위에 포함하지 않습니다. 실제 수신기 주소·인증서·네트워크 권한이 제공되면 동일 계약으로 timeout, 5xx, 응답 유실, 재시도 및 DLQ를 별도 실행합니다.
