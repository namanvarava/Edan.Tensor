# DataSet 검증 v1 상세 계약

## 문서 상태

이 문서는 기존 DataSet Validation Handler 요구사항의 승인된 v1 계약이다.
사용자는 2026-11-07에 이 문서의 범위와 아래 수용 기준을 승인했다.
구현은 이 계약을 기준으로 하며, DuckDB·Parquet·Commercial·Licensing·WPF는 이번 릴리스에 포함하지 않는다.

## 승인된 최초 지원 범위

| 항목 | 승인된 계약 |
| --- | --- |
| 콘텐츠 포맷 | UTF-8 CSV와 JSON 객체 배열을 지원한다. 콘텐츠 저장 엔진과 파일 포맷은 별도 식별자로 관리한다. |
| 저장 엔진 | 기존 SQLite 콘텐츠 어댑터를 사용한다. DuckDB·Parquet 지원은 별도의 실제 어댑터와 포맷 계약을 구현한다. |
| 프로필 소유권 | 프로필은 Tenant에 소속되며 다른 Tenant의 DataSet에서 참조할 수 없다. |
| 프로필 버전 | 프로필 ID와 증가하는 revision으로 불변 실행 계약을 식별한다. 수정은 새 revision을 생성한다. |
| 입력 | 요청은 DataSet ID, 기대 DataSet 버전, Schema revision 및 프로필 ID/revision만 포함한다. 경로와 SQL은 받지 않는다. |
| 최초 검증 규칙 | 필수값, 문자열·정수·소수·불리언 형식, 수치 범위, 필드 조합의 중복 여부를 지원한다. |
| 필드 접근 | v1은 최상위 필드만 지원한다. 중첩 경로와 사용자 실행 코드는 지원 계약을 별도로 정한다. |
| 오류와 경고 | 프로필은 규칙별 Error 또는 Warning을 지정한다. Error가 존재하면 검증 판정은 Invalid가 된다. |
| 결과 | 실행 요약은 전체 레코드 수, Error/Warning 수, 규칙별 집계를 보관한다. 상세 결과는 Tenant 범위의 불투명 참조로 조회한다. |
| 상세 결과 상한 | 실행은 전체 레코드를 검증하고 상세 항목은 설정된 상한까지만 보관한다. 잘림 여부와 전체 오류 수를 함께 기록한다. |
| 민감 값 | 결과는 행 번호·필드명·오류 코드만 기본 보관한다. 원문 셀 값과 연결 문자열은 기록하지 않는다. |
| 결과 보존 | 결과는 자동 삭제하지 않고 Tenant 관리자의 명시적 삭제를 지원한다. 삭제 시 소유권과 실행 상태를 검증한다. |
| 자원 제한 | 검증기는 업로드 한도·최대 레코드 수·필드 길이·작업 취소를 준수한다. 제한 초과는 잘못된 유효 판정으로 처리하지 않는다. |
| 실행 예약 멱등성 | `Idempotency-Key`는 Tenant 범위에서 최대 200자로 저장한다. 같은 키와 같은 DataSet·버전·스키마·프로필 revision 요청은 기존 실행과 Worker 작업으로 수렴하고, 다른 요청은 충돌로 거부한다. |

## 실행과 정합성

1. Handler는 Tenant 범위로 DataSet과 프로필 revision을 읽고 기대 버전과 Schema revision을 검증한다.
2. Handler는 불투명 Storage Reference와 등록된 어댑터로 콘텐츠를 읽는다.
3. Handler는 진행률과 체크포인트에 입력 revision과 처리 위치를 기록한다.
4. 체크포인트 재개 시 콘텐츠와 프로필 revision이 바뀌었으면 기존 실행을 계속하지 않는다.
5. `Idempotency-Key`는 같은 Tenant의 같은 DataSet·버전·스키마·프로필 revision 요청을 하나의 검증 실행과 Worker 작업으로 수렴시키며, 다른 요청에 재사용하면 충돌로 거부한다.
6. 결과 저장은 실행 식별자에 대해 멱등적으로 처리하며, 현재 Lease와 Fencing Token 검증에 실패한 실행은 공개 결과를 갱신하지 못한다.
7. Worker의 ResultReference에는 결과 참조만 기록한다.
8. 검증 대상의 Error는 정상 실행의 Invalid 결과이며, 저장소 장애는 재시도 정책에 따라 처리한다.
9. 실행 레코드와 Worker 작업 사이에 키 충돌이 발생하면 새로 만든 실행 레코드를 정리하여 고아 실행을 남기지 않는다. 이미 존재하던 실행은 삭제하지 않는다.

## 승인된 선택

사용자는 위 지원 포맷, 최초 규칙, 상세 결과 보존 정책을 승인했다.
Validation Profile은 별도의 명시적인 `fields`와 `duplicateSets` 배열을 포함해야 하며, 임의의 JSON 객체를 검증 스키마로 해석하지 않는다.
Profile 문서의 필드 정의 형식은 구현에서 다음과 같이 고정한다.

```json
{
  "fields": [
    { "name": "id", "type": "string", "required": true, "severity": "Error" },
    { "name": "amount", "type": "decimal", "min": 0, "max": 1000000, "severity": "Error" }
  ],
  "duplicateSets": [
    { "fields": ["id"], "severity": "Error", "code": "duplicate.id" }
  ],
  "limits": { "maxRecords": 100000, "maxFieldLength": 4096, "maxDetailItems": 1000 }
}
```

`type`은 `string`, `integer`, `decimal`, `boolean` 중 하나이며, `severity`는 `Error` 또는 `Warning`이다. 문자열 필드는 `minLength`·`maxLength`를 선택적으로 사용하고, 수치 필드는 `min`·`max`를 선택적으로 사용한다. 모든 필드는 최상위 필드명만 지정하며 중첩 경로와 실행 코드는 허용하지 않는다.

Commercial·Licensing·WPF는 이번 릴리스 계약에 포함되지 않는다. 해당 영역은 다음 릴리스에서 업무 흐름, 권리·만료·오프라인 정책, 결제 공급자 및 화면 수용 기준을 별도로 승인한 뒤 진행한다.

## 구현 완료를 판단할 증거

- 허용 포맷과 잘못된 포맷, 각 검증 규칙 및 Error/Warning 집계가 테스트를 통과한다.
- 다른 Tenant의 DataSet·프로필·결과 접근이 거부된다.
- 입력 revision 변경, 취소·재개, 오래된 Fencing Token의 결과 공개가 검증된다.
- 원문 값과 내부 저장 위치가 API·로그·결과 참조로 노출되지 않는다.
- 최신 main 대상 GitHub Actions 전체 CI가 성공한다.

구현은 검증 실행 레코드에 현재 Worker Fencing Token을 저장하고, 행 진행·완료·실패·재시도 대기 전환을 같은 Token과 낙관적 동시성으로 보호한다. 새 Worker가 같은 실행을 인계하면 이전 Token의 결과 쓰기는 거부되며, 이 경계는 DataSet Infrastructure 회귀 테스트로 확인한다.
