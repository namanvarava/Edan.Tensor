# 2026-11-06 구현 및 CI 검증 현황

## 판정

CI 실패 복구 완료. 프로젝트 전체 완료는 미선언.
기능 문서의 후속 과제는 승인된 상세 완료 계약과 구분하며, 테스트 통과만으로 운영 적합성을 승인하지 않는다.

## 공식 검증 증거

- 코드 기준: `14db2e38ed5eba66f7737d0861b10595b7ff5362`
- [Actions Run #612](https://github.com/namanvarava/Edan.Tensor/actions/runs/42167385135)
- Restore / Build / Test 성공. 183개 통과, 실패 0, 건너뜀 0.
- SharedKernel 8, Platform Domain 27 / Application 30 / Infrastructure 12,
  DataSet Domain 6 / Application 8 / Infrastructure 8,
  Worker Domain 21 / Infrastructure 54, API 통합 9.
- API 통합 테스트는 이전 두 실패 및 호스트 간 실제 DbContext 경로 격리 회귀 검증을 포함한다.
- 이 문서와 함께 추가하는 아키텍처 테스트는 위 183개에 포함되지 않는다. 해당 커밋의 후속 Actions로 별도 검증한다.

## 실패 원인과 수정

Run #611은 Worker TaskType UNIQUE 충돌과 Tenant 소속 수 불일치(2 대신 6)로 실패했다.
Program은 CreateBuilder 직후 설정을 읽어 저장소를 등록하지만, 테스트는 ConfigureWebHost의
ConfigureAppConfiguration에서 임시 DB 설정을 주입했다. 기본 DB 사용과 이전 데이터 누적을 막기 위해
ApiHostFactory.CreateHost의 ConfigureHostConfiguration으로 설정을 앞당겼다.
Platform / Worker / DataSet 메타데이터 / 콘텐츠 DB를 각각 고유 임시 경로로 지정하고
Pooling=False로 호스트 종료 후 Windows 파일 삭제 시 연결 풀 잠금을 방지했다.
실제 Platform / Worker / DataSet DbContext가 두 호스트 간 경로를 공유하지 않는 회귀 테스트가 Run #612에서 통과했다.
기존 오염 DB는 삭제하지 않았으며 제품의 중복 제약이나 권한 검증을 완화하지 않았다.

## 구현과 요구사항 비교

| 영역 | 확인된 구현 | 남은 조건 |
| --- | --- | --- |
| Platform | Tenant·Organization·Project·소속·Policy·감사·아카이브 | 실제 Identity Provider, 키 회전, 외부 계정 연결 등 운영 보안 계약과 검증 |
| DataSet | 독립 DbContext·Migration, 불투명 저장 참조, SQLite 콘텐츠 어댑터, 계층형 quota | 검증 프로필·규칙 버전·상세 결과 저장 계약, 검증 Handler, DuckDB·Parquet 어댑터 |
| Worker | 전용 DB, 영속 카탈로그, Lease/Fencing, 체크포인트, 재시도·DLQ·재처리 | 실제 프로세스 강제 종료 및 외부 전달 응답 유실의 종단 간 시험 증거 |
| 종료·순회 | Registry/Dispatcher 단일 handoff 예산, Tenant cursor LRU 상한 | Dispatcher LRU 직접 회귀 테스트는 본 후속 커밋에 추가; 종료 경로 직접 회귀 테스트는 잔여 |
| Commercial / Licensing | 분리된 Contracts·Domain·Application·Infrastructure 기반 프로젝트 | 업무 규칙·라이선스·결제 상세 요구사항과 수용 기준 |
| 클라이언트 | 현재 소스 트리에 WPF 클라이언트 없음 | 화면·업무 흐름 및 완료 범위 확정 |

DataSet Validation 설계 문서의 선행 조건 중 식별자·소유 범위·저장 참조·스키마 revision은 구현되었다.
검증 프로필과 규칙 버전, 오류·경고/상세 결과 저장 계약은 확인되지 않았다.
이 부분을 임의의 JSON 구문 검사로 대체하여 완료 처리하지 않는다.
WorkerHost에 등록된 실제 업무 Handler는 DataSet 저장량 스냅샷이다.

## 아키텍처 검토

소스 csproj 참조를 대조하여 SharedKernel 독립성, Domain의 SharedKernel 의존,
Application의 동일 모듈 Domain/Contracts 및 DI 추상화 의존,
Commercial/Licensing 내부 구현 간 참조 부재를 확인했다.
DataSet은 Platform 도메인을 직접 참조하지 않고 Host의 scope validator로 경계를 연결한다.
WorkerDbContext는 PlatformDbContext와 분리되어 있다.
같은 커밋의 ArchitectureDependencyTests가 주요 프로젝트 참조 규칙을 자동 검사한다.
이는 런타임 Tenant 보안 시험과 운영 보안 검증을 대체하지 않는다.

## 잔여 우선순위와 재개 지점

1. 아키텍처 테스트 추가 커밋 `1c4d19c5cc0b13137554428b29420250b4c08476`의 [Run #613](https://github.com/namanvarava/Edan.Tensor/actions/runs/42168208608)은 성공했다. LRU 테스트 후속 커밋의 CI 결과를 확인한다.
2. Dispatcher LRU 상한·퇴출·재접근·삭제 검증을 추가했다. Registry/Dispatcher 단일 종료 예산의 직접 회귀 검증은 잔여다.
3. DataSet 검증 프로필·지원 포맷·오류/경고·결과 보존 계약 확정 후 Validation Handler 구현.
4. 실제 프로세스 장애 복구·외부 호출 중복 전달 시험 환경과 증거 수립.
5. Identity Provider·Commercial·Licensing·WPF·분석 어댑터의 승인된 기능 범위와 수용 기준 확정.

열린 Issue가 없다는 사실은 위 제품 요구사항이 완료되었다는 증거가 아니다.
README와 일부 초기 설계 문서는 과거 기반 단계의 표현을 포함한다.
본 문서의 구현 현황은 지정 SHA 기준이며 미래 작업의 승인을 새로 부여하지 않는다.

## 2026-11-06 Worker 종료 계약 추가 검증

- 수정 커밋 `0059f7a7369d145efaaa71069bedfb8902e40855`의 [Actions Run #615](https://github.com/namanvarava/Edan.Tensor/actions/runs/42169916191)는 Restore / Build / Test를 통과했다.
- 총 190개 테스트가 통과했으며 실패와 건너뜀은 0개다.
- 인계 콜백이 Task를 반환하기 전에 동기 대기하면 기존 WaitAsync가 호출되지 않는 종료 제한의 빈틈을 수정했다.
- Registry는 콜백 호출을 별도 작업에서 시작하고 제한된 시간만 기다린다. 취소를 무시하는 콜백 자체를 강제 종료하지는 않는다.
- 이 문서와 함께 Dispatcher의 실제 종료 경로가 Registry와 동일한 인계 예산을 공유하는 회귀 테스트를 추가한다. 이 추가 테스트의 실행 증거는 해당 커밋의 후속 Actions로 확인해야 한다.
- 기존 인수인계 문서는 CI 복구·운영 경계 문서이며 전체 제품의 상세 기능 계약은 포함하지 않는다. Commercial/Licensing은 실제 소스에서도 AssemblyMarker 기반 상태이며, 결제 정책·라이선스 권리·WPF 업무 화면의 완료 기준을 추정하여 완료 처리할 수 없다.
