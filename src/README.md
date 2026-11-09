# 소스 프로젝트

제품 코드는 모듈 경계와 프로세스 Host별로 구성됩니다. 자세한 규칙은 [의존성 규칙](../docs/architecture/dependency-rules.md)을 참고하세요.
## Client

`EDAN.Tensor.Client.Contracts`는 Platform·DataSet·Worker API의 공유 요청·응답 계약을 제공하고, `EDAN.Tensor.Client`는 인증·Tenant·Idempotency 헤더를 보존하는 typed HTTP Client를 제공합니다. WPF 화면과 Commercial·Licensing 업무 흐름은 별도 승인 계약이 제공된 뒤 추가합니다.
