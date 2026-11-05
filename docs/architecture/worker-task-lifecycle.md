# Worker 작업 수명주기와 종료 인계 설계

## 목적과 범위

이 문서는 WorkerHost의 작업 수명주기, 비정상 종료 복구, Dead Letter Queue(DLQ), 정상 종료 시 작업 인계에 필요한 구현 계약과 운영 설정을 정의합니다. Domain 상태 전이, 영속 Lease/체크포인트, 재시도·DLQ, 실행 Registry 및 디스패처가 구현되어 있으며, 아래 핵심 보장과 검증 기준은 지속적인 회귀 검증을 위한 계약으로 유지합니다.

작업 저장소는 플랫폼 메타데이터 저장소와 분리 가능한 계약으로 설계합니다. 초기 SQLite 구현은 플랫폼 메타데이터 DB를 사용할 수 있지만, 작업량 증가 시 전용 저장소나 메시지 브로커로 이전할 수 있어야 합니다.

## 핵심 보장

- 작업 상태와 Lease 소유권은 영속 저장소를 기준으로 판단합니다.
- Tenant 소유 작업은 모든 조회·갱신·인계 조건에 Tenant 식별자를 포함합니다.
- Lease를 획득하거나 갱신할 때 단조 증가하는 Fencing Token을 발급합니다. 만료된 Worker의 쓰기는 현재 Token과 일치하지 않으면 거부합니다.
- 외부 시스템 전달은 기본적으로 최소 한 번(at-least-once) 전달을 전제로 합니다. Exactly-once를 보장한다고 주장하지 않으며, 작업별 Idempotency Key와 수신 측 중복 제거를 사용합니다.
- 체크포인트는 작업 데이터와 버전, 처리 위치, 외부 전달 단계 및 갱신 시각을 함께 보존합니다. 비밀 값과 인증 정보는 체크포인트에 기록하지 않습니다.
- 작업을 다른 Worker에 인계했다고 확정하는 시점은 체크포인트 저장과 Lease 상태 변경이 원자적으로 완료된 뒤입니다.

## 작업 상태

| 상태 | 의미 | 허용되는 다음 상태 |
| --- | --- | --- |
| Pending | 실행 대기 | Leased, Cancelled |
| Leased | Worker가 Lease를 획득 | Running, Pending, DeadLettered |
| Running | 작업 실행 중 | Checkpointed, Succeeded, RetryScheduled, DeadLettered, Cancelled |
| Checkpointed | 복구 가능한 체크포인트 저장 완료 | Leased, Running, RetryScheduled, Cancelled |
| RetryScheduled | 재시도 시각 대기 | Leased, DeadLettered, Cancelled |
| Succeeded | 작업 완료 | 없음 |
| DeadLettered | 자동 재시도 한도 또는 정책 초과 | 운영자 재처리 또는 종료 |
| Cancelled | 정책 또는 사용자 요청으로 취소 | 정책상 재개가 허용되는 경우에만 Pending |

상태 전이는 저장소에서 현재 상태와 버전을 조건으로 원자적으로 수행합니다. 유효하지 않은 전이는 거부하고 감사 기록을 남깁니다.

## Lease와 Fencing Token

작업 Lease는 최소한 다음 정보를 보유합니다.

- TenantId 및 작업 식별자
- Lease 소유 Worker 식별자와 고유 LeaseId
- 단조 증가 FencingToken
- Lease 만료 시각(UTC)
- 동시성 제어용 Version

Lease 획득·갱신·완료·해제는 현재 LeaseId, FencingToken, Version 및 만료 여부를 함께 검증합니다. 만료된 Lease를 보유했던 Worker는 체크포인트, 완료 상태, 외부 전달 상태를 갱신할 수 없습니다. 시간 비교는 저장소 공급자별 변환 문제를 피하도록 UTC 정규화 기준을 적용하고, SQLite에서 DateTimeOffset 비교를 SQL로 직접 수행하지 않습니다.

## 정상 종료 절차

1. 호스트 종료 신호를 받으면 신규 작업 Lease 획득과 신규 메시지 수신을 중단합니다.
2. 실행 중인 작업 목록을 고정하고, 작업별 종료 정책과 종료 유예 시간을 적용합니다.
3. 유예 시간 안에 완료 가능한 작업은 정상 완료 처리합니다.
4. 완료할 수 없는 작업은 취소 신호를 전달하고 안전한 처리 경계에서 체크포인트를 저장합니다.
5. 체크포인트와 외부 전달 단계가 일관됨을 확인한 뒤 현재 FencingToken으로 Lease를 해제하거나 재시도 대기 상태로 전환합니다.
6. 저장 또는 소유권 검증에 실패하면 Lease를 임의로 해제하지 않습니다. Lease 만료 후 복구 경로가 인수하도록 둡니다.
7. 유예 시간 종료 후에도 남은 작업은 호스트 종료를 지연시키지 않으며, 영속 상태와 만료된 Lease를 통해 재시작 시 복구합니다.

종료 유예 시간은 설정으로 관리하고 상한을 둡니다. 작업별 정책은 유예 시간 안에서만 우선순위를 조정할 수 있으며, 프로세스 강제 종료를 막을 수 없습니다.

## 외부 전달과 체크포인트

외부 호출 직전과 직후의 체크포인트 경계를 명확히 구분합니다. 외부 시스템이 요청을 수락했지만 Worker가 확인 응답을 저장하기 전에 종료될 수 있으므로, 재시도 시 동일한 Idempotency Key를 사용합니다. 수신 측 멱등 처리를 지원하지 않는 외부 시스템은 중복 전달 가능성을 작업 계약에 명시하고 별도 보정 절차를 제공합니다.

체크포인트 저장에는 작업 버전과 FencingToken 조건을 포함합니다. 오래된 Worker가 저장한 체크포인트가 최신 상태를 덮어쓰지 못하도록 합니다.

## DLQ 및 재처리

DLQ 이동은 예외 발생만으로 즉시 수행하지 않습니다. 재시도 가능 여부, 시도 횟수, 오류 분류, 재시도 지연 및 작업별 정책을 평가합니다. 일시 오류는 지수형 지연과 최대 지연을 적용한 재시도 대상으로 남기고, 영구 오류 또는 재시도 한도 초과 작업은 DLQ로 이동합니다.

DLQ 항목은 원본 작업 식별자, TenantId, 마지막 오류 코드, 오류 요약, 시도 횟수, 마지막 FencingToken, 생성 시각 및 체크포인트 참조를 보유합니다. 비밀 값, 원문 인증 정보 및 불필요한 개인정보는 저장하지 않습니다. 재처리는 원본 이력을 보존하고 별도 재처리 이벤트를 기록합니다.

## 감사와 관측성

다음 이벤트를 구조화된 로그 및 영속 감사 기록으로 남깁니다.

- 작업 Lease 획득·갱신·상실·해제
- 체크포인트 저장 및 복구
- 정상 완료·취소·재시도 예약·DLQ 이동
- 종료 신호 수신·신규 작업 중단·유예 만료
- 다른 Worker의 인수 및 Fencing Token 불일치 거부
- 운영자 재처리 및 최종 종료

로그에는 TenantId, 작업 식별자, Worker 식별자, LeaseId, FencingToken, 상관관계 식별자 및 결과 코드를 포함하되 비밀 값은 포함하지 않습니다.

## 구현 순서와 검증 기준

1. Domain 작업 모델과 상태 전이 검증
2. Application 작업 실행·체크포인트·Lease 저장소 계약
3. SQLite 영속 구현과 동시성/Fencing Token 제약
4. WorkerHost의 신규 작업 차단 및 정상 종료 조정기
5. 재시도·DLQ·운영자 재처리 경로
6. 강제 종료·Lease 만료·중복 전달·다중 Worker 경쟁 통합 테스트

완료 기준은 정상 종료뿐 아니라 체크포인트 저장 실패, Lease 만료, 오래된 Worker의 쓰기 거부, 외부 전달 응답 유실, 프로세스 강제 종료 후 복구, Tenant 간 작업 격리까지 자동 테스트로 입증하는 것입니다.


## WorkerHost 디스패처 운영 설정

WorkerHost는 다음 설정을 사용합니다. 설정값은 코드에서 안전 범위로 제한되며, 범위를 벗어난 값은 아래 clamp 범위의 최솟값 또는 최댓값으로 보정됩니다.

| 설정 키 | 기본값 | 허용 범위 | 목적 |
| --- | ---: | ---: | --- |
| `Worker:PollIntervalSeconds` | 5 | 1–60초 | 디스패처의 반복 조회 간격 |
| `Worker:MaxConcurrentTasks` | 4 | 1–128 | Worker 프로세스의 동시 실행 작업 상한 |
| `Worker:LeaseRecoveryBatchSize` | 100 | 1–500 | 한 번의 디스패치 주기에서 복구할 만료 Lease 최대 건수 |
| `Worker:TenantCandidateBatchSize` | 16 | 1–100 | Tenant별 준비 작업 조회 배치 크기 |
| `Worker:TenantCandidateScanLimit` | 500 | 배치 크기–5,000 | Tenant별 한 주기에서 검사할 후보 작업 상한 |
| `Worker:TenantTaskCursorLimit` | 10,000 | 500–100,000 | Tenant별 작업 순회 커서 메모리 상한 |
| `Worker:LeaseSeconds` | 300 | 3–3,600초 | 작업 Lease 유효 기간 |
| `Worker:ShutdownGraceSeconds` | 30 | 1–300초 | 정상 완료를 기다리는 종료 유예 시간 |
| `Worker:ShutdownHandoffSeconds` | 15 | 1–120초 | 체크포인트/인계 및 남은 실행 정리에 사용하는 전체 추가 시간 예산 |

### Tenant 작업 커서 관리

디스패처는 Tenant 목록과 Tenant별 작업 목록에 keyset cursor를 사용하여 매 주기마다 처음부터 전체 후보를 조회하지 않습니다. 작업 커서는 해당 Tenant의 후보를 이어서 조회하기 위한 프로세스 내 메모리 상태이며, 영속 작업 상태나 Lease 소유권을 대신하지 않습니다.

준비 Tenant 수가 장기간 증가하거나 삭제된 Tenant의 커서가 남는 경우를 대비해 `Worker:TenantTaskCursorLimit`으로 커서 수를 제한합니다. 상한을 넘으면 현재 처리 중인 Tenant를 제외한 일부 커서를 제거합니다. 제거된 Tenant는 다음 조회에서 처음부터 후보 순회를 재개할 수 있으므로 추가 조회 비용이 발생할 수 있지만, 작업의 영속 상태와 Lease 기반 정합성에는 영향을 주지 않습니다. 이 설정을 낮게 조정하면 메모리 사용량은 줄지만 후보 재검색이 늘어날 수 있습니다.

### 종료 시간 예산

`Worker:ShutdownHandoffSeconds`는 Registry의 인계 요청과 디스패처의 남은 실행 정리가 공유하는 단일 시간 예산입니다. Registry가 소비한 시간(`HandoffElapsed`)을 차감한 나머지 시간만 디스패처가 기다립니다. 따라서 두 단계가 각각 전체 인계 제한 시간을 다시 소비하여 종료가 설정값의 두 배까지 늘어나는 동작을 피합니다.

운영 환경에서는 작업 처리 시간, Tenant 수, DB 성능 및 재시도 부하를 관찰한 뒤 동시 실행 수와 후보 스캔 상한을 조정합니다. 동시 실행 수만 크게 높이면 DB 연결·외부 시스템 부하가 함께 증가할 수 있으므로, 작업 처리량과 저장소 용량을 함께 검증해야 합니다.
