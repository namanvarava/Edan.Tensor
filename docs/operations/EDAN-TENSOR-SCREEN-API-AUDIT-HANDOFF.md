# EDAN.Tensor 화면 API·DTO 정합성 전수 조사 및 후속 개발 인계 지시서

- 문서 목적: 다른 ChatGPT 계정 또는 ChatGPT Work에서 본 작업을 중단 없이 이어가기 위한 실행 기준
- 저장소: `namanvarava/Edan.Tensor`
- 작업 브랜치: `main-auto`
- 보호 브랜치: `main` (직접 변경·병합 금지)
- 기준일: 2026-11-15
- 현재 작업 단계: Screen Registry 및 15개 화면 기능 명세 작성 완료 → 실제 API·DTO 정합성 전수 조사 시작
- 작업 방식: 저장소 현황 재확인 → 근거 기반 조사 → 문서화 → 자동 검증 → 결과 보고 및 커밋

---

## 1. 최종 목표

Screen Registry에 등록된 15개 화면의 기능 명세를 실제 소스 코드와 대조한다. API Endpoint, HTTP Method, Route, Request·Response DTO, 검색·페이징, 인증·인가, Application Service, DI 등록, 테스트 근거를 확인하여 다음 내용을 완성한다.

1. 화면별 실제 지원 기능과 미지원 기능을 명확하게 구분한다.
2. 화면 명세에 기록된 입력·출력·검색 조건을 실제 API 계약과 대조한다.
3. 인증·인가 정책을 실제 서버 코드 기준으로 확인한다.
4. 화면별 API 연결 관계와 소스 경로를 추적할 수 있게 한다.
5. 기존 화면 기능 명세의 잘못된 추정·과장·누락을 보정한다.
6. 추가 구현이 필요한 기능을 우선순위와 근거가 있는 작업 목록으로 만든다.
7. 이후 WPF/MVVM UI 아키텍처 설계와 실제 화면 구현으로 이어질 수 있는 검증된 기준을 제공한다.

이번 1차 작업은 **조사·검증·문서화**다. UI를 구현하거나 API 계약을 임의 변경하는 작업이 아니다.

## 2. 작업 시작 전 필수 확인

작업을 시작할 때 현재 저장소 상태를 새로 확인한다. 이 문서의 기준일과 과거 커밋 SHA만 믿고 현재 상태를 가정하지 않는다.

1. 저장소가 `namanvarava/Edan.Tensor`인지 확인한다.
2. `main-auto` 브랜치가 존재하는지 확인한다.
3. 현재 `main-auto` 최신 커밋과 작업 트리를 확인한다.
4. 아래 기준 파일을 모두 읽는다.
   - `docs/README.md`
   - `docs/registry/screens.csv`
   - `docs/registry/screen-functional-specifications.md`
   - `docs/registry/screen-functional-specifications.csv`
   - `docs/registry/screen-service-bindings.csv`
   - `docs/registry/screen-module-area-codes.csv`
   - `docs/standards/EDAN-SCREEN-REGISTRY-STANDARD.md`
   - `docs/standards/EDAN-SOURCE-DEVELOPMENT-STANDARD.md`
5. 실제 소스 트리와 프로젝트 파일을 조사한다. 이전 대화에서 언급된 파일 경로가 현재 브랜치에도 존재하는지 확인한다.
6. 미커밋 변경이 발견되면 덮어쓰거나 초기화하지 않는다. 변경 내용을 보존하고 충돌 위험을 먼저 보고한다.
7. `main-auto`가 없거나 접근할 수 없으면 임의로 다른 브랜치에 기록하지 말고 중단 사유를 보고한다.

### 브랜치 보호 규칙

- 모든 변경은 `main-auto`에만 수행한다.
- `main`에 커밋, 푸시, 병합하지 않는다.
- PR 생성이나 병합은 별도 명시적 승인이 있기 전까지 하지 않는다.
- 기존 커밋을 reset, rebase, force-push로 변경하지 않는다.
- 기존 작업을 삭제하거나 무관한 소스 리팩터링을 하지 않는다.

## 3. 현재 기준 Registry — 조사 대상 15개

| 순서 | ScreenId | 화면명 | 모듈 |
|---:|---|---|---|
| 1 | `PLT-TNT-001` | 테넌트 관리 | Platform |
| 2 | `PLT-ORG-001` | 조직 관리 | Platform |
| 3 | `PLT-PJT-001` | 프로젝트 관리 | Platform |
| 4 | `PLT-USR-001` | 사용자 초대 관리 | Platform |
| 5 | `PLT-MBR-001` | 멤버십 관리 | Platform |
| 6 | `PLT-AUTH-001` | 권한 정책 관리 | Platform |
| 7 | `PLT-AUD-001` | 권한 감사 관리 | Platform |
| 8 | `PLT-QTA-001` | 저장 용량 할당량 관리 | Platform |
| 9 | `DSET-DSM-001` | 데이터셋 관리 | DataSet |
| 10 | `DSET-SCHM-001` | 데이터셋 스키마 관리 | DataSet |
| 11 | `DSET-STO-001` | 데이터셋 콘텐츠·저장소 관리 | DataSet |
| 12 | `DSET-VLD-001` | 데이터셋 검증 관리 | DataSet |
| 13 | `WRK-TSK-001` | 작업 모니터링 | Worker |
| 14 | `WRK-DLQ-001` | 실패 작업 관리 | Worker |
| 15 | `WRK-XDL-001` | 외부 전달 모니터링 | Worker |

화면 ID, MODULE, AREA, 화면명은 기존 Registry를 기준으로 한다. 조사 편의를 이유로 임의 변경하거나 신규 ID를 발급하지 않는다.

## 4. 화면별 조사 범위

각 화면에 대해 아래 항목을 모두 확인한다. 해당하지 않는 항목은 근거를 확인한 뒤 `NotApplicable`로 처리한다.

### 4.1 API Endpoint
- Endpoint 등록 파일과 실제 경로
- HTTP Method
- Route 템플릿
- Endpoint 이름 또는 Map 메서드
- Query·Route·Header 파라미터
- 인증 요구 여부
- Endpoint Filter 및 인가 정책
- 응답 코드와 오류 응답
- 페이지네이션·정렬·검색 필터 지원 여부
- 취소·재시도·멱등성·동시성 처리 여부(해당 기능이 있을 때)

### 4.2 DTO 및 계약
- 실제 Request 타입과 전체 필드
- 필드 타입, nullable 여부, 기본값
- 필수·선택 여부 및 검증 속성
- 실제 Response 타입과 전체 필드
- 반환 컬렉션·페이징 구조
- 직렬화 이름 또는 JSON 계약에 영향을 주는 설정
- DTO가 없는 경우 Endpoint가 사용하는 구체 입력 타입과 응답 타입
- 문서의 입력·출력 항목이 실제 계약에 없는 경우 차이 기록

### 4.3 Application Service 및 DI
- Service 계약 파일 경로
- 구현체 파일 경로
- Endpoint에서 호출하는 메서드
- 서비스가 실제 수행하는 동작
- DI 등록 프로젝트·확장 메서드·등록 코드 위치
- Lifetime
- Decorator·Factory·조건부 등록이 있으면 실제 구성 확인
- API Endpoint가 직접 처리하는 경우 Service가 있다고 추정하지 않는다.

### 4.4 인증·인가 및 데이터 경계
- 인증 정책·Authorization Filter·Endpoint Filter
- 사용자/역할/정책 검사
- Tenant·Organization·Project 범위 검증
- 소유권 또는 리소스 접근 검사
- 서버 측 최종 인가 여부
- 감사 이벤트 발생 여부
- UI에서 버튼을 숨기는 것과 서버의 실제 인가를 혼동하지 않는다.
- 기존 역할명이나 제안 역할을 구현된 역할로 승격하지 않는다.

### 4.5 테스트 근거
- 관련 Unit Test 경로와 테스트명
- API/Integration Test 경로와 테스트명
- 성공·실패·권한 거부·경계 조건 테스트
- 실제 테스트 실행 여부와 결과
- 실행하지 않은 테스트는 통과로 표시하지 않는다.
- 테스트가 없으면 누락 사실을 기록한다.

## 5. 15개 화면별 중점 검토 질문

아래는 조사 우선순위를 위한 질문이다. 구현 여부를 미리 단정하는 목록이 아니다.

### Platform
- `PLT-TNT-001`: 전체 테넌트 목록·상세·검색과 내 테넌트 조회가 구분되는가? 활성화 정책과 권한 범위는 무엇인가?
- `PLT-ORG-001`: 조직 생성 외 목록·상세·수정·상태 API가 있는가? DTO와 테넌트 소속 검증은 무엇인가?
- `PLT-PJT-001`: 프로젝트 생성 외 목록·상세·수정·상태 API가 있는가? 조직-프로젝트 관계와 범위 인가는 어떻게 확인하는가?
- `PLT-USR-001`: 초대 생성 외 목록·상세·수락·만료·재전송·취소 API가 있는가? 토큰 및 개인정보 노출 경계는 무엇인가?
- `PLT-MBR-001`: 테넌트·조직·프로젝트별 멤버십 부여·조회·역할 변경·회수 API가 각각 존재하는가? 실제 권한은 무엇인가?
- `PLT-AUTH-001`: 정책 CRUD·활성 상태 변경·평가 API가 각각 있는가? 우선순위·충돌·범위 검증은 무엇인가?
- `PLT-AUD-001`: 감사 조회·무결성·아카이브·예약·보존·작업 API를 구분하고 각각의 권한·DTO·상태를 확인한다.
- `PLT-QTA-001`: 정책 조회·생성·수정·상태 변경·이력과 실제 사용량 조회가 구분되는가? 동시성 버전 및 범위 검증은 무엇인가?

### DataSet
- `DSET-DSM-001`: 데이터셋 생성·상세·목록·검색·활성화·보관 기능의 실제 범위와 테넌트 경계는 무엇인가?
- `DSET-SCHM-001`: 스키마 등록·개정·현재 버전·개정 이력 조회가 어떤 Endpoint와 DTO로 제공되는가?
- `DSET-STO-001`: 콘텐츠 업로드·조회·다운로드·삭제·버전 및 저장소 사용량 API를 구분한다. 대용량 처리 방식도 확인한다.
- `DSET-VLD-001`: 프로파일 생성·개정·조회, 검증 실행, 실행 목록·상세·결과·삭제 기능의 실제 범위와 권한을 확인한다.

### Worker
- `WRK-TSK-001`: 작업 등록·목록·상세·상태·진행률·취소·재시도·결과 API를 구분한다.
- `WRK-DLQ-001`: DLQ 목록·상세·재처리 API, 재처리 조건 및 권한을 확인한다.
- `WRK-XDL-001`: 외부 전달 생성·목록·상세·상태·재시도 API, 외부 Provider 계약과 비밀정보 노출 경계를 확인한다.

## 6. 권장 산출물

다음 문서를 `docs/registry/`에 작성한다.

### 6.1 `screen-api-mapping.csv`
화면과 실제 API Endpoint 사이의 관계를 기록한다. 한 Endpoint가 여러 화면에서 사용되면 화면별로 행을 나눈다.

권장 헤더:
`ScreenId,ApiId,HttpMethod,Route,EndpointName,EndpointPath,RequestType,RequestDtoPath,ResponseType,ResponseDtoPath,ServiceContract,ServiceImplementation,RegistrationProject,RegistrationMethod,Lifetime,AuthPolicy,AuthorizationEvidence,TestEvidence,ImplementationStatus,Remarks`

- `ApiId`는 문서 내에서 유일하고 안정적인 식별자로 부여한다. 예: `API-PLT-TNT-001-001`
- 같은 ScreenId에 Endpoint가 여러 개면 여러 행을 사용한다.
- 같은 Endpoint를 여러 화면에서 쓰면 ScreenId별 관계 행을 각각 기록한다.
- 계약·구현체가 없는 경우 추정하지 않고 빈 값으로 둔다.
- 파일 경로는 저장소 루트 기준 상대 경로로 기록한다.
- 행 단위는 ScreenId와 ApiId의 관계다.

### 6.2 `screen-api-validation.csv`
화면별 기능 또는 검증 항목의 결과를 기록한다. 한 화면에 여러 검증 항목이 있으면 여러 행을 사용한다.

권장 헤더:
`ScreenId,ValidationId,Feature,ValidationCategory,ExpectedBehavior,ActualBehavior,Status,EvidencePaths,EvidenceSymbols,TestStatus,GapId,Remarks`

- `ValidationId`는 파일 내 고유 식별자다.
- 기능 단위로 검증한다. 예: 생성, 목록, 상세, 수정, 삭제, 검색, 페이징, 권한 거부.
- 근거 경로는 실제 확인한 소스만 기록한다.
- 기능 명세가 잘못되었거나 근거 없는 요구사항은 실제 동작과 차이를 적는다.

### 6.3 `screen-implementation-gaps.md`
미구현·부분 구현·계약 부족·권한 부족·테스트 부족 항목을 정리한다.
- 요약 및 전체 판정
- 화면별 격차
- 문제 설명과 근거
- 사용자 영향
- 보완 제안
- 선행 조건 및 의존성
- 우선순위
- 후속 작업 단위
- 완료 검증 기준

### 6.4 기존 문서 갱신
- `screen-functional-specifications.md`: 실제 확인 결과와 설계 제안의 구분을 보강한다.
- `screen-functional-specifications.csv`: MD와 동일한 결과로 동기화한다.
- `screens.csv`: 기존 화면 ID와 상세 명세 링크를 유지한다. API 매핑 링크를 추가하는 경우 기존 열 구조를 무단 변경하지 말고 표준·README도 함께 갱신한다.
- `docs/README.md`: 신규 문서 링크를 추가한다.
- `EDAN-SCREEN-REGISTRY-STANDARD.md`: 새 API 매핑·검증 파일의 책임과 갱신 규칙이 필요할 때만 보완한다.
- `screen-service-bindings.csv`: 실제 ViewModel-Service 연결이 아직 없다면 빈 헤더 상태를 유지한다. API의 Application Service를 화면 ViewModel 연결로 잘못 등록하지 않는다.

## 7. 상태 값 및 판정 기준

기능 검증 상태:
- `Implemented`: 관련 구현이 실제 존재하고 코드 근거를 확인함
- `PartiallyImplemented`: 일부 동작만 구현되었거나 필수 흐름 일부가 누락됨
- `NotImplemented`: 요구된 동작의 구현이 확인되지 않음
- `NotVerified`: 현재 조사 근거만으로 판정할 수 없음
- `NotApplicable`: 해당 기능이 화면 범위에 적용되지 않으며 그 사유가 명확함

테스트 상태는 별도로 관리한다.
- `Passed`: 실제 실행 결과 통과
- `Failed`: 실제 실행 결과 실패
- `NotRun`: 실행하지 않음
- `NotFound`: 관련 테스트를 찾지 못함
- `NotApplicable`: 해당 없음

중요:
- API가 존재한다는 사실만으로 화면 기능을 `Implemented`로 판정하지 않는다.
- 구현 소스가 있다는 사실만으로 테스트를 `Passed`로 판정하지 않는다.
- 검색 UI 조건이 실제 API Query 계약에 없으면 API 지원 검색으로 표시하지 않는다.
- DTO에 없는 필드를 입력·출력 계약에 있는 것처럼 기록하지 않는다.
- 역할 이름이 코드에 등장하는 것과 실제 Endpoint에 인가가 적용되는 것을 구분한다.
- 미확인과 미구현을 구분한다.

## 8. 조사 실행 순서

### Phase 0 — 기준선 확보
- 브랜치·커밋·작업 트리 확인
- 기준 문서와 Registry 읽기
- 전체 소스 트리 및 프로젝트 구조 확인
- 조사 대상 15개와 기존 문서 행 수 검증

### Phase 1 — Endpoint 목록 전수 조사
- 모든 Host의 Endpoint 등록 위치 조사
- Route·HTTP Method·핸들러를 추출
- 화면별 API 후보를 연결
- 어느 화면에도 연결되지 않는 관련 Endpoint도 별도 메모
- Endpoint를 찾지 못한 경우 검색 범위와 근거를 기록

### Phase 2 — DTO·서비스 계약 조사
- Endpoint에서 실제 참조하는 Request·Response 타입 추적
- 타입 선언과 검증 규칙 확인
- Application Service 호출 흐름 추적
- 인터페이스·구현체·DI 등록·Lifetime 확인
- 외부 Provider 또는 저장소 경계 추적

### Phase 3 — 권한 및 데이터 경계 조사
- Endpoint별 인증·인가 구성 확인
- 실제 정책·역할·클레임·범위 검사 확인
- Tenant/Organization/Project 리소스 경계 확인
- 감사 이벤트와 오류 처리 확인

### Phase 4 — 테스트 조사 및 실행
- 관련 테스트 프로젝트·테스트 코드 탐색
- 테스트 케이스와 기능 연결
- 가능한 범위에서 대상 테스트 실행
- 실행 명령·환경·결과·실패 원인을 기록
- 전체 솔루션 테스트가 필요하면 현재 SDK·환경·비용을 고려하고 실행 전 명령을 보고

### Phase 5 — 기능 명세 대조
- 기존 MD·CSV와 실제 코드 대조
- 잘못된 API 경로·DTO 필드·권한·검색 조건 보정
- 확인된 사실, 설계 권장, 미확인 항목 구분
- 미구현 격차와 선행 조건 작성

### Phase 6 — 문서·자동 검증
- 신규 CSV 작성
- MD·CSV 간 ScreenId 집합 일치 확인
- 15개 화면 누락·중복 검사
- ApiId·ValidationId 중복 검사
- 참조 ScreenId 유효성 검사
- 파일 경로가 실제 저장소에 존재하는지 확인
- CSV 인용·줄바꿈·인코딩 확인
- README 링크와 Registry 링크 확인
- 변경 diff 검토

### Phase 7 — 최종 보고 및 커밋
- 모든 산출물과 검증 결과를 보고
- 미해결·미확인 항목을 숨기지 않음
- 사용자 승인이 별도로 필요하지 않은 문서화 범위는 `main-auto`에 커밋
- 소스 기능 구현·API 계약 변경은 이번 조사 범위에 포함하지 않음
- 커밋 SHA와 변경 파일 목록 제공

## 9. 우선순위 기준

- P0: 보안·테넌트 격리·권한 우회·데이터 손상 위험
- P1: 화면 핵심 업무 기능의 API 또는 계약 누락
- P2: 검색·페이징·상태 표시·오류 처리 등 사용성 및 운영 기능 누락
- P3: 보조 정보·편의 기능·문서 정리

우선순위는 실제 조사 근거와 영향으로 판정한다. 근거 없이 P0로 올리지 않는다.

## 10. 품질 게이트 — 완료로 판단하기 위한 조건

다음 조건을 모두 만족해야 1차 정합성 조사를 완료했다고 보고한다.

1. Screen Registry의 15개 ScreenId 모두 조사 결과가 존재한다.
2. 각 화면의 관련 Endpoint 탐색 결과가 기록되어 있다.
3. 확인된 모든 Endpoint에 HTTP Method·Route·실제 파일 경로가 있다.
4. 확인 가능한 Request·Response DTO와 필드가 실제 타입 선언과 일치한다.
5. Service·DI·권한 근거는 실제 코드에서 확인한 것만 기록되어 있다.
6. 테스트 상태는 실제 탐색·실행 결과와 일치한다.
7. 모든 기능이 구현·부분 구현·미구현·미확인·해당 없음 중 하나로 판정되어 있다.
8. 기존 MD·CSV의 과장 또는 잘못된 기술 내용이 보정되어 있다.
9. 신규 CSV의 ScreenId·ApiId·ValidationId 중복 및 누락이 0건이다.
10. 참조 무결성·CSV 형식·문서 링크 검증이 통과한다.
11. 미구현 격차와 후속 권장 작업이 우선순위와 근거를 갖는다.
12. 작업 결과·실행하지 않은 검증·제약 사항·커밋 SHA가 보고되어 있다.

조사 자체로 UI 개발 완료를 선언하지 않는다. UI 구현 단계 진입 가능 여부만 별도로 판정한다.

## 11. 후속 단계 진입 조건

1차 조사가 완료되면 바로 UI 코드를 무조건 생성하지 않는다. 다음 순서로 이어간다.

1. P0 보안·테넌트 격리 문제 여부 판단
2. 화면 핵심 기능에 필요한 API 계약 부족 여부 판단
3. 필요한 API 보완 작업과 UI 작업의 선후 관계 결정
4. WPF·MVVM·Navigation·공통 UI·API Client·DI 아키텍처 설계
5. 공통 UI 기반 구현 및 테스트
6. Platform 8개 → DataSet 4개 → Worker 3개 화면 순으로 구현하되 실제 의존성과 조사 결과에 따라 조정
7. Screen Registry의 View·ViewModel·Service 연결 정보는 구현과 검증이 끝난 항목만 기록

## 12. 작업 중 금지 사항

- 없는 Endpoint·DTO·Service·권한 정책을 만들어내지 않는다.
- 소스 조사만 하기로 한 단계에서 UI나 API를 임의 구현하지 않는다.
- 실제 API가 지원하지 않는 검색·수정·삭제를 명세에 지원한다고 기재하지 않는다.
- ViewModel-Service 관계가 확인되지 않았는데 `screen-service-bindings.csv`를 채우지 않는다.
- Registry ID·MODULE·AREA 코드를 임의 변경하지 않는다.
- 기존 사용자 변경사항을 덮어쓰지 않는다.
- 비밀정보·토큰·개인정보를 문서나 로그에 복사하지 않는다.
- 검증하지 않은 내용을 완료·통과로 표시하지 않는다.
- `main`에 직접 반영하거나 병합하지 않는다.

## 13. 최종 보고 형식

작업 종료 시 다음 순서로 한국어 보고서를 작성한다.

1. 작업 요약 및 기준 커밋
2. 15개 화면별 판정 표
3. API Endpoint 수 및 화면별 매핑 결과
4. Request·Response DTO 조사 결과
5. Service·DI 조사 결과
6. 인증·인가·테넌트 경계 결과
7. 테스트 탐색·실행 결과
8. P0~P3 미구현 격차
9. 생성·수정한 파일 목록
10. CSV·링크·참조 무결성 검사 결과
11. 실행하지 못한 검사와 이유
12. 남은 위험 및 UI 단계 진입 가능 여부
13. 커밋 SHA 및 GitHub 링크
14. 다음 단계에서 실행할 구체적인 작업 지시

보고서의 숫자는 실제 결과에서 집계한다. 예상 숫자나 임의의 완료율을 쓰지 않는다.

---

## 14. 다른 ChatGPT 계정의 Work 실행 명령

새 ChatGPT 계정에서 **Work 모드**를 열고 EDAN.Tensor GitHub 저장소에 접근할 수 있는 연결·권한을 준비한다. 아래 명령 전체를 Work에 전달한다.

> 저장소 `namanvarava/Edan.Tensor`의 `main-auto` 브랜치에서 작업하라. 먼저 `docs/operations/EDAN-TENSOR-SCREEN-API-AUDIT-HANDOFF.md`를 처음부터 끝까지 읽고 모든 지시를 준수하라. 현재 브랜치·최신 커밋·작업 트리와 기준 문서를 다시 확인한 다음, Screen Registry에 등록된 15개 화면의 실제 API Endpoint·HTTP Method·Route·Request/Response DTO·Application Service·DI 등록·인증/인가·테스트를 소스 코드 근거로 전수 조사하라. 권장 산출물 `screen-api-mapping.csv`, `screen-api-validation.csv`, `screen-implementation-gaps.md`를 작성하고 기존 화면 기능 명세 MD·CSV와 README 및 필요한 표준 문서를 정합성 있게 갱신하라. 모든 경로·필드·정책·테스트 결과는 실제 저장소에서 확인하고, 확인하지 못한 것은 NotVerified로 남겨라. 이번 단계에서는 UI 구현이나 API 계약 변경을 하지 말라. 기존 사용자 변경사항을 덮어쓰지 말고, 모든 변경은 `main-auto`에만 반영하라. `main` 직접 변경·병합·PR 생성은 하지 말라. 15개 화면 전체를 검증하고 CSV·참조·링크 검사를 수행한 뒤, 결과와 미해결 항목을 한국어로 보고하고 커밋 SHA를 제시하라. 중간에 컨텍스트가 부족해지면 추측하지 말고 저장소 문서와 커밋을 기준으로 이어가라.

진행은 조사부터 산출물 검증·커밋까지 책임지고 수행하되, API 구현이나 UI 구현으로 범위를 확대하지 않는다. 외부 권한 승인이나 연결이 필요한 경우 해당 단계에서 멈추고 필요한 사용자 조치를 구체적으로 안내한다.

## 15. 인계 기준 문서

- [Screen Registry](registry/screens.csv)
- [화면별 상세 기능 명세](registry/screen-functional-specifications.md)
- [화면별 상세 기능 명세 CSV](registry/screen-functional-specifications.csv)
- [화면-Service 연결 목록](registry/screen-service-bindings.csv)
- [MODULE·AREA 코드 사전](registry/screen-module-area-codes.csv)
- [Screen Registry 관리 표준](../standards/EDAN-SCREEN-REGISTRY-STANDARD.md)
- [소스 개발 표준](../standards/EDAN-SOURCE-DEVELOPMENT-STANDARD.md)

이 문서는 다른 계정이 작업을 재개하기 위한 실행 지시서다. 실제 소스 상태와 최신 브랜치 상태가 본 문서의 과거 기준과 다르면 최신 저장소를 우선한다.

## 16. 2026-11-16 후속 구현 진행 기록

사용자 승인에 따라 조사 후 남아 있던 확정 필요 항목을 `main-auto`에서 실제 구현 범위로 전환했다.

- `PLT-USR-001`: `Invitation` aggregate, token hash 저장, Pending/Accepted/Expired/Revoked 전이, resend/revoke/token-only accept API, Delivery Adapter 및 typed Client 연결
- `WRK-TSK-001`: `IWorkerResultStore`, 개발용 파일 저장 Adapter, 성공 작업 raw result 저장·metadata·streaming API, Tenant/Task 경계 및 64 MiB 상한
- HTTP smoke: Invitation lifecycle와 Worker raw result bytes의 실제 요청 테스트를 `EDAN.Tensor.ApiHost.IntegrationTests`에 추가
- WPF smoke: STA 스레드에서 `App`·`MainWindow`·15개 Registry 화면 DataTemplate 로딩을 검증하는 `EDAN.Tensor.Desktop.Tests` 추가
- CI: 전체 테스트 외에 HTTP smoke와 WPF smoke 단계를 명시적으로 추가

API Host의 임시 SQLite migration 기동과 Release build를 확인했다. GitHub Actions run `43736265722`에서 전체 Build/Test, HTTP smoke 2건, WPF UI smoke 1건이 통과했다. 로컬 VSTest testhost는 연결되지 않았지만 원격 검증 결과를 CSV에 반영했으며, action-level UI 조작 행은 smoke 범위 밖이므로 `NotRun`을 유지한다. 상세 계약은 [invitation-and-worker-result-contract.md](../architecture/invitation-and-worker-result-contract.md)에 기록한다.
