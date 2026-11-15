# EDAN Tensor 소스 개발 표준

- 문서 상태: 검토 통합안
- 표준 버전: 0.1
- 작성 기준일: 2026-11-15
- 적용 제품: EDAN Tensor
- 적용 범위: src 및 tests의 C# 소스, UI·API 계약 코드
- 언어 정책: 소스 식별자는 영어, 개발 문서·XML 문서·주석은 한국어

> 이 문서는 사용자가 직접 확정한 규칙과 검토 과정에서 채택한 권장안을 구분한다. 권장안은 신규 코드의 기본 적용 기준으로 제시한 것이며, 사용자 확정 규칙과 동일한 승인 상태로 간주하지 않는다.

## 1. 목적과 적용 원칙

EDAN Tensor 소스 코드의 명명·문서화·파일 설명·코드 구조를 일관되게 관리한다.

1. 식별자는 짧고 정확하게 작성한다.
2. 식별자에 모든 설명을 담지 않고, 상세 의미와 제약은 XML 문서 및 주석으로 설명한다.
3. 모든 C# 파일에 파일 유형별 상단 개요를 작성한다.
4. 화면 ID와 화면 정보는 별도 목록으로 관리하고 관련 소스에서 동일한 ID를 참조한다.
5. 기존 업무 용어 사전과 DB 명명 규칙의 책임을 중복 정의하지 않는다.
6. 기존 코드가 본 표준과 다르다는 이유만으로 일괄 변경하지 않는다. 변경 영향과 호환성을 별도 검토한다.

## 2. 규칙 상태 구분

| 상태 | 의미 | 적용 원칙 |
| --- | --- | --- |
| **확정** | 사용자가 명시적으로 선택한 기준 | 확정 기준으로 유지 |
| **권장** | 검토 과정에서 채택한 기술적 권장안 | 신규 코드의 기본 기준으로 사용, 변경 검토 가능 |
| **참고** | 상황에 따라 적용하는 설명·예시 | 실제 구조와 계약에 맞게 적용 |
| **미확정** | 추가 검증 또는 별도 결정이 필요한 항목 | 확정 규칙처럼 강제하지 않음 |

## 3. 기존 표준과의 책임 경계

| 문서·설정 | 소유 범위 | 본 문서와의 관계 |
| --- | --- | --- |
| docs/global-business-term-dictionary.md | 445개 전역 업무 개념의 대표 영문명·한글명·확정 약어 | 업무 개념 의미 참조 |
| docs/database-naming-conventions.md | DB 테이블·컬럼·인덱스·제약조건 및 물리 약어 조합 | DB 물리 명칭은 해당 문서 적용 |
| .editorconfig | 인코딩·줄바꿈·들여쓰기·공백 등 편집기 기본 설정 | 편집기 설정 중복 정의 금지 |
| docs/architecture/project-structure.md | 프로젝트 디렉터리와 모듈별 책임 | 네임스페이스와 파일 분류 시 참조 |
| 본 문서 | C# 식별자, 파일 개요, XML 문서, 주석, UI·API 소스 명명 | 소스 개발 표준 |

### 3.1 업무 약어와 소스 식별자 분리

- 업무 용어 사전의 약어는 DB 물리 명칭 및 사전에서 정의한 용도에 사용한다.
- C# 변수·속성·메서드·클래스 이름에 DB 약어를 기계적으로 붙이지 않는다.
- C# 소스 식별자는 의미가 명확한 영문 단어를 우선한다.
- DB 컬럼명과 C# 속성명은 서로 다른 명명 규칙을 사용하며, EF Core 매핑 등 경계에서 명시적으로 연결한다.
- 자료형 접두사 str, int, bool 등을 일반 C# 변수명에 붙이지 않는다.

## 4. 기본 표기 규칙

### 4.1 식별자 표기 — 권장

| 대상 | 규칙 | 예시 |
| --- | --- | --- |
| 네임스페이스 | PascalCase | EDAN.Tensor.Platform.Tenants |
| 클래스·구조체·레코드·열거형 | PascalCase | TenantService |
| 인터페이스 | I + PascalCase | ITenantRepository |
| 메서드 | PascalCase, 동작 중심 | CreateTenantAsync |
| 공개·보호 속성 | PascalCase | TenantName |
| 이벤트 | PascalCase | TenantCreated |
| 매개변수·지역 변수 | camelCase | tenantId |
| 인스턴스 필드 | _camelCase | _tenantRepository |
| 비공개 정적 필드 | s_camelCase | s_instanceCount |
| 상수 | PascalCase | DefaultPageSize |
| 제네릭 형식 매개변수 | T 또는 역할 기반 T... | TEntity, TKey |

### 4.2 대소문자와 약어 — 권장

- C# 식별자는 PascalCase 또는 camelCase를 따른다.
- 두문자어는 일반적으로 첫 글자만 대문자로 표기한다.
- .NET 생태계에서 통용되는 고정 표기는 해당 기술 관례를 따른다.

| 지양 | 권장 |
| --- | --- |
| USERID | UserId |
| APIURL | ApiUrl |
| HTTPClient | HttpClient |
| JSONData | JsonData |
| XMLDocument | XmlDocument |
| GUID | Guid |
| URI | Uri |

업무 개념은 전역 업무 용어 사전의 대표 영문명을 우선 사용한다. 사전의 DB 약어를 C# 식별자에 그대로 적용하지 않는다. 임의 약어와 숫자만 다른 식별자는 지양한다.

## 5. 메서드 명명

### 5.1 기본 형식 — 권장

메서드는 수행 동작을 나타내는 동사로 시작하고, 필요한 경우 대상 또는 조건을 뒤에 둔다.

기본 형식: [동작] + [대상 또는 조건] + [Async]

| 목적 | 권장 동사 | 예시 |
| --- | --- | --- |
| 생성 | Create | CreateTenant |
| 등록 | Register | RegisterTenant |
| 수정 | Update | UpdateTenant |
| 삭제 | Delete | DeleteTenant |
| 검증 | Validate | ValidateTenant |
| 변환 | Convert | ConvertTenantData |
| 계산 | Calculate | CalculateUsage |
| 동기화 | Synchronize 또는 일관된 축약형 Sync | SyncTenantData |
| 초기화 | Initialize | InitializeTenantContext |
| 실행 | Execute | ExecuteTask |

메서드 이름은 실제 수행 동작과 일치해야 한다. 간결화를 이유로 핵심 업무 의미를 제거하지 않는다.

### 5.2 조회 동사 구분 — 사용자 확정

**상태: 확정. 조회 동사별 의미를 구분한다.**

| 동사 | 사용 의미 | 예시 |
| --- | --- | --- |
| Get | 식별된 필수 대상 또는 값 조회 | GetTenantAsync |
| Find | 조건에 맞는 대상 탐색. 결과가 없을 수 있음 | FindTenantAsync |
| Load | 화면·메모리·작업 컨텍스트에서 사용할 데이터 적재 | LoadTenantListAsync |
| Fetch | 외부 시스템 또는 원격 자원에서 데이터 가져오기 | FetchTenantDataAsync |
| Read | 파일·스트림·저장 매체의 원시 데이터 읽기 | ReadTenantFileAsync |

적용 원칙:

1. 같은 의미의 작업에 조회 동사를 임의로 혼용하지 않는다.
2. Get은 필수 대상 조회에 사용한다. 대상이 없을 때의 예외 또는 반환 정책은 메서드 계약에 명시한다.
3. Find는 검색 결과 없음이 정상적으로 발생할 수 있는 탐색에 사용한다.
4. Load는 단순 조회와 구별되는 적재·초기화 책임이 있을 때 사용한다.
5. Fetch는 외부·원격 경계에서 데이터를 가져오는 작업에 사용한다.
6. Read는 파일·스트림 등 저수준 읽기 작업에 사용한다.
7. 클래스명 또는 네임스페이스에 대상이 이미 명확하면 메서드명에서 중복 단어를 제거할 수 있다.

예시:

| 서비스 외부 공개 API | 저장소 내부 메서드 |
| --- | --- |
| GetTenantByIdAsync(tenantId) | GetByIdAsync(tenantId) |
| FindTenantByCodeAsync(code) | FindByCodeAsync(code) |

동일 계층에서는 같은 패턴을 일관되게 사용한다.

### 5.3 메서드 이름 길이 — 사용자 확정

**상태: 확정. 권장 길이와 예외 검토 기준을 사용한다.**

| 항목 | 기준 |
| --- | --- |
| 핵심 의미 단어 | 1`4개 권장 |
| 권장 길이 | 영문 기준 30자 이내 |
| 30자 초과 | 중복 의미와 간결화 가능성 검토 |
| 40자 초과 | 이름 구조 특별 검토 |
| 강제 길이 제한 | 적용하지 않음 |

길이보다 의미 정확성을 우선한다. 30자·40자 기준은 컴파일 제한이나 자동 실패 기준이 아니다.

### 5.4 비동기 메서드 — 권장

**상태: 권장. 실제 비동기 메서드에만 Async 접미사를 붙인다.**

- Task, Task<T>, ValueTask, ValueTask<T>를 반환하는 비동기 메서드는 Async로 끝낸다.
- 동기 메서드에는 Async를 붙이지 않는다.
- 내부에서 비동기 메서드를 호출한다는 이유만으로 동기 메서드에 Async를 붙이지 않는다.
- 이벤트 핸들러 등 프레임워크 규약이 있는 경우 해당 규약을 우선한다.
- async void는 UI 이벤트 핸들러 등 프레임워크가 요구하는 경우에만 사용한다.
- 비동기 호출에 .Result 또는 .Wait()를 사용해 동기 대기하는 패턴을 지양한다.
- 장시간 실행 또는 취소 가능한 작업은 CancellationToken을 받아 하위 호출에도 전파하는 방식을 권장한다.

| 동기 | 비동기 |
| --- | --- |
| GetTenant | GetTenantAsync |
| CreateTenant | CreateTenantAsync |
| ValidateTenant | ValidateTenantAsync |

### 5.5 존재 여부·상태 확인 — 권장

| 목적 | 권장 이름 |
| --- | --- |
| 존재 여부 | TenantExists, TenantExistsAsync |
| 상태 확인 | IsTenantActive |
| 권한 보유 | HasTenantAccess |
| 동작 가능 여부 | CanDeleteTenant |
| 재시도 필요 여부 | ShouldRetry |

Boolean 속성은 현재 상태를, 메서드는 확인 작업 또는 판단을 표현한다.

## 6. 클래스와 자료형 명명 — 권장

| 대상 | 규칙 | 예시 |
| --- | --- | --- |
| 구체 클래스 | 책임을 나타내는 명사 | TenantService |
| 인터페이스 | I + 계약 역할 | ITenantService |
| 추상 클래스 | 추상 책임을 나타내는 이름 | TenantProviderBase |
| 구조체 | 값의 의미를 나타내는 명사 | TenantInfo |
| 레코드 | 데이터 의미를 나타내는 명사 | TenantSummary |
| 열거형 | 상태·유형을 나타내는 단수 명사 | TenantStatus |
| 예외 | 상황 + Exception | TenantNotFoundException |
| 특성 | 의미 + Attribute | AuditAttribute |
| 대리자 | 동작 또는 이벤트 역할 | TenantChangedHandler |

- 클래스는 명사 중심, 메서드는 동사 중심으로 작성한다.
- 클래스명에 구현 기술을 불필요하게 노출하지 않는다.
- Helper, Util, Common, Manager처럼 책임이 불명확한 이름은 지양한다. 명확한 관리 책임을 표현하는 경우는 예외다.
- Base는 실제 상속 기반 추상 형식에만 사용한다.
- 클래스명과 메서드명의 의미가 중복되면 공개 범위를 고려해 한쪽에서 중복 단어를 제거한다.

## 7. 변수·필드·속성·상수 명명 — 권장

### 7.1 변수와 필드

| 대상 | 규칙 | 예시 |
| --- | --- | --- |
| 지역 변수 | camelCase | tenantCount |
| 매개변수 | camelCase | tenantId |
| 인스턴스 필드 | _camelCase | _tenantRepository |
| 비공개 정적 필드 | s_camelCase | s_instanceCount |
| 상수 | PascalCase | DefaultTimeoutSeconds |

- 자료형 접두사 str, int, bool 등을 변수명에 붙이지 않는다.
- 변수명은 값의 업무 의미와 사용 목적을 나타낸다.
- data, info, temp, value, item처럼 단독으로 의미가 모호한 이름은 구체화한다. 짧은 범위에서 의미가 명확한 경우는 허용한다.
- 반복문 인덱스 i, j 등은 짧은 범위에서 허용한다.
- 읽기 전용 인스턴스 의존성은 readonly 필드로 선언하는 방식을 권장한다.

### 7.2 속성

| 의미 | 권장 이름 |
| --- | --- |
| 일반 값 | TenantName |
| 개수 | TenantCount |
| 상태 | TenantStatus |
| 생성 시각 | CreatedAtUtc |
| 활성 여부 | IsActive |
| 권한 보유 여부 | HasPermission |
| 삭제 가능 여부 | CanDelete |
| 재시도 필요 여부 | ShouldRetry |

Boolean 속성은 Is, Has, Can, Should 등을 의미에 맞게 사용한다. 설정값에는 Enable 또는 Allow를 사용할 수 있으나, 상태값과 정책 설정값을 구분한다.

### 7.3 상수와 읽기 전용 필드

- 컴파일 시점 상수는 const와 PascalCase를 사용한다.
- 실행 시점 초기화가 필요한 정적 읽기 전용 필드는 static readonly와 s_camelCase를 사용한다.
- 인스턴스 읽기 전용 필드는 readonly와 _camelCase를 사용한다.
- 변경 가능한 설정값을 상수처럼 취급하지 않는다.

## 8. 컬렉션·열거형·제네릭 — 권장

### 8.1 컬렉션

컬렉션은 복수형을 기본으로 하고, 자료구조를 구분할 필요가 있을 때 의미 있는 접미사를 사용한다.

| 유형 | 예시 |
| --- | --- |
| 복수 항목 | tenants |
| 목록 | tenantList |
| 검색 결과 | tenantResults |
| 사전 | tenantMap |
| 집합 | tenantIds |
| 배열 | tenantIds |

```csharp
IReadOnlyList<Tenant> tenants;
Dictionary<Guid, Tenant> tenantMap;
HashSet<Guid> tenantIds;
```

### 8.2 열거형

- 열거형 형식명은 PascalCase 단수형을 사용한다.
- 항목은 PascalCase로 작성한다.
- 비트 플래그 열거형은 Flags를 적용하고 복수형 이름을 권장한다.
- 외부 계약의 숫자 값은 호환성 검토 없이 변경하지 않는다.

```csharp
public enum TenantStatus
{
    Pending,
    Active,
    Suspended,
    Terminated
}

[Flags]
public enum TenantPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4
}
```

### 8.3 제네릭 형식 매개변수

| 의미 | 이름 |
| --- | --- |
| 일반 형식 | T |
| 키 | TKey |
| 값 | TValue |
| 엔터티 | TEntity |
| 요청 | TRequest |
| 응답 | TResponse |

## 9. 이벤트·명령·콜백 — 권장

| 대상 | 규칙 | 예시 |
| --- | --- | --- |
| 이벤트 | 발생한 사실을 과거형으로 표현 | TenantCreated |
| 이벤트 인수 | 이벤트명 + EventArgs | TenantCreatedEventArgs |
| 이벤트 발생 메서드 | On + 이벤트명 | OnTenantCreated |
| 명령 | 동작 + 대상 + Command | SaveTenantCommand |
| 처리기 | 대상 + Handler | TenantChangedHandler |
| 콜백 | 역할 + Callback 또는 Handler | TenantChangedCallback |

이벤트는 발생한 사실을 표현한다. 이벤트 처리기와 UI 프레임워크가 요구하는 시그니처는 프레임워크 규약을 우선한다.

## 10. 네임스페이스와 폴더 — 권장

- 네임스페이스는 PascalCase를 사용한다.
- 제품·모듈·업무 영역·책임 계층을 반영한다.
- 폴더 구조와 네임스페이스는 가능한 한 일치시킨다.
- 기존 프로젝트의 모듈 경계와 계층 구조를 임의로 재명명하지 않는다.

예시:

```text
EDAN.Tensor.Platform.Tenants
EDAN.Tensor.Platform.Tenants.Application
EDAN.Tensor.Platform.Tenants.Infrastructure
EDAN.Tensor.Licensing.Contracts
```

## 11. API·DTO·메시지 계약 — 권장

| 대상 | 권장 이름 | 예시 |
| --- | --- | --- |
| Controller | 업무 영역 + Controller | TenantController |
| 생성 요청 | 동작 + 대상 + Request | CreateTenantRequest |
| 수정 요청 | 동작 + 대상 + Request | UpdateTenantRequest |
| 응답 | 대상 + Response | TenantResponse |
| 상세 응답 | 대상 + DetailResponse | TenantDetailResponse |
| 목록 응답 | 대상 + ListResponse | TenantListResponse |
| 이벤트 계약 | 대상 + 동작 + Event | TenantCreatedEvent |
| 메시지 계약 | 대상 + 동작 + Message | TenantCreatedMessage |

- 요청·응답·이벤트·메시지는 외부 계약 여부를 명확히 한다.
- 계약 필드의 의미와 직렬화 이름은 소스 속성명과 별도로 호환성을 검토한다.
- API 경로·HTTP 동사·버전 정책은 API 설계 문서에서 관리하고 본 문서에 중복 정의하지 않는다.
- 모듈 간 계약은 기존 아키텍처의 Contracts 경계를 따른다.

## 12. UI·WPF 명명 — 권장

### 12.1 화면 및 ViewModel

| 대상 | 권장 이름 |
| --- | --- |
| 화면 | TenantListView |
| ViewModel | TenantListViewModel |
| 명령 | SaveTenantCommand |
| 화면 전용 서비스 계약 | 실제 책임 기반 이름 |

View와 ViewModel은 대응 관계를 유지하고, 화면명은 실제 업무 기능을 나타낸다.

### 12.2 컨트롤

| 컨트롤 | 권장 예시 |
| --- | --- |
| Button | SaveButton |
| TextBox | TenantNameTextBox |
| ComboBox | TenantStatusComboBox |
| DataGrid | TenantDataGrid |
| CheckBox | IsActiveCheckBox |
| DatePicker | EffectiveDatePicker |

- 컨트롤 이름은 업무 대상과 컨트롤 종류를 식별할 수 있게 작성한다.
- Button1, TextBox2 등 자동 생성 이름을 업무 코드에 그대로 사용하지 않는다.
- 모든 컨트롤에 이름을 부여할 필요는 없다. 코드·바인딩·자동화 테스트 등에서 식별할 필요가 있는 컨트롤에 적용한다.

## 13. 화면 ID 및 Screen Registry

### 13.1 화면 ID 형식 — 사용자 확정

**상태: 확정. 모듈 + 업무 영역 + 일련번호**

형식: [MODULE]-[BUSINESS AREA]-[SEQUENCE]

| 화면 ID | 설명 |
| --- | --- |
| PLT-TNT-001 | Platform 테넌트 영역 첫 번째 화면 |
| DSET-DSM-001 | DataSet 데이터셋 관리 영역 첫 번째 화면 |
| WRK-TSK-001 | Worker 작업 관리 영역 첫 번째 화면 |
| COML-PAY-001 | Commercial 결제 영역 첫 번째 화면 |
| LIC-LCM-001 | Licensing 라이선스 수명주기 영역 첫 번째 화면 |

모듈·업무 영역 코드는 [Screen Module·Area Code Registry](../registry/screen-module-area-codes.csv)를 단일 기준으로 사용한다. 화면 ID는 제품 전체에서 중복되지 않아야 하며 폐기된 ID를 다른 화면에 재사용하지 않는다.

### 13.2 화면 정보 관리 — 사용자 확정

**상태: 확정. Screen Registry 별도 관리와 소스 파일 기록을 병행한다.**

Screen Registry는 화면 단위의 공식 목록으로 관리한다. 상세한 ID 발급, 화면명, View·ViewModel·Service 연결, 상태, CSV 구조, 변경 절차 및 검증 규칙은 [EDAN Screen Registry 관리 표준](EDAN-SCREEN-REGISTRY-STANDARD.md)을 따른다.

Registry 데이터는 다음 파일에서 관리한다.

- 화면 기본 정보: ../registry/screens.csv
- 화면과 Service 계약 관계: ../registry/screen-service-bindings.csv
- MODULE·AREA 코드 사전: ../registry/screen-module-area-codes.csv

파일 상단 개요에는 화면명과 화면 ID 등 해당 파일의 핵심 정보를 기록한다. 화면 Registry의 필드 정의와 생명주기 규칙은 본 절에서 중복 정의하지 않고 전용 표준 문서에서 관리한다. MODULE·AREA 코드는 별도 코드 Registry를 단일 기준으로 사용한다.

## 14. C# 파일 상단 개요

### 14.1 적용 범위 — 사용자 확정

**상태: 확정. 모든 .cs 파일에 적용하며 파일 유형별 템플릿을 사용한다.**

파일 상단 개요는 해당 파일의 목적과 책임을 설명한다. 화면 전용 항목을 비화면 파일에 강제로 작성하지 않는다.

공통 필드:

- 파일명
- 분류 또는 모듈 경로
- 목적
- 주요 책임 또는 기능
- 주요 연결 요소(해당하는 경우)

파일 개요는 using 선언 및 네임스페이스 선언보다 앞에 둔다. 파일명은 실제 파일명과 일치시킨다.

### 14.2 화면 파일 템플릿

```csharp
// =============================================================================
// 파일명         : TenantListView.xaml.cs
// 화면명         : 테넌트 목록
// 화면 ID        : PLT-TNT-001
// 분류           : Platform / Tenant
// 목적           : 테넌트 정보의 조회 및 관리
// 주요 기능      : 목록 조회, 검색, 등록, 수정, 비활성화
// 구성 영역      : 검색 영역, 목록 영역, 상세 정보 영역
// 연결 ViewModel : TenantListViewModel
// 연결 Service   : ITenantService
// =============================================================================
```

### 14.3 비화면 파일 템플릿

```csharp
// =============================================================================
// 파일명       : TenantService.cs
// 분류         : Platform / Tenant / Application
// 목적         : 테넌트 업무 처리
// 주요 책임    : 테넌트 조회, 등록, 수정 및 상태 변경
// 연결 요소    : ITenantRepository
// =============================================================================
```

### 14.4 파일 유형별 기재 항목

| 파일 유형 | 주요 기재 항목 |
| --- | --- |
| View | 화면명, 화면 ID, 목적, 주요 기능, 구성 영역, ViewModel, Service |
| ViewModel | 대상 화면, 화면 ID, 상태 관리 목적, 주요 명령, 연결 Service |
| Service | 업무 영역, 처리 목적, 주요 기능, 연결 Repository 또는 계약 |
| Repository | 데이터 영역, 저장소 책임, 주요 데이터 접근 기능 |
| Entity | 도메인 영역, 엔터티 책임, 주요 상태 또는 관계 |
| DTO | 전달 목적, 사용 경계, 계약 역할 |
| Interface | 계약 목적, 책임, 주요 구현 경계 |
| Controller | API 영역, 제공 기능, 연결 Application 서비스 |
| Worker | 작업 목적, 실행 방식, 처리 범위, 연결 구성 요소 |
| 기타 | 파일명, 분류, 목적, 주요 책임 |

파일 개요와 XML Summary는 같은 문장을 반복하지 않는다. 파일 개요는 파일 전체 맥락, XML Summary는 타입 또는 멤버의 책임을 설명한다.

## 15. XML 문서 및 주석

### 15.1 문체 — 사용자 확정

**상태: 확정. A 방식인 기술 설명형 단축 문장**

- 불필요한 `입니다, `합니다 종결어미를 사용하지 않는다.
- 짧지만 의미가 완결된 기술 설명형 표현을 사용한다.
- 지나친 축약으로 의미가 불분명해지지 않도록 한다.
- 문서와 주석은 한국어로 작성한다. 코드 식별자와 기술 고유명은 원문 표기를 유지한다.

예시:

- 권장: 테넌트 식별자 기준 단건 조회
- 지양: 테넌트 식별자를 이용하여 테넌트 정보를 조회합니다.
- 과도한 축약 지양: 테넌트 조회 (조회 기준과 범위가 중요한 경우)

### 15.2 XML 요소별 작성

| 요소 | 작성 내용 |
| --- | --- |
| summary | 타입·멤버의 핵심 책임 또는 동작 |
| param | 매개변수의 의미, 단위, 허용 조건 |
| returns | 반환 결과와 null·빈 결과 등 조건 |
| exception | 발생 가능한 예외와 발생 조건 |
| remarks | 권한·보안·일관성·동시성·제약 등 추가 설명 |
| example | 호출 또는 사용 예시 |

```csharp
/// <summary>
/// 테넌트 식별자 기준 단건 조회
/// </summary>
/// <param name="tenantId">조회 대상 테넌트 식별자</param>
/// <returns>조회된 테넌트. 없으면 null</returns>
public Task<Tenant?> FindTenantAsync(Guid tenantId);
```

실제 반환 형식과 계약이 예시와 다르면 문서도 실제 계약에 맞춰 작성한다.

### 15.3 XML 문서 적용 범위 — 권장

다음 공개 계약에는 XML 문서를 작성한다.

- Public 및 Protected 클래스·인터페이스·구조체·레코드·열거형
- Public 및 Protected 메서드·속성·이벤트
- 외부 모듈 또는 외부 소비자에게 공개되는 계약

Private 멤버는 복잡한 동작이나 비자명한 제약이 있는 경우 작성한다. 모든 지역 변수에 주석을 붙이지 않는다. 프레임워크가 요구하는 구현 멤버는 실제 문서 생성 설정과 분석기 정책을 함께 확인한다.

### 15.4 일반 주석

주석은 코드만으로 알기 어려운 설계 의도·제약·예외·보안·성능·복구 근거를 설명한다.

주석을 권장하는 중요 로직:

- 트랜잭션 및 데이터 일관성
- 테넌트 격리와 권한 검사
- 중복 실행 방지와 멱등성
- 재시도 및 보상 트랜잭션
- 외부 시스템 연계
- 동시성·잠금·순서 보장
- 캐시 일관성
- 보존·삭제·복구 정책

```csharp
// 테넌트 범위와 Resource 권한을 모두 충족해야 실행 허용
```

코드 내용을 그대로 한국어로 반복하는 주석은 작성하지 않는다.

### 15.5 작업 표시 주석

- TODO: 후속 구현 또는 개선 필요
- FIXME: 확인된 결함 또는 잘못된 동작
- NOTE: 유지보수에 필요한 주의사항

작업 표시에는 가능하면 이슈 번호 또는 추적 가능한 근거를 함께 기록한다. 해결되지 않은 작업을 완료된 것으로 표현하지 않는다.

## 16. 코드 구조와 선언 순서 — 권장

다음 순서는 권장 기본값이다. 프레임워크 요구사항과 기존 파일 구조가 우선하는 경우 합리적인 예외를 허용한다.

### 16.1 파일 구성

1. 파일 상단 개요 주석
2. using 선언
3. 네임스페이스
4. 타입 XML Summary
5. 타입 선언

### 16.2 클래스 내부 구성

1. 상수 및 정적 필드
2. 인스턴스 필드
3. 생성자
4. 이벤트
5. 속성
6. 공개 메서드
7. 보호 메서드
8. 내부 메서드
9. 비공개 메서드
10. 중첩 타입

관련 멤버는 기능 단위로 가까이 배치할 수 있다. 선언 순서를 위해 관련 로직을 불필요하게 분리하지 않는다.

## 17. 예외·null·비동기·보안 — 권장

### 17.1 예외

- 예외 타입은 상황을 구체적으로 나타내고 Exception으로 끝낸다.
- 정상적인 검색 결과 없음이나 사용자 입력 검증 실패에 예외를 남용하지 않는다.
- 예외 메시지는 원인과 대상을 설명하되 비밀번호·토큰·개인정보 등 민감정보를 포함하지 않는다.
- 예외를 잡은 뒤 아무 처리 없이 무시하지 않는다.
- 예외를 다시 던질 때 원래 Stack Trace를 보존한다.

### 17.2 null 및 반환 계약

- Nullable Reference Types를 활성화한 프로젝트 설정을 존중한다.
- 반환값이 없을 수 있으면 nullable 반환 형식 또는 명시적 결과 타입으로 표현한다.
- 빈 컬렉션을 반환하는 메서드는 가능한 경우 null 대신 빈 컬렉션을 사용한다.
- Get과 Find의 결과 없음 정책은 XML 문서와 실제 구현이 일치해야 한다.

### 17.3 비동기 및 취소

- 비동기 호출은 가능한 한 끝까지 비동기 흐름으로 유지한다.
- 장시간 실행 또는 취소 가능한 작업은 CancellationToken을 받아 하위 호출에 전달한다.
- 동기 API와 비동기 API를 중복 제공할 때는 실제 소비 요구와 교착 위험을 검토한다.
- 라이브러리 코드의 컨텍스트 캡처 정책은 UI·ASP.NET Core·Worker 실행 환경을 고려한다. 일괄적인 ConfigureAwait(false) 적용을 강제하지 않는다.

### 17.4 보안 및 멀티테넌트

- 테넌트 소유 데이터는 요청 컨텍스트와 저장소 접근 범위에서 테넌트 경계를 확인한다.
- 인증된 사용자와 대상 리소스의 권한을 모두 확인한다.
- 로그와 예외에 비밀정보 및 민감한 데이터 값을 기록하지 않는다.
- 권한 검사·격리·감사 기록은 중요한 설계 의도를 주석 또는 XML remarks로 설명한다.

## 18. 통합 예시

아래 예시는 명명·문서화 형식을 설명하기 위한 예시이며, 현재 저장소의 실제 구현이나 공개 API 존재를 의미하지 않는다.

```csharp
/// <summary>
/// 테넌트 업무 처리
/// </summary>
public sealed class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;

    /// <summary>
    /// 테넌트 서비스 초기화
    /// </summary>
    /// <param name="tenantRepository">테넌트 저장소 계약</param>
    public TenantService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    /// <summary>
    /// 테넌트 식별자 기준 단건 조회
    /// </summary>
    /// <param name="tenantId">조회 대상 테넌트 식별자</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>조회된 테넌트. 없으면 null</returns>
    public Task<Tenant?> FindTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _tenantRepository.FindByIdAsync(
            tenantId,
            cancellationToken);
    }
}
```

## 19. 신규 코드 검토 체크리스트

### 명명
- [ ] 클래스·메서드·속성·변수의 대소문자 규칙이 일관적인가?
- [ ] 클래스는 명사 중심, 메서드는 동사 중심인가?
- [ ] Get, Find, Load, Fetch, Read의 의미가 구분되는가?
- [ ] 비동기 메서드에만 Async 접미사를 사용하는가?
- [ ] 메서드 이름이 불필요하게 길거나 의미가 중복되지 않는가?
- [ ] 자료형 접두사와 DB 전용 약어를 C# 변수명에 사용하지 않았는가?
- [ ] Boolean 이름이 상태·보유·가능·정책 의미를 명확히 나타내는가?
- [ ] 인터페이스와 예외·이벤트 이름이 역할을 나타내는가?

### 파일·화면
- [ ] 모든 .cs 파일에 파일 상단 개요가 있는가?
- [ ] 파일 유형에 맞는 항목만 기재했는가?
- [ ] 화면 파일의 화면 ID가 Screen Registry와 일치하는가?
- [ ] View와 ViewModel의 연결 관계가 명확한가?
- [ ] 파일 개요와 XML Summary가 불필요하게 중복되지 않는가?

### 문서화·품질
- [ ] 공개 계약의 XML Summary와 매개변수·반환 설명이 있는가?
- [ ] null·예외·권한·취소 조건이 실제 동작과 일치하는가?
- [ ] 주석이 코드 반복이 아니라 설계 의도와 제약을 설명하는가?
- [ ] 민감정보를 로그·예외·주석에 포함하지 않았는가?
- [ ] 기존 아키텍처 및 Contracts 경계를 준수하는가?
- [ ] DB 물리 명칭은 DB 명명 규칙과 전역 업무 사전을 따르는가?

## 20. 표준 변경 및 예외 관리

1. 확정 규칙 변경은 변경 사유와 영향 범위를 검토한 후 별도 승인한다.
2. 권장 규칙 변경은 기존 코드와 신규 코드의 일관성, 도구 지원, 유지보수 영향을 검토한다.
3. 예외는 대상 파일·식별자·사유·영향·해소 계획을 기록한다.
4. 기존 코드가 본 표준과 다르다는 이유만으로 일괄 변경하지 않는다.
5. 기존 API·DB·메시지 계약은 호환성 및 Migration 계획 없이 이름을 변경하지 않는다.
6. 표준과 실제 구현이 다르면 준수로 간주하지 않고 차이와 예외를 기록한다.

## 21. 확정·권장·미확정 항목 요약

### 21.1 사용자 확정 사항

| 항목 | 확정 내용 |
| --- | --- |
| XML Summary 및 주석 문체 | 기술 설명형 단축 문장(A 방식) |
| 화면 ID | 모듈 + 업무 영역 + 일련번호 |
| 파일 상단 개요 적용 범위 | 모든 .cs 파일, 유형별 템플릿 |
| 화면 정보 관리 | Screen Registry 별도 관리 + 소스 파일에 핵심 정보 기록 |
| 조회 동사 | Get, Find, Load, Fetch, Read 의미 구분 |
| 메서드 길이 | 30자 이내 권장, 초과 검토, 40자 초과 특별 검토, 강제 제한 없음 |

### 21.2 권장안으로 정리한 사항

| 항목 | 권장 기준 |
| --- | --- |
| C# 식별자 표기 | PascalCase / camelCase |
| 인터페이스 | I 접두사 |
| 인스턴스 필드 | _camelCase |
| 비공개 정적 필드 | s_camelCase |
| 상수 | PascalCase |
| 비동기 메서드 | 실제 비동기 메서드에 Async 접미사 |
| 컬렉션 | 복수형 |
| Boolean | Is, Has, Can, Should 등 의미별 접두사 |
| XML 문서 | 공개·보호 계약에 작성 |
| 파일 상단 개요 | 모든 C# 파일에 유형별 템플릿 적용 |
| 코드 구조 | 일관된 멤버 선언 순서 권장 |

### 21.3 별도 검증이 필요한 사항

| 항목 | 후속 검증 내용 |
| --- | --- |
| 화면 모듈·업무 영역 코드 | 실제 EDAN 모듈 및 업무 영역과 대조 |
| Screen Registry 경로·상태값 | 별도 등록 문서에서 확정 |
| 메서드 분석기 | 길이 기준과 XML 문서 누락 검사를 자동화할지 검토 |
| 기존 소스 적용 | 현재 소스의 명명 편차와 변경 영향 분석 |
| API·DTO 세부 규칙 | 실제 공개 계약과 버전 정책 대조 |
| 파일 헤더 자동 검사 | 필수 필드 및 유형별 템플릿 검사 가능성 확인 |

## 22. 참고 문서

- [문서 안내](../README.md)
- [프로젝트 구조](../architecture/project-structure.md)
- [전역 업무 용어·약어 확정 사전](../global-business-term-dictionary.md)
- [데이터베이스 테이블명·컬럼명 표준 명명 규칙](../database-naming-conventions.md)
- [Microsoft C# 식별자 명명 규칙](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names)
- [Microsoft .NET 라이브러리 설계 지침](https://learn.microsoft.com/dotnet/standard/design-guidelines/naming-guidelines)

## 변경 이력

| 버전 | 일자 | 내용 |
| --- | --- | --- |
| 0.1 | 2026-11-15 | 사용자 확정 기준과 권장안을 구분한 소스 개발 표준 통합 초안 작성 |
