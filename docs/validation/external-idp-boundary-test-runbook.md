# 비운영 외부 IdP 경계 수동 검증 런북

이 문서는 실제 비운영 외부 IdP와 테스트 Tenant가 제공된 뒤 **External** 인증 경계를 한 번 실행하고 증거를 남기는 절차를 정의합니다. 이 문서는 예약 작업이나 자동화를 만들지 않으며, 실제 외부 환경이 없을 때 검증을 완료한 것으로 표시하지 않습니다.

## 검증 전제

검증 담당자는 다음 값을 외부 환경 관리자에게 받아야 합니다.

| 항목 | 검증 조건 |
| --- | --- |
| Metadata 주소 | 실제 비운영 IdP의 OpenID Connect Metadata HTTPS 주소입니다. |
| JWKS 주소 | Metadata 응답의 **jwks_uri**이며 HTTPS 주소여야 합니다. |
| Issuer | Metadata의 **issuer**와 일치해야 합니다. |
| Audience | API가 발급받은 토큰에서 요구하는 대상 식별자여야 합니다. |
| 테스트 Tenant | 운영과 분리된 Tenant 코드와 API에서 사용할 Tenant 식별자입니다. |
| 테스트 사용자 | 안정적으로 유지되는 opaque **sub**를 가진 사용자입니다. 이메일 주소만으로 사용자를 식별하지 않습니다. |
| 키 회전 권한 | 기존 키와 새 키를 겹쳐 제공하고, 회전 후 기존 키를 폐기할 수 있는 비운영 권한입니다. |

개인키, 클라이언트 비밀값, 비밀번호, 액세스 토큰은 저장소·문서·CI 변수 출력·로그에 기록하지 않습니다. 테스트 식별자는 공개해도 되는 비운영 값만 기록하고, 필요하면 일부를 마스킹합니다.

## 실행 준비

1. 검증 시점의 **main SHA**와 실행 주체, 비운영 환경 이름을 기록합니다.
2. API Host를 **Development Bypass**가 아닌 격리된 **External** 설정으로 시작합니다.
3. 다음 설정은 환경 변수 또는 비밀 저장소에서 주입합니다.

```text
Authentication__Mode=External
Authentication__MetadataAddress=<실제 비운영 Metadata 주소>
Authentication__Issuer=<Metadata의 issuer 값>
Authentication__Audience=<API Audience 값>
Authentication__RequireHttpsMetadata=true
```

4. **Authentication__Mode=Bypass**를 설정하지 않습니다. Bypass 경고 로그가 나타나거나 비개발 환경에서 Bypass가 선택되면 검증을 중단하고 실패로 기록합니다. Development Bypass의 주체·Tenant·IdP 시뮬레이션 값은 코드의 고정 프로필에서만 제공되며, 이 런북의 External 검증에는 사용하지 않습니다.
5. API 로그와 IdP 감사 로그의 보존 위치를 확인하되, 토큰 원문과 개인키가 로그에 남지 않는지 먼저 확인합니다.

### 보호 API와 판정 기준

외부 IdP 검증에서 사용하는 보호 경로는 `GET /api/platform/tenants/{tenantId}/storage-usage`입니다. 이 경로는 `TenantMember` 권한과 Tenant 경로·소속 검사를 모두 적용하므로, 다음 결과를 판정 기준으로 사용합니다.

| 주체 상태 | 요청 | 기대 결과 |
| --- | --- | --- |
| 테스트 Tenant에 소속된 유효 주체 | 올바른 `tenantId`로 `storage-usage` 호출 | HTTP 200 |
| 유효하지만 해당 Tenant에 소속되지 않은 주체 | 같은 `tenantId`로 `storage-usage` 호출 | HTTP 403 |
| 유효한 주체가 다른 Tenant를 선택 | 다른 `tenantId`로 `storage-usage` 호출 | HTTP 403 |
| 토큰이 없거나 검증에 실패한 주체 | 같은 경로 호출 | HTTP 401 |

`GET /api/platform/tenants/mine`은 주체의 소속 목록 조회 경로입니다. 미가입 주체가 빈 목록으로 HTTP 200을 받을 수 있으므로, 미가입 주체의 403 수용 기준을 확인할 때 이 경로를 사용하지 않습니다.

### 토큰 발급과 안전한 전달 예시

다음 예시는 Ubuntu VM의 SSH 터미널에서 실행합니다. `<...>` 부분만 비운영 환경의 값으로 바꾸고, 비밀번호를 명령줄에 직접 입력하지 않습니다. 토큰 파일은 검증이 끝난 뒤 삭제합니다.

```text
umask 077
T='https://<idp-host>:<https-port>/realms/<realm>/protocol/openid-connect/token'
C='<client-id>'
U='<test-username>'
read -r -s -p 'Keycloak 테스트 사용자 비밀번호: ' P
echo
curl -k -sS "$T" \
  -d "client_id=$C" \
  -d 'grant_type=password' \
  -d "username=$U" \
  --data-urlencode "password=$P" \
  -d 'scope=openid' > /tmp/edan-external-token.json
unset P
jq '{ok:(.access_token != null),token_type,expires_in,error,error_description}' /tmp/edan-external-token.json
```

정상 결과의 `ok`가 `true`인지 확인한 후, 토큰 원문을 문서나 로그에 복사하지 않습니다. Windows PowerShell에서 API를 호출해야 하면 SSH로 토큰을 한 번만 읽어 메모리에 보관하고, 응답 상태만 기록합니다.

```text
$Token = (ssh -T <ssh-user>@<vm-ip> "jq -r '.access_token' /tmp/edan-external-token.json").Trim()
$Headers = @{ Authorization = "Bearer $Token" }
Invoke-RestMethod -UseBasicParsing `
  -Uri "http://localhost:5000/api/platform/tenants/<tenant-id>/storage-usage" `
  -Headers $Headers
Remove-Variable Token -ErrorAction SilentlyContinue
```

검증이 끝난 뒤 VM에서 `rm -f /tmp/edan-external-token.json`을 실행하고 PowerShell의 토큰 변수도 삭제합니다. 화면 캡처에는 토큰, 비밀번호, 개인키가 보이지 않아야 합니다.

## 검증 순서

### 1. Metadata와 JWKS 경계

- Metadata 주소가 HTTPS로 연결되고 응답의 **issuer**가 설정한 Issuer와 정확히 일치하는지 확인합니다.
- Metadata 응답의 **jwks_uri**가 예상한 비운영 JWKS 주소를 가리키는지 확인합니다.
- JWKS 응답에 공개키의 **kid**, **kty**, **n**, **e**가 있고 개인키 필드가 없는지 확인합니다.
- 현재 검증 대상 API가 Metadata에서 받은 JWKS를 사용하고, 저장소의 고정 대칭키나 Development Bypass를 사용하지 않는지 로그와 설정으로 확인합니다.

### 2. 정상 토큰과 Tenant 격리

- 유효한 비운영 토큰의 **iss**, **aud**, 만료 시각, 서명 **kid**, 안정적인 **sub**를 확인합니다.
- 테스트 사용자의 안정적인 **sub**를 테스트 Tenant의 Member 또는 Administrator 소속에 등록합니다.
- 유효한 토큰과 올바른 Tenant 선택으로 보호 API를 호출하여 예상된 성공 응답을 확인합니다.
- 같은 토큰으로 다른 Tenant를 선택하면 HTTP 403이 반환되고, 다른 Tenant의 데이터가 반환되지 않는지 확인합니다.
- Tenant 소속이 없는 유효한 주체가 `storage-usage` 보호 API에 접근하면 HTTP 403이 반환되는지 확인합니다.

미가입 주체를 확인하려면 테스트 Tenant에 소속되지 않은 별도 테스트 사용자를 만들고, 그 사용자의 안정적인 `sub`를 기록합니다. 그 주체에게 Tenant 멤버십을 부여하지 않은 상태로 동일한 보호 API를 호출합니다. `/mine`의 빈 목록 여부만으로 403을 판정하지 않습니다.

### 3. 실패 토큰 경계

다음 각 조건은 보호 API에서 HTTP 401을 반환해야 합니다.

| 조건 | 확인 내용 |
| --- | --- |
| 토큰 없음 | 인증되지 않은 요청이 보호 API를 통과하지 않습니다. |
| 잘못된 Issuer | 신뢰하지 않는 발급자의 토큰을 거부합니다. |
| 잘못된 Audience | 다른 서비스용 토큰을 거부합니다. |
| 만료 토큰 | 만료된 토큰을 거부합니다. |
| 서명 변경 | Payload 또는 서명이 변경된 토큰을 거부합니다. |
| 알 수 없는 **kid** | 신뢰하지 않는 키 식별자의 토큰을 거부합니다. |

상태 확인 경로의 공개 응답과 보호 API의 401·403 응답을 혼동하지 않고 각각 기록합니다.

잘못된 Issuer 또는 Audience 검증은 JWT의 Payload 문자열만 수정해서 만들 수 없습니다. Payload를 수정하면 기존 서명이 깨지므로 결과는 단순한 서명 오류가 됩니다. 이 두 수용 기준을 실제 외부 IdP에서 충족하려면 다음 중 하나를 사용해야 합니다.

1. 신뢰하지 않는 별도 비운영 Realm/Issuer에서 동일한 테스트용 키로 정상 서명한 토큰을 발급합니다.
2. 외부 IdP 관리자가 제공하는 별도 비운영 서명 도구로 정상 서명한 테스트 토큰을 발급합니다.

그 토큰의 서명 키가 검증 대상 API의 신뢰 JWKS에 포함되지 않는 경우에는 먼저 `kid`와 JWKS 관계를 기록하고, 결과가 401인지 확인합니다. 토큰을 임의로 조작하여 “유효한 서명”으로 기록하지 않습니다.

### 4. 키 회전 경계

1. 회전 전에 현재 JWKS의 서명용 `kid`와 각 키 공급자의 `priority`, `enabled`, `active` 상태만 기록합니다. 개인키와 인증서 내용은 기록하지 않습니다.
2. 새 키를 생성하고 기존 키와 새 키가 동시에 JWKS에 게시된 회전 구간을 시작한 뒤 회전 시각을 기록합니다.
3. 기존 키 토큰을 하나 발급하고, 새 키가 선택되도록 한 뒤 새 키 토큰을 하나 발급합니다. 각 토큰의 `kid`만 디코드하여 기록합니다.
4. 두 키가 모두 게시되고 검증 가능한 겹침 구간에서는 기존 토큰과 새 토큰을 각각 보호 API에 보내 HTTP 200이 되는지 확인합니다.
5. 새 **kid**를 API가 조회할 때 Metadata·JWKS 재조회가 발생하고, 새 키 토큰이 계속 HTTP 200인지 확인합니다.
6. 기존 키 공급자를 먼저 `Active=Off`, `Enabled=On`으로 전환하여 신규 발급에서 제외하고, 새 토큰이 HTTP 200인지 다시 확인합니다. 기존 토큰은 아직 JWKS에 남아 있으므로 이 단계의 결과를 “폐기 완료”로 기록하지 않습니다.
7. 기존 키를 JWKS에서 제거하거나 공급자를 비활성화한 뒤 기존 키 토큰은 HTTP 401, 새 키 토큰은 HTTP 200인지 확인합니다. 이때 API가 새 JWKS를 다시 읽을 수 있도록 필요하면 새 요청을 보내고 결과를 기록합니다.
8. 회전 전후의 키 회전 권한 로그와 API 결과를 대조합니다. 개인키 자체는 증거에 포함하지 않습니다.

## 증거 기록 양식

다음 정보만 기록하고 비밀값은 기록하지 않습니다.

```text
검증 일시(UTC):
검증 담당자:
저장소 main SHA:
API Host 실행 환경:
비운영 IdP 식별자:
Metadata 주소의 호스트:
JWKS 주소의 호스트:
Issuer/Audience 식별자:
테스트 Tenant 식별자:
테스트 주체 sub(마스킹 또는 허용된 값):
기존 kid와 신규 kid:
정상 토큰 결과:
Tenant 격리 결과:
실패 토큰 결과:
키 회전 결과:
Bypass 로그 발생 여부:
토큰·개인키·비밀번호 노출 여부:
정리 및 권한 회수 결과:
판정: 통과 / 실패 / 미완료
```

검증 결과는 테스트 실행 시각과 API 응답 상태를 포함해야 하며, 로그만으로 성공을 추정하지 않습니다. 한 항목이라도 실행되지 않았거나 실제 외부 IdP가 아닌 로컬 시뮬레이션이었다면 전체 외부 경계 판정은 **미완료**입니다.

## 정리와 재현 방지

검증 후 테스트 사용자의 권한과 키 회전 권한을 회수하고, 임시 토큰·로그·환경 변수를 삭제합니다. API 설정을 운영 구성에 복사하지 않으며, CI에 외부 IdP 비밀값을 추가하지 않습니다. 검증 중 Bypass를 사용한 경우 결과를 통과로 기록하지 않고 External 모드로 다시 실행합니다.

