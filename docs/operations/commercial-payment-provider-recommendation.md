# Commercial 결제 공급자 권고 기록

결제 공급자는 이번 구현에서 확정하지 않습니다. 아래 순서는 소프트웨어 라이선스 판매와 공급자 교체 가능성을 고려한 권고일 뿐이며, 실제 연동 승인이나 유료 권리 부여를 의미하지 않습니다.

1. Paddle을 1순위로 검토합니다. 소프트웨어 사업자가 판매자 대행(Merchant of Record)과 세금·청구 경계를 함께 검토하기 쉽고, 공식 가격 페이지는 월 정액 없이 거래 기반 요금 구조를 안내합니다. [Paddle 공식 가격](https://www.paddle.com/pricing)
2. Stripe를 2순위로 검토합니다. API와 결제 수단 확장성이 좋고, 공식 가격 페이지는 설정비·월 사용료 없이 거래별 요금 구조를 안내합니다. [Stripe 공식 가격](https://stripe.com/pricing)
3. PayPal을 대안으로 검토합니다. 고객이 익숙한 결제 수단을 제공할 수 있고, 공식 Checkout 안내는 월 정액·설정비 없이 거래 기반 요금 구조를 안내합니다. [PayPal 공식 Checkout](https://www.paypal.com/us/business/accept-payments/checkout)

완전 무료로 실서비스 결제·세금·환불·분쟁 처리를 제공하는 공급자는 전제하지 않습니다. 결제 공급자를 선정하기 전에는 상품 가격, 결제 호출, Webhook, 환불, 유료 entitlement를 구현하지 않으며 결제 성공을 라이선스 발급 성공으로 변환하지 않습니다.

현재 코드에는 `IPaymentProvider`와 `PaymentProviderGateway`라는 공급자 중립 경계가 있으며, 공급자가 없으면 `commercial.payment_provider_not_selected`로 fail-closed 처리합니다. 상품·결제 세션·Webhook·환불 결과의 세부 계약과 공급자별 어댑터는 공급자 선정 후 연결합니다. Webhook 서명, 중복 이벤트, 재처리, 환불에 따른 권리 회수와 감사 기록은 공급자 선정 후 별도 승인 기준으로 검증합니다.

