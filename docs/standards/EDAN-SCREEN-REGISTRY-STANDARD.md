# EDAN Tensor Screen Registry 관리 표준

- 문서 상태: 확정 표준
- 표준 버전: 1.0
- 적용 제품: EDAN Tensor
- 언어: 문서·화면명은 한국어 우선, 코드 식별자는 영어
- 상위 표준: [EDAN 소스 개발 표준](EDAN-SOURCE-DEVELOPMENT-STANDARD.md)

## 1. 목적

Screen Registry는 EDAN Tensor의 모든 사용자 화면을 식별하고, 화면과 구현 소스 및 의존 서비스 사이의 연결 관계를 추적하는 공식 관리 목록이다.

관리 대상은 화면 ID, 화면명, View, ViewModel, Service 연결, 업무 목적, 기능, 소유 모듈 및 생명주기 상태다. Registry는 화면 구현을 대체하지 않으며 실제 소스와 연결 관계를 비교·검증하기 위한 기준이다.

## 2. 관리 원칙

### 2.1 단일 기준 정보

- 화면 ID와 공식 화면명은 Screen Registry를 기준으로 관리한다.
- 화면 ID는 XAML, C# 파일 헤더, 테스트 및 기술 문서에서 동일하게 사용한다.
- 화면명이나 파일 경로가 변경되어도 화면 ID는 유지한다.
- 화면 ID를 파일명이나 클래스명에서 추론하여 새로 만들지 않는다.
- Registry에 없는 화면 ID를 소스에 임의로 추가하지 않는다.
- 실제 구현과 Registry가 다르면 양쪽의 정합성을 확인하고 함께 정정한다.

### 2.2 화면과 구현 요소의 관계

화면 하나는 하나의 대표 View와 하나의 대표 ViewModel에 연결한다.

| 구성 요소 | 책임 |
| --- | --- |
| View | 화면 렌더링 및 사용자 입력 전달 |
| ViewModel | 화면 상태, 표시 데이터, 명령 및 화면 동작 |
| Service 계약 | ViewModel에 업무 기능 또는 데이터 접근 기능 제공 |
| Service 구현체 | DI 구성에 따라 계약에 연결되는 실제 구현 |

기본 의존 방향은 View → ViewModel → Service 계약 → DI 등록 구현체다. View에서 Service 구현체나 Repository를 직접 호출하지 않는다. 프레임워크가 요구하는 예외는 사유와 범위를 기록한다.

### 2.3 책임 분리

| 정보 | 공식 관리 위치 |
| --- | --- |
| 화면 ID·화면명·업무 목적·상태 | docs/registry/screens.csv |
| View·ViewModel 형식 및 파일 경로 | docs/registry/screens.csv |
| 화면과 Service 계약·구현체 연결 | docs/registry/screen-service-bindings.csv |
| 화면 ID 코드 구성 및 발급 규칙 | 본 표준 |
| 파일별 상세 책임 및 설명 | 각 소스 파일 상단 개요 |
| 실제 클래스 동작과 계약 | C# XML 문서 및 코드 |
| DI 등록의 실제 구성 | 해당 모듈의 DependencyInjection 소스 |

같은 정보를 여러 Registry 파일에 중복 저장하지 않는다. Service 연결은 별도 관계 파일에서 관리한다.

## 3. 화면 ID 표준

### 3.1 형식 — 사용자 확정

화면 ID는 모듈 코드 + 업무 영역 코드 + 일련번호로 구성한다.

    [MODULE]-[AREA]-[SEQUENCE]

| 화면 ID | 모듈 | 업무 영역 | 의미 |
| --- | --- | --- | --- |
| PLT-TNT-001 | Platform | Tenant | 테넌트 영역 첫 번째 화면 |
| DSET-DSM-001 | DataSet | DataSet Management | 데이터셋 관리 영역 첫 번째 화면 |
| WRK-TSK-001 | Worker | Worker Task | 작업 관리 영역 첫 번째 화면 |
| COML-PAY-001 | Commercial | Payment | 결제 영역 첫 번째 화면 |
| LIC-LCM-001 | Licensing | License Lifecycle | 라이선스 수명주기 영역 첫 번째 화면 |

위 예시는 코드 Registry에 등록된 MODULE·AREA 조합을 사용하는 형식 예시다. 화면 구현이 확인되기 전까지 실제 화면이 등록된 것으로 간주하지 않는다.

### 3.2 구성 규칙

| 요소 | 규칙 |
| --- | --- |
| MODULE | 모듈 코드 사전에 등록된 대문자 영문 코드 |
| AREA | 업무 영역 코드 사전에 등록된 대문자 영문 코드 |
| SEQUENCE | 3자리 숫자, 001부터 순차 발급 |

- 구분자는 하이픈만 사용한다.
- 대문자 영문과 숫자 외 문자를 사용하지 않는다.
- 일련번호는 동일한 MODULE-AREA 조합 안에서 증가한다.
- 화면 ID 전체는 제품 범위에서 유일해야 한다.
- 코드 길이 또는 일련번호 자릿수 변경은 기존 ID를 바꾸지 않고 표준 개정으로 처리한다.

### 3.3 코드 발급·변경

1. 신규 화면 등록 시 모듈과 업무 영역을 먼저 확정한다.
2. 코드 사전에 해당 MODULE과 AREA가 있는지 확인한다.
3. 같은 MODULE-AREA 조합의 마지막 일련번호 다음 값을 발급한다.
4. 발급한 ID를 Registry에 먼저 등록한 후 소스 파일과 테스트에 반영한다.
5. 발급된 ID는 화면 폐기·통합·이름 변경 후에도 재사용하지 않는다.
6. 화면 통합 시 대표 화면 ID를 유지하고 흡수된 화면 ID는 Retired로 전환한다.
7. 화면 분리 시 신규 화면에 새 ID를 발급한다.
8. 화면 소유 모듈이 바뀌어도 기존 ID는 유지한다. ID의 모듈 코드를 변경하지 않는다.

### 3.4 코드 사전 관리

- MODULE 코드는 실제 업무 모듈의 소유 경계를 나타낸다.
- AREA 코드는 해당 모듈이 소유하는 화면 업무 영역을 나타낸다.
- MODULE 코드는 제품 전체에서 유일해야 한다.
- AREA 코드는 모듈 간에도 제품 전체에서 유일해야 하며 MODULE 코드와 중복될 수 없다.
- 코드는 대문자 영문 2~4자로 구성하고 하이픈을 포함하지 않는다.
- 코드 폐기 후 다른 의미로 재사용하지 않는다.
- 프로젝트 디렉터리명, DB 약어 또는 기술 계층명을 검토 없이 화면 코드로 전용하지 않는다.
- 코드의 정식 명칭·소유 모듈·상태·근거는 [Screen Module·Area Code Registry](../registry/screen-module-area-codes.csv)를 단일 기준으로 관리한다.
- Registry에 없는 코드는 화면 ID에 사용할 수 없다.

### 3.5 실제 저장소 대조 결과

현재 소스 모듈은 Platform(PLT), DataSet(DSET), Worker(WRK), Commercial(COML), Licensing(LIC)으로 구분한다.

- Commercial은 전역 업무 모듈 분류의 COM(Common Master)과 충돌하지 않도록 COML을 사용한다.
- COM은 공통 마스터 업무 모듈의 미래 사용을 위해 예약하며 현재 구현 모듈로 간주하지 않는다.
- Client, Hosts, SharedKernel은 API Client·실행 진입점·공통 기술 기반이므로 독립적인 업무 화면 소유 MODULE로 등록하지 않는다.
- 실제 구현 소스가 없는 ERP·MES·MOM·QMS·PLM·EAM·IIoT·TAG 등 확장 업무 모듈은 현재 MODULE 코드로 발급하지 않는다.
- 업무 용어 사전의 적용 모듈 표기와 실제 소스 프로젝트 경계가 다르면 실제 소스 소유권을 우선하고 차이는 Registry 비고에 남긴다.
- AREA 코드가 전역 업무 용어 사전의 대표 약어와 같은 개념을 나타내면 해당 대표 약어를 우선 사용한다. 다른 개념과 약어가 충돌하면 의미가 다른 코드를 새로 선정한다. 예를 들어 Dispatch는 전역 약어 `disp`와 일치하는 `DISP`를 사용하고, Display에 이미 사용 중인 `DSP`는 사용하지 않는다.
- MODULE 코드와 AREA 코드는 Screen Registry의 코드 공간에서 서로 중복될 수 없다. 전역 업무 용어 약어와의 비교는 의미 충돌 방지를 위한 검토이며, 모듈 분류 코드와 전역 용어 약어를 같은 식별자 체계로 취급하지 않는다.

AREA 코드는 실제 소스 책임과 전역 업무 용어를 대조해 등록한다. 현재 등록된 코드는 다음 영역을 포함한다.

| MODULE | AREA 코드 | 관리 영역 |
| --- | --- | --- |
| PLT | TNT, ORG, PJT, USR, MBR, AUTH, AUD, QTA | 테넌트·조직·프로젝트·사용자·소속·권한·감사·할당량 |
| DSET | DSM, SCHM, STO, VLD | 데이터셋 관리·스키마·저장소·검증 |
| WRK | TSK, DISP, WLS, DLQ, XDL | 작업·디스패치·Lease·실패 격리·외부 전달 |
| COML | PAY | 결제 및 결제 공급자 경계 |
| LIC | LCM, KEY | 라이선스 수명주기·서명키 |

이 목록의 상세 영문명, 구현 근거 및 코드 상태는 CSV Registry를 기준으로 한다. 화면 ID 발급 전에 해당 조합이 승인 상태인지 확인한다.

## 4. 화면명 표준

### 4.1 화면명 필드

| 필드 | 규칙 | 예시 |
| --- | --- | --- |
| ScreenNameKo | 사용자에게 표시되는 한국어 이름 | 테넌트 목록 |
| ScreenNameEn | 코드·기술 문서에서 사용하는 영문 이름 | Tenant List |
| ViewType | View 클래스명 | TenantListView |
| ViewModelType | ViewModel 클래스명 | TenantListViewModel |

### 4.2 명명 원칙

- 한국어 화면명은 사용자가 수행하는 업무와 화면 목적을 명확하게 나타낸다.
- 영문 화면명은 PascalCase 클래스명으로 변환하기 쉬운 단어를 사용한다.
- 화면 유형이 명확하면 목록, 상세, 등록, 수정, 설정 등의 표현을 구분한다.
- 관리, 정보, 화면, 기타만으로 화면 목적을 표현하지 않는다.
- 동일한 업무 목적의 화면은 동일한 용어를 사용한다.
- 화면명 변경은 사용자 표시 명칭의 변경이며 화면 ID 변경을 의미하지 않는다.

## 5. View 연결 정보

### 5.1 관리 항목

| 항목 | 설명 |
| --- | --- |
| ViewTechnology | 실제 UI 기술. 예: WPF |
| ViewType | View의 식별 가능한 형식명 |
| ViewPath | 저장소 루트 기준 상대 경로 |
| ViewCodeBehindPath | 코드 비하인드가 존재하는 경우 상대 경로 |

### 5.2 작성 규칙

- ViewPath는 저장소 루트 기준 경로를 사용한다.
- 경로 구분자는 슬래시로 통일하고 실제 저장소의 대소문자를 유지한다.
- WPF XAML View는 .xaml 파일 경로를 기록한다.
- .xaml.cs 코드 비하인드가 있으면 별도 필드에 기록한다.
- 코드 비하인드가 없으면 빈 값으로 둔다.
- 공통 UserControl은 대표 화면 View로 등록하지 않는다.
- 아직 파일이 없는 계획 화면은 ViewType과 ViewPath를 비워두고 Planned 상태로 관리한다.

## 6. ViewModel 연결 정보

### 6.1 관리 항목

| 항목 | 설명 |
| --- | --- |
| ViewModelType | ViewModel의 식별 가능한 형식명 |
| ViewModelPath | 저장소 루트 기준 상대 경로 |

### 6.2 작성 규칙

- View와 ViewModel은 기본적으로 1:1로 연결한다.
- ViewModel 이름은 화면의 업무 목적을 반영하고 ViewModel 접미사를 사용한다.
- ViewModel은 View에 직접 종속되지 않는 화면 상태 및 동작을 담당한다.
- 화면 전용 ViewModel이 없는 경우 공용 ViewModel을 임의로 연결하지 말고 설계 사유를 기록한다.
- 공용 ViewModel을 여러 화면에서 실제 공유하는 경우 각 화면에 연결을 명시하고 공유 사유를 기록한다.
- 파일 경로는 실제 소스 위치와 일치해야 한다.

## 7. Service 연결 정보

### 7.1 관리 원칙

화면별로 여러 Service 계약을 사용할 수 있으므로 별도 관계 파일인 docs/registry/screen-service-bindings.csv에서 관리한다. Service 연결은 View가 아닌 ViewModel 기준으로 기록한다.

기본 흐름은 View → ViewModel → IService 계약 → DI 구현체다.

### 7.2 Service 관계 필드

| 필드 | 필수 | 설명 |
| --- | --- | --- |
| ScreenId | 예 | 연결 대상 화면 ID |
| ViewModelType | 예 | Service를 사용하는 ViewModel 형식 |
| ServiceContract | 예 | 주입받는 인터페이스 또는 계약 형식 |
| ImplementationType | 조건부 | 실제 DI 구현 형식. 미확인 시 공란 |
| RegistrationProject | 조건부 | DI 등록을 소유한 프로젝트 |
| RegistrationMethod | 조건부 | DI 등록 확장 메서드 또는 등록 위치 |
| Lifetime | 조건부 | Singleton, Scoped, Transient 등 실제 수명 |
| UsagePurpose | 예 | 해당 화면에서 Service를 사용하는 목적 |
| Required | 예 | 화면 동작에 필수인지 여부 |
| Remarks | 선택 | 추가 설명 |

### 7.3 연결 규칙

- ViewModel은 가능한 한 구체 구현체가 아닌 Service 계약에 의존한다.
- ImplementationType은 DI 구성에서 확인한 경우에만 기록한다.
- 구현체가 환경별로 달라지면 하나를 대표 구현으로 임의 고정하지 않는다. 공통 계약을 기록하고 환경별 차이를 비고 또는 별도 바인딩 행으로 관리한다.
- 하나의 ViewModel이 여러 Service 계약을 사용하면 계약마다 한 행을 추가한다.
- 같은 Service를 여러 화면에서 사용하면 화면별 관계 행을 각각 작성한다.
- Repository, DbContext, Provider 등 내부 인프라 의존성을 Service로 잘못 분류하지 않는다.
- View가 Service를 직접 호출하는 예외 구조는 근거를 기록하고 별도 아키텍처 검토 대상으로 표시한다.

## 8. 화면 생명주기 상태

Status는 다음 값만 사용한다.

| 상태 | 의미 | View/ViewModel 경로 |
| --- | --- | --- |
| Planned | 승인된 화면 계획. 구현 전 | 비워둘 수 있음 |
| InDevelopment | 개발 진행 중 | 존재하는 항목만 기록 |
| Active | 사용 가능한 구현 완료 화면 | View·ViewModel 필수 |
| Deprecated | 대체 화면이 안내된 사용 중단 예정 화면 | 기존 연결 유지 |
| Retired | 사용 종료 및 신규 진입 차단 화면 | 이력 추적을 위해 ID와 기록 유지 |

- 신규 화면은 Planned 상태로 등록할 수 있다.
- 구현이 시작되면 InDevelopment로 변경한다.
- View와 ViewModel 구현·연결 검증이 끝나면 Active로 전환한다.
- 대체 화면이 있으면 Deprecated 상태와 대체 ScreenId를 기록한다.
- 실제 사용 종료 후 Retired로 변경한다.
- Retired 행은 삭제하지 않는다.
- Deprecated 및 Retired 화면 ID는 재사용하지 않는다.

## 9. Registry 파일 구조 및 데이터 형식

Registry 파일은 다음 위치에서 관리한다.

    docs/
      registry/
        screens.csv
        screen-service-bindings.csv
        screen-module-area-codes.csv
        screen-functional-specifications.md
        screen-functional-specifications.csv
        screen-api-mapping.csv
        screen-api-validation.csv
        screen-implementation-gaps.md

CSV는 UTF-8 인코딩, 첫 행 헤더, 쉼표 구분을 사용한다. Excel 호환성을 위해 RFC 4180 방식의 필드 인용 규칙을 따른다.

### 9.1 screens.csv

화면 하나당 한 행을 사용한다.

| 순서 | 필드 | 필수 | 설명 |
| --- | --- | --- | --- |
| 1 | ScreenId | 예 | 화면 고유 ID |
| 2 | ModuleCode | 예 | 모듈 코드 |
| 3 | AreaCode | 예 | 업무 영역 코드 |
| 4 | ScreenNameKo | 예 | 한국어 화면명 |
| 5 | ScreenNameEn | 예 | 영문 화면명 |
| 6 | Purpose | 예 | 화면 목적 |
| 7 | MainFeatures | 예 | 주요 기능. 세미콜론으로 구분 |
| 8 | LayoutAreas | 권장 | 화면 구성 영역. 세미콜론으로 구분 |
| 9 | ViewTechnology | 구현 시 | UI 기술 |
| 10 | ViewType | 구현 시 | View 형식명 |
| 11 | ViewPath | 구현 시 | View 파일 경로 |
| 12 | ViewCodeBehindPath | 조건부 | 코드 비하인드 경로 |
| 13 | ViewModelType | 구현 시 | ViewModel 형식명 |
| 14 | ViewModelPath | 구현 시 | ViewModel 파일 경로 |
| 15 | Status | 예 | 화면 상태 |
| 16 | ReplacementScreenId | 조건부 | 대체 화면 ID |
| 17 | IntroducedVersion | 권장 | 최초 포함 버전 |
| 18 | Owner | 권장 | 담당 모듈 또는 책임 팀 |
| 19 | Remarks | 선택 | 추가 설명 |

### 9.2 screen-service-bindings.csv

화면과 Service 계약 관계 하나당 한 행을 사용한다.

| 순서 | 필드 | 필수 | 설명 |
| --- | --- | --- | --- |
| 1 | ScreenId | 예 | 화면 ID |
| 2 | ViewModelType | 예 | 사용하는 ViewModel |
| 3 | ServiceContract | 예 | Service 계약 |
| 4 | ImplementationType | 조건부 | DI 구현체 |
| 5 | RegistrationProject | 조건부 | DI 등록 프로젝트 |
| 6 | RegistrationMethod | 조건부 | DI 등록 위치 |
| 7 | Lifetime | 조건부 | DI 수명 |
| 8 | UsagePurpose | 예 | 사용 목적 |
| 9 | Required | 예 | 필수 연결 여부 |
| 10 | Remarks | 선택 | 추가 설명 |

### 9.4 화면별 상세 기능 명세

화면별 업무 명세는 다음 전용 문서에서 관리한다.

| 파일 | 책임 |
| --- | --- |
| docs/registry/screen-functional-specifications.md | 화면별 목적·주요 기능·검색 조건·입력·출력·권한·API 근거·구현 격차의 상세 설명 |
| docs/registry/screen-functional-specifications.csv | 동일한 화면별 명세를 Excel 필터·정렬·검토용으로 제공 |

- 상세 명세는 ScreenId를 기준으로 screens.csv의 화면 행과 연결한다.
- screens.csv에는 화면 식별·상태·구현 연결 정보만 유지하고 상세 기능 명세 전체를 중복 기재하지 않는다.
- CSV와 MD는 같은 1개 화면당 1개 레코드 원칙을 적용한다.
- 검색 조건은 화면 설계 조건이며, API 지원 여부를 별도로 표시한다.
- 입력·출력 항목은 실제 Request/Response 계약과 대조하기 전까지 화면 설계 항목으로 취급한다.
- 사용자 권한은 현재 구현된 인증 정책과 설계 제안 역할을 구분한다.
- API 근거와 추가 구현 필요 사항을 명시하며, 미구현 기능을 구현 완료로 표시하지 않는다.
- 상세 명세의 기준 파일과 변경 이력은 MD를 기준으로 관리하고 CSV는 같은 변경에서 함께 갱신한다.

### 9.5 화면 API·DTO 조사 파일

API·DTO 정합성 조사를 수행한 경우 다음 파일을 같은 조사 단위로 갱신한다.

| 파일 | 책임 |
|---|---|
| docs/registry/screen-api-mapping.csv | ScreenId별 실제 Endpoint, HTTP method/route, Request/Response 계약, Application Service, 구현체, DI 등록, lifetime, 인증·인가, 테스트 증거 및 구현 상태 |
| docs/registry/screen-api-validation.csv | 화면 기능별 기대 동작과 실제 동작의 비교, validation category, 상태, evidence path/symbol, 테스트 상태, gap 연결 |
| docs/registry/screen-implementation-gaps.md | 15개 화면의 조사 요약, 미구현/미확인 구분, 우선순위, 테스트 결과 및 UI 단계 판단 |
| docs/registry/screen-service-bindings.csv | 실제 ViewModel이 사용하는 Service 계약만 기록. API Endpoint에서 사용하는 Application Service를 UI 바인딩으로 추정하지 않음 |

- API 조사 파일의 ScreenId는 `screens.csv`의 기존 15개 ID와 일치해야 한다.
- API 조사 파일은 실제 소스 경로와 심볼을 근거로 작성하며, 소스에 없는 DTO 필드·query·권한을 추정하지 않는다.
- `ImplementationStatus` 및 `Status`는 실제 구현과 부분 구현을 구분하고, 검증하지 않은 항목은 `NotVerified`로 남긴다.
- 테스트를 실행하지 않은 경우 `Passed`로 기록하지 않는다. 결과가 없는 항목은 `NotRun` 또는 `NotFound`로 기록한다.
- API 조사 문서가 추가되어도 `screens.csv`의 ScreenId, MODULE·AREA 코드, 화면 상태를 임의 변경하지 않는다.

### 9.3 빈 값과 다중 값

- 확인되지 않은 값은 추정하지 않고 빈 값으로 둔다.
- N/A, TBD, 하이픈을 실제 경로·형식명 대신 사용하지 않는다.
- MainFeatures와 LayoutAreas는 한 셀 안에서 세미콜론으로 구분한다.
- Service 계약은 한 셀에 나열하지 않는다. 계약마다 관계 파일에 별도 행을 추가한다.
- 실제 구현이 없는 계획 화면은 상태로 표현하고 파일 경로를 임의로 만들지 않는다.

## 10. 등록 및 변경 절차

### 10.1 신규 화면

1. 화면 목적과 소유 모듈을 결정한다.
2. 업무 영역 및 코드 사전의 등록 여부를 확인한다.
3. 고유 ScreenId를 발급한다.
4. screens.csv에 기본 정보와 Planned 상태를 등록한다.
5. View 및 ViewModel 구현 경로를 결정한다.
6. 구현이 시작되면 InDevelopment로 변경한다.
7. Service 계약을 확인하고 관계 파일에 등록한다.
8. 파일 상단 개요에 화면명과 ScreenId를 기록한다.
9. 실제 소스 경로·형식·DI 연결을 대조한다.
10. 검증이 끝나면 Active로 변경한다.

### 10.2 화면명·구현 경로 변경

- 화면명 변경 시 ScreenId는 유지한다.
- View 또는 ViewModel 파일 이동 시 Registry 경로를 같은 변경에 반영한다.
- 형식명 변경 시 ViewType 또는 ViewModelType과 관련 소스·테스트를 함께 수정한다.
- Service 계약 변경 시 관계 파일과 DI 구성 및 ViewModel 생성 경로를 함께 검증한다.
- 화면 목적이나 소유 업무가 변경되어도 ID를 임의 변경하지 않는다.

### 10.3 화면 통합·폐기

- 화면 통합 시 유지할 대표 ScreenId를 결정한다.
- 흡수되는 화면은 Retired로 전환하고 대체 ScreenId를 기록한다.
- 화면을 폐기해도 Registry 행을 삭제하지 않는다.
- View와 ViewModel 소스 제거는 사용처·라우팅·테스트·문서 확인 후 진행한다.

## 11. 검증 규칙

### 11.1 데이터 무결성

- ScreenId 중복 없음
- ScreenId 형식 준수
- ModuleCode 및 AreaCode가 코드 사전에 등록됨
- ScreenId 내부 코드와 ModuleCode·AreaCode 일치
- Status가 허용된 값 중 하나임
- ReplacementScreenId가 존재하며 자기 자신을 가리키지 않음
- Retired ID 재사용 없음

### 11.2 소스 연결 검증

- Active 화면의 ViewType과 ViewPath가 실제 소스에 존재
- Active 화면의 ViewModelType과 ViewModelPath가 실제 소스에 존재
- XAML의 DataContext 또는 DI 연결이 등록된 ViewModel과 일치
- Service 관계의 ScreenId와 ViewModelType이 화면 Registry와 일치
- ServiceContract가 실제 계약 형식과 일치
- ImplementationType과 Lifetime은 DI 등록 코드로 확인된 경우에만 기재
- View에서 Service 구현체를 직접 호출하는 위반 여부 확인

### 11.3 변경 영향 검증

- ScreenId 참조가 소스 파일 헤더와 일치
- 화면명 변경이 메뉴·탐색·접근성 이름·테스트에 반영
- ViewModel 변경이 DI 및 View 연결에 반영
- Service 변경이 DI 등록 및 관련 테스트에 반영
- Deprecated 화면의 대체 화면 연결이 유효

## 12. 소스 파일 헤더 연계

화면 관련 소스 파일은 상위 EDAN 소스 개발 표준의 파일 상단 개요 규칙을 따른다.

View 파일 예시:

    // =============================================================================
    // 파일명         : TenantListView.xaml.cs
    // 화면명         : 테넌트 목록
    // 화면 ID        : PLT-TNT-001
    // 분류           : Platform / Tenant
    // 목적           : 테넌트 정보의 조회 및 관리
    // 주요 기능      : 목록 조회, 검색, 등록, 수정
    // 구성 영역      : 검색 영역, 목록 영역, 상세 영역
    // 연결 ViewModel : TenantListViewModel
    // 연결 Service   : ITenantService
    // =============================================================================

ViewModel 파일에는 대상 화면과 화면 ID, 상태 관리 목적, 주요 명령, 연결 Service 계약을 기록한다. Service 파일 자체에는 특정 화면 ID를 넣지 않는다. 여러 화면에서 공유되는 Service를 특정 화면 소유로 오해하지 않도록 한다.

## 13. 역할 및 변경 통제

| 역할 | 책임 |
| --- | --- |
| 화면 설계 담당 | 화면 목적·명칭·영역 및 신규 ID 요청 |
| 모듈 담당 | 모듈·업무 영역 코드와 화면 소유권 확인 |
| 구현 담당 | View·ViewModel·Service 연결 정보 갱신 |
| 검토 담당 | 중복 ID, 경로, 형식, DI 관계 및 상태 검증 |

소규모 단독 개발에서는 동일 사용자가 모든 역할을 수행할 수 있다. 다만 등록·구현·검증 단계를 체크리스트로 구분한다.

Registry 변경은 관련 소스 변경과 동일한 작업 단위에서 커밋한다. Registry만 변경하거나 소스만 변경한 상태를 완료로 처리하지 않는다.

## 14. 현재 저장소 기준 적용 상태

2026-11-15 기준 main-auto 저장소의 전체 Git 트리(444개 경로)를 확인한 결과, .xaml, .xaml.cs, ViewModel 또는 Screen Registry로 식별되는 화면 구현 파일은 확인되지 않았다. src/Client에는 Client Contracts와 API Client가 존재한다. 업무 화면 소유 모듈은 Platform, DataSet, Worker, Commercial, Licensing으로 구분하며 코드 정의는 screen-module-area-codes.csv에서 관리한다.

2026-11-15 기준으로 Registry의 15개 화면은 `main-auto`의 `src/Client/EDAN.Tensor.Desktop`에 WPF View·ViewModel과 `IDesktopScreenService` binding이 실제로 등록되어 `InDevelopment`로 관리한다. `RegistryScreenView.xaml`은 공통 WPF View이고, 15개 화면별 View 클래스·ViewModel·MainWindow DataTemplate 매핑이 존재한다. API·DTO 조사 파일은 API 구현과 화면 구현을 구분하기 위해 계속 별도로 관리한다.

`InDevelopment`는 실제 View·ViewModel·typed Client 조회 경로가 존재한다는 뜻이다. 모든 mutation workflow와 UI smoke/test 증거가 확인되기 전에는 `Active`로 승격하지 않는다. API가 제공하지 않는 목록·필터·운영 제어를 추정하여 Registry에 기록하지 않는다.

## 15. 확정된 관리 기준 요약

| 항목 | 확정 기준 |
| --- | --- |
| ID 형식 | MODULE-AREA-### |
| ID 범위 | 제품 전체 고유 |
| ID 재사용 | 금지 |
| 화면명 | 한국어 공식명 + 영문 기술명 |
| View | 형식명·저장소 상대 경로 관리 |
| ViewModel | 형식명·저장소 상대 경로 관리 |
| Service | ViewModel이 사용하는 계약 기준으로 별도 관계 관리 |
| Service 구현체 | DI 구성에서 확인된 경우에만 기록 |
| 데이터 구조 | 화면 마스터 + Service 관계 CSV 분리 |
| 화면 상태 | Planned / InDevelopment / Active / Deprecated / Retired |
| 파일 관리 | 소스 파일 헤더와 Registry 동시 갱신 |
| 폐기 기록 | 삭제하지 않고 Retired 유지 |
| 검증 | ID·경로·형식·DI·소스 참조 일치 확인 |

## 16. 변경 이력

| 버전 | 내용 |
| --- | --- |
| 1.0 | 화면 ID, 화면명, View, ViewModel, Service 관계 및 Registry 운영 규칙 확정 |
| 1.1 | API·DTO 조사 파일의 책임, 실제 소스·테스트 증거, UI 바인딩과 API 서비스의 분리 규칙 추가 |
