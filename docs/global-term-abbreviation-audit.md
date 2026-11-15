# EDAN 전역 대표 약어 통합 검증 결과

- 기준 원본 전자사전: 붙여넣은 마크다운(2)(1).md
- ERP 확장 사전: enterprise-business-term-dictionary.md 10.2~10.13
- 기존 추가 후보: 26개
- 검증일: 2026-11-15
- 상태: 표준 버전 1.0 확정 / 원본 통합 및 전역 약어 정적 검증 완료

## 1. 집계

| 항목 | 건수 |
|---|---:|
| 기존 원본 데이터 행 | 116 |
| 기존 정규화 항목 | 110 |
| 기존 항목 중 독립 대표 개념 | 103 |
| 기존 복합 컬럼 조합 규칙(대표 개념에서 제외) | 7 |
| 확장 통합 원장 행 | 362 |
| 기존 대표 개념과 통합한 확장 행 | 20 |
| 확장으로 추가된 독립 대표 개념 | 342 |
| **최종 독립 대표 개념** | **445** |
| 최종 3자 대표 약어 | 378 |
| 최종 4자 대표 약어 | 67 |
| 대표 약어 미배정 | 0 |
| 대표 약어 중복 | 0 |
| 3자·4자 형식 위반 | 0 |

## 2. 기존 전자사전 중복 통합

| 영문명 | 대표 약어 | 처리 |
|---|---|---|
| Archive | arc | 동일 개념 통합 |
| Action | act | 동일 개념 통합 |
| Effect | eff | 동일 개념 통합 |
| Priority | pri | 동일 개념 통합 |
| Role | rol | role은 레거시 표기 |
| Enabled | ena | 동일 개념 통합 |

## 3. 의미 충돌 분리

| 기존 개념 | 확장 개념 | 약어 | 분리 사유 |
|---|---|---|---|
| External Delivery | Goods Delivery | dlv / dly | 외부 데이터 전달과 물품 납품 |
| Execution Schedule | Work Schedule | schd / wsc | 예약 실행 시각과 작업 일정 |
| System Operation | Manufacturing Operation | opr / mop | 시스템 동작과 제조 공정작업 |
| Authorization Resource | Production Resource | rsrc / prr | 접근통제 대상과 생산 투입 자원 |
| Authorization Audit | Compliance Audit | aud / adq | 권한 감사 기록과 규정 준수 심사 |
| Execution Failure | Equipment Failure | fail / efl | 처리 실패와 설비 고장 |
| Worker Task | Business Task | tsk / btk | 비동기 작업 요청과 업무 작업 |
| Access Role | Organizational Role | rol / hrl | IAM 권한 역할과 조직상 역할 |
| Used State | Used Quantity | use / usq | 사용 여부와 사용량 |
| Unix Epoch | Unix | uep | 운영체제명과 시간 기준 개념 분리 |

## 4. 기존 길이 규칙 위반의 교정

| 기존 용어 | 기존 약어 | 권장 처리 |
|---|---|---|
| Identifier | id | idr, id는 레거시 |
| Code | cd | cod, cd는 레거시 |
| Name | nm | nam, nm는 레거시 |
| Operation | op | opr, op는 레거시 |
| DateTime | dt | dtt, dt는 레거시 |
| Yes/No | yn | ynf, yn은 레거시 |
| Hour | hr | hou, hr은 레거시 또는 단위 표기 |
| Fencing | fence | fcn |
| Lease | lease | lse |
| Progress | prgrs | prg |
| Creator / Updater | crtmer / updmer | crt_by / upd_by 조합 |
| Created / Updated DateTime | crtdt / upddt | crt_dtt / upd_dtt 조합 |
| Use / Delete Yes/No | useyn / delyn | use_ynf / del_ynf 조합 |
| Sort Number | sortnum | srt_num 조합 |
| Millisecond | ms | msec(4자); ms는 SI 단위 기호로 별도 관리 |

## 5. 후보 전역 중복 및 형식 검사

기존 전자사전 110개 정규화 항목 중 7개 복합 컬럼 조합 규칙을 제외한 103개 대표 개념과, 확장 원장 362행 중 기존 개념 통합 20행을 제외한 342개 신규·분리 개념을 결합했다. 그 결과 독립 대표 개념은 445개다.
최종 445개 대표 개념을 대상으로 전역 약어 중복, 미배정, 3자·4자 형식 검사를 수행했다. 약어 중복 0건, 미배정 0건, 형식 위반 0건이다. 7개 복합 컬럼 패턴은 독립 대표 개념이 아니므로 별도 조합 규칙으로 관리한다. 상세 확정 목록은 [전역 업무 용어·약어 확정 사전](global-business-term-dictionary.md)과 [전체 CSV](global-business-term-dictionary.csv)를 기준으로 한다.

## 6. 확장 대표 용어 및 약어 통합 원장(원본 판정 이력)

아래 표는 확장 원장 362행의 통합·분리 판정 이력이다. 표의 '신규 후보 · 약어 배정' 표시는 작성 당시 후보 상태를 보존한 것이다. 현재 확정 상태와 최종 대표 약어는 [전역 업무 용어·약어 확정 사전](global-business-term-dictionary.md) 및 CSV를 기준으로 한다. 기존 개념 통합 행은 최종 대표 개념 수에 중복 합산하지 않는다.

| 영문 풀네임 | 권장 약어 후보 | 한글명 | 소유 업무 영역 | 정의 | 판정 |
|---|---|---|---|---|---|
| Legal Entity | len | 법인 | COM, ERP-FIN, COML | 법률상 권리·의무의 주체. 조직·사업장과 구분 | 신규 후보 · 약어 배정 |
| Organization | org | 조직 | COM, ERP-HR, IAM | 업무 책임과 권한을 갖는 조직 단위. 법인과 동일하지 않음 | 기존 개념 통합 |
| Business Unit | bun | 사업부 | COM, ERP-FIN | 손익·운영 책임을 갖는 경영 단위 | 신규 후보 · 약어 배정 |
| Department | dep | 부서 | COM, ERP-HR | 조직 내 하위 책임 단위. 팀과 부서는 조직 정책에 따라 계층 구분 | 신규 후보 · 약어 배정 |
| Site | sit | 사업장 | COM, ERP, MES, EAM | 운영이 수행되는 물리·관리 거점. Plant와 동의어로 자동 통합 금지 | 신규 후보 · 약어 배정 |
| Plant | pla | 공장 | ERP, MES, MOM, QMS, EAM | 생산계획·재고·원가 책임이 부여된 제조 운영 단위 | 신규 후보 · 약어 배정 |
| Branch | bra | 지점 | COM, ERP-SAL, ERP-FIN | 법인·조직의 지역 운영 단위 | 신규 후보 · 약어 배정 |
| Work Location | wlo | 근무장소 | ERP-HR, MES, EAM | 사람이 근무하거나 작업을 수행하는 장소. 창고 Location과 구분 | 신규 후보 · 약어 배정 |
| Business Partner | bpa | 거래처 | COM, ERP, CRM, SRM | 고객·공급업체 등 거래 관계의 공통 상위 개념 | 신규 후보 · 약어 배정 |
| Customer | cus | 고객 | ERP-SAL, CRM, COML | 제품·서비스를 구매하거나 제공받는 거래 주체 | 신규 후보 · 약어 배정 |
| Supplier | sup | 공급업체 | ERP-PUR, SRM, SCM | 자재·제품·서비스를 공급하는 거래 주체 | 신규 후보 · 약어 배정 |
| Carrier | car | 운송업체 | TMS, ERP-SAL, ERP-PUR | 화물 운송 서비스를 제공하는 주체. Supplier의 하위 유형일 수 있음 / 운송 수행 주체 | 신규 후보 · 약어 배정 |
| Contact | con | 담당자 | COM, CRM, SRM | 거래처·조직과 연결되는 개인 연락 대상. User 계정과 구분 | 신규 후보 · 약어 배정 |
| Employee | emp | 사원 | ERP-HR, MES, MOM | 고용 관계에 속한 인적 자원 | 신규 후보 · 약어 배정 |
| Person | per | 개인 | COM, IAM, CRM | 고용 여부와 무관한 개인 주체 | 신규 후보 · 약어 배정 |
| User | usr | 사용자 | IAM, COM | 시스템 인증·사용 계정. Employee·Person과 동일하지 않음 | 기존 개념 통합 |
| Address | add | 주소 | COM, ERP, TMS | 우편·물류·법적 주소 정보. Address Type으로 용도 구분 | 신규 후보 · 약어 배정 |
| Country | cou | 국가 | COM, ERP | 국가·지역 기준정보 | 신규 후보 · 약어 배정 |
| Currency | cur | 통화 | ERP-FIN, COML | 금액의 통화 단위. 환율과 분리 | 신규 후보 · 약어 배정 |
| Exchange Rate | era | 환율 | ERP-FIN, COML | 기준 통화 간 교환 비율과 유효기간 | 신규 후보 · 약어 배정 |
| Unit of Measure | uom | 측정단위 | ERP, MES, QMS, TAG | 수량·측정값의 단위. 포장단위·재고단위·기준단위는 용도 관계로 관리 | 신규 후보 · 약어 배정 |
| Calendar | cal | 달력 | COM, ERP, MES | 날짜·휴일·근무일 계산 기준 | 신규 후보 · 약어 배정 |
| Shift | shi | 교대조 | ERP-HR, MES, MOM | 시간대별 인력·설비 운영조 | 신규 후보 · 약어 배정 |
| Holiday | hol | 휴일 | COM, ERP-HR, MES | 업무·생산 달력에서 비근무로 처리되는 날짜 | 신규 후보 · 약어 배정 |
| Common Code System | ccs | 공통코드체계 | COM, 모든 모듈 | 코드의 정의·범위·버전·소유권을 관리하는 체계 | 신규 후보 · 약어 배정 |
| Code Group | cgr | 코드그룹 | COM, 모든 모듈 | 관련 코드값의 논리적 묶음 | 신규 후보 · 약어 배정 |
| Code Value | cva | 코드값 | COM, 모든 모듈 | 코드그룹에 속하는 개별 값 | 신규 후보 · 약어 배정 |
| Classification | cla | 분류 | COM, ERP, PLM, QMS | 객체를 기준에 따라 묶는 분류. Type과 구분 | 신규 후보 · 약어 배정 |
| Tax | tax | 세금 | ERP-FIN, COML | 과세 규칙·세목·세율 및 세금 계산의 기준 | 신규 후보 · 약어 배정 |
| Tax Rate | tra | 세율 | ERP-FIN, COML | 과세표준에 적용하는 비율 | 신규 후보 · 약어 배정 |
| Payment Term | pte | 결제조건 | ERP-SAL, ERP-PUR, COML | 결제기한·분할·조건을 정의 | 신규 후보 · 약어 배정 |
| Incoterms | inc | 무역인도조건 | ERP-SAL, ERP-PUR, SCM | 국제 거래에서 비용·위험·인도 책임을 정의 | 신규 후보 · 약어 배정 |
| Lead | lea | 영업잠재고객 | CRM, ERP-SAL | 아직 거래기회로 확정되지 않은 잠재 수요 주체 | 신규 후보 · 약어 배정 |
| Opportunity | opp | 영업기회 | CRM, ERP-SAL | 예상 매출 가능성이 있는 영업 활동 단위 | 신규 후보 · 약어 배정 |
| Quotation | quo | 견적 | ERP-SAL, COML | 가격·조건을 제안하는 문서. 견적요청과 구분 | 신규 후보 · 약어 배정 |
| Request for Quotation | rfq | 견적요청 | ERP-PUR, ERP-SAL | 공급자 또는 판매자에게 견적을 요청하는 행위·문서 | 신규 후보 · 약어 배정 |
| Sales Order | sor | 수주 | ERP-SAL, COML | 고객의 구매 의사를 접수하고 판매 이행 대상으로 확정한 거래 | 신규 후보 · 약어 배정 |
| Sales Order Line | sol | 수주항목 | ERP-SAL, COML | 수주 문서 내 품목·수량·가격 단위 | 신규 후보 · 약어 배정 |
| Contract | cont | 계약 | ERP-SAL, ERP-PUR, COML | 당사자 간 법적·상업적 의무와 조건 | 신규 후보 · 약어 배정 |
| Contract Line | cli | 계약항목 | ERP-SAL, ERP-PUR, COML | 계약 내 개별 품목·서비스·조건 단위 | 신규 후보 · 약어 배정 |
| Sales Channel | sch | 판매채널 | ERP-SAL, CRM | 주문이 유입되거나 판매가 수행되는 경로 | 신규 후보 · 약어 배정 |
| Price List | pli | 가격표 | ERP-SAL, ERP-PUR | 품목·고객·기간별 가격 정책 | 신규 후보 · 약어 배정 |
| Price | pric | 가격 | ERP-SAL, ERP-PUR | 특정 기준 단위의 거래 가격. 금액·원가와 구분 | 신규 후보 · 약어 배정 |
| Discount | dis | 할인 | ERP-SAL, COML | 기준 가격에서 차감하는 조건·금액 | 신규 후보 · 약어 배정 |
| Surcharge | sur | 할증 | ERP-SAL, ERP-PUR | 기준 가격에 추가하는 금액 | 신규 후보 · 약어 배정 |
| Sales | sal | 판매 | ERP-SAL | 상품·서비스를 고객에게 판매하는 활동·거래. Revenue와 구분 | 신규 후보 · 약어 배정 |
| Revenue | reve | 매출 | ERP-FIN, ERP-SAL, COML | 회계기준에 따라 인식한 수익 금액. 수주금액·입금액과 구분 | 신규 후보 · 약어 배정 |
| Billing | bil | 청구 | ERP-FIN, COML | 고객에게 지급 의무를 통지·생성하는 절차 | 신규 후보 · 약어 배정 |
| Invoice | inv | 송장 | ERP-FIN, ERP-SAL, COML | 거래·청구 내역을 증빙하는 문서. Payment와 구분 | 신규 후보 · 약어 배정 |
| Accounts Receivable | are | 매출채권 | ERP-FIN | 고객에게 받을 권리로 인식된 금액 | 신규 후보 · 약어 배정 |
| Receipt | rece | 수금 | ERP-FIN, COML | 고객으로부터 대금을 회수하는 거래. Goods Receipt와 구분 | 신규 후보 · 약어 배정 |
| Refund | refu | 환불 | ERP-FIN, COML | 기존 수납·결제의 전부 또는 일부를 되돌리는 거래 | 신규 후보 · 약어 배정 |
| Return | ret | 반품 | ERP-SAL, ERP-PUR, WMS | 물품을 원거래 상대방에게 되돌리는 물류·상업 거래 | 신규 후보 · 약어 배정 |
| Credit Note | cno | 대변전표 | ERP-FIN, ERP-SAL | 기존 청구·매출 금액을 감액하는 문서 | 신규 후보 · 약어 배정 |
| Debit Note | dno | 차변전표 | ERP-FIN, ERP-PUR | 추가 지급·청구 금액을 통지하는 문서 | 신규 후보 · 약어 배정 |
| Commission | com | 수수료 | ERP-SAL, ERP-FIN | 중개·판매 서비스에 대한 보상 | 신규 후보 · 약어 배정 |
| Sales Forecast | sfo | 판매예측 | ERP-SAL, SCM | 미래 판매량·금액 예측. 확정 수주와 구분 | 신규 후보 · 약어 배정 |
| Purchase Requisition | pre | 구매요청 | ERP-PUR | 내부에서 구매 필요를 요청하는 문서. 발주와 구분 | 신규 후보 · 약어 배정 |
| Purchase Order | por | 발주 | ERP-PUR, SCM | 공급업체에 구매 조건을 확정·전달하는 주문 | 신규 후보 · 약어 배정 |
| Purchase Order Line | pol | 발주항목 | ERP-PUR | 발주 문서 내 개별 품목·수량·납기 | 신규 후보 · 약어 배정 |
| Purchase Agreement | pag | 구매약정 | ERP-PUR, SCM | 기간·조건을 약정한 구매 계약. 개별 발주와 구분 | 신규 후보 · 약어 배정 |
| Supplier Quotation | squ | 공급업체 견적 | ERP-PUR | 공급업체가 제출한 견적 | 신규 후보 · 약어 배정 |
| Procurement | pro | 조달 | ERP-PUR, SCM | 필요한 자원·서비스를 확보하는 전체 업무 | 신규 후보 · 약어 배정 |
| Sourcing | sou | 공급처선정 | ERP-PUR, SCM, SRM | 공급자 탐색·평가·선정 업무 | 신규 후보 · 약어 배정 |
| Supplier Evaluation | sue | 공급업체 평가 | SRM, ERP-PUR, QMS | 납기·품질·가격·위험 기준으로 공급자를 평가 | 신규 후보 · 약어 배정 |
| Purchase Receipt | pur | 구매입고 | ERP-PUR, WMS | 구매 주문에 대한 물품 수령 사건. 재고 반영 정책은 별도 | 신규 후보 · 약어 배정 |
| Goods Receipt | gre | 입고 | ERP-PUR, ERP-INV, WMS | 재고·구매 시스템에서 물품 수령을 기록하는 재고 거래 | 신규 후보 · 약어 배정 |
| Inspection Receipt | ire | 수입검사 접수 | QMS, ERP-PUR | 수령 물품을 검사 대상으로 등록하는 행위 | 신규 후보 · 약어 배정 |
| Subcontracting | subc | 외주가공 | ERP-PUR, ERP-PRD, MES | 외부 업체에 공정·가공을 위탁하는 업무 | 신규 후보 · 약어 배정 |
| Service Procurement | spr | 용역구매 | ERP-PUR | 물품이 아닌 서비스·용역을 조달 | 신규 후보 · 약어 배정 |
| Supplier Invoice | sin | 매입송장 | ERP-FIN, ERP-PUR | 공급업체가 청구한 대금 문서 | 신규 후보 · 약어 배정 |
| Accounts Payable | apa | 매입채무 | ERP-FIN | 공급자에게 지급할 의무로 인식된 금액 | 신규 후보 · 약어 배정 |
| Payment | pay | 지급 | ERP-FIN, COML | 채무·청구에 대해 돈을 지급하는 거래 | 신규 후보 · 약어 배정 |
| Advance Payment | apy | 선급금 | ERP-FIN, ERP-PUR | 재화·서비스 수령 전에 지급한 금액 | 신규 후보 · 약어 배정 |
| Purchase Return | prt | 구매반품 | ERP-PUR, WMS | 구매 물품을 공급업체에 반환하는 거래 | 신규 후보 · 약어 배정 |
| Inventory | inve | 재고 | ERP-INV, WMS, MES | 보유 중인 물품의 수량·상태·가치 | 신규 후보 · 약어 배정 |
| Stock Quantity | stq | 재고수량 | ERP-INV, WMS | 특정 재고 차원의 수량. 재고금액과 구분 | 신규 후보 · 약어 배정 |
| Available Quantity | aqu | 가용수량 | ERP-INV, WMS, MES | 예약·보류·품질상태 등을 제외하고 사용 가능한 수량 | 신규 후보 · 약어 배정 |
| Reserved Quantity | rqa | 예약수량 | ERP-INV, WMS | 특정 주문·작업을 위해 점유한 수량 | 신규 후보 · 약어 배정 |
| Safety Stock | sst | 안전재고 | ERP-INV, ERP-PRD | 수요·공급 변동에 대비해 유지하는 재고 | 신규 후보 · 약어 배정 |
| Reorder Point | rpo | 재주문점 | ERP-INV, ERP-PUR | 보충 주문을 시작하는 재고 기준 | 신규 후보 · 약어 배정 |
| Warehouse | war | 창고 | ERP-INV, WMS | 재고 보관·운영의 관리 단위 | 신규 후보 · 약어 배정 |
| Storage Location | slo | 저장위치 | ERP-INV, WMS | 창고 또는 공장 내 재고관리 구역. 일반 Location과 구분 | 신규 후보 · 약어 배정 |
| Bin Location | blo | 빈 위치 | WMS | 창고 내 최하위 보관 셀·주소 | 신규 후보 · 약어 배정 |
| Putaway | put | 적치 | WMS | 수령 물품을 지정 보관 위치에 배치하는 작업 | 신규 후보 · 약어 배정 |
| Picking | pic | 피킹 | WMS | 출고·생산 투입 대상 물품을 보관 위치에서 꺼내는 작업 | 신규 후보 · 약어 배정 |
| Packing | pac | 포장 | WMS, ERP-SAL | 물품을 운송·보관 가능한 포장 단위로 구성 | 신규 후보 · 약어 배정 |
| Shipment | ship | 출하 | ERP-SAL, WMS, TMS | 출고 물품을 운송 단위로 인계·발송하는 사건 | 신규 후보 · 약어 배정 |
| Goods Delivery | dly | 납품 | ERP-SAL, TMS | 고객·수령지에 물품을 전달하는 이행 사건 | 의미 분리 |
| Goods Issue | gis | 출고 | ERP-INV, WMS, MES | 재고에서 수량을 차감하는 거래. 배송과 구분 | 신규 후보 · 약어 배정 |
| Stock Transfer | stt | 재고이동 | ERP-INV, WMS | 재고의 보관 장소·조직·상태를 이동하는 거래 | 신규 후보 · 약어 배정 |
| Stock Adjustment | sad | 재고조정 | ERP-INV, WMS | 실사·오류·손상 등에 따른 장부 재고 수정 | 신규 후보 · 약어 배정 |
| Stock Count | sco | 재고실사 | ERP-INV, WMS | 실물 수량을 확인하는 조사 활동 | 신규 후보 · 약어 배정 |
| Lot | lot | 로트 | ERP-INV, MES, QMS | 동일 생산·입고 조건으로 추적하는 물품 집합 | 신규 후보 · 약어 배정 |
| Serial Number | snu | 일련번호 | ERP-INV, MES, EAM | 개별 물품·설비를 유일하게 식별하는 번호 | 신규 후보 · 약어 배정 |
| Package | pack | 포장단위 | WMS, ERP-SAL | 물품을 묶은 물리 단위 | 신규 후보 · 약어 배정 |
| Handling Unit | hun | 물류취급단위 | WMS, TMS | 팔레트·박스 등 물류 이동·추적 단위 | 신규 후보 · 약어 배정 |
| Shipment Line | sli | 출하항목 | ERP-SAL, WMS | 출하 문서 내 품목·수량 단위 | 신규 후보 · 약어 배정 |
| Delivery Route | dro | 배송경로 | TMS | 출발지와 목적지를 잇는 운송 경로 | 신규 후보 · 약어 배정 |
| Freight | fre | 운임 | TMS, ERP-FIN | 운송 서비스의 비용 | 신규 후보 · 약어 배정 |
| Proof of Delivery | pod | 배송증명 | TMS, ERP-SAL | 물품이 수령자에게 전달된 증거. POD는 동일 개념의 약칭 | 신규 후보 · 약어 배정 |
| Demand | dem | 수요 | SCM, ERP-PRD | 제품·자재·서비스가 필요로 되는 양 | 신규 후보 · 약어 배정 |
| Demand Plan | dpl | 수요계획 | SCM, ERP-PRD | 미래 수요를 기간·품목별로 계획 | 신규 후보 · 약어 배정 |
| Supply Plan | spl | 공급계획 | SCM, ERP-PRD | 수요 충족을 위한 조달·생산·재고 계획 | 신규 후보 · 약어 배정 |
| Material Requirements Planning | mrp | 자재소요계획 | ERP-PRD, ERP-PUR | 생산·수요를 바탕으로 자재별 필요량·시점을 계산 | 신규 후보 · 약어 배정 |
| Master Production Schedule | mps | 기준생산계획 | ERP-PRD | 완제품·주요 품목의 기간별 생산 계획 | 신규 후보 · 약어 배정 |
| Production Plan | ppl | 생산계획 | ERP-PRD, MOM | 생산량·기간·자원에 대한 계획 | 신규 후보 · 약어 배정 |
| Production Order | pord | 생산오더 | ERP-PRD, MES | 생산할 품목·수량·기간·공장을 지정하는 실행 관리 단위 | 신규 후보 · 약어 배정 |
| Work Order | wor | 작업오더 | MES, EAM | 수행할 작업과 자원·일정을 지시하는 단위 | 신규 후보 · 약어 배정 |
| Routing | rou | 공정경로 | ERP-PRD, MES, PLM | 제품 생산에 필요한 공정의 순서·경로 정의 / 생산 수행 순서와 작업 단계 | 신규 후보 · 약어 배정 |
| Work Center | wce | 작업장 | ERP-PRD, MES, MOM | 생산능력·작업이 배정되는 자원 그룹 / 작업 수행 자원의 논리적 그룹 | 신규 후보 · 약어 배정 |
| Capacity | cpty | 생산능력 | ERP-PRD, MOM | 기간·자원별 처리 가능한 양 | 신규 후보 · 약어 배정 |
| Work Schedule | wsc | 작업일정 | ERP-PRD, MES, MOM | 작업의 시간 배치. 계획과 실제 실행시간 구분 | 기존 Schedule 개념과 통합 |
| Dispatch | disp | 작업배포 | MES, MOM | 실행 순서·자원을 정해 작업을 현장에 배정 | 신규 후보 · 약어 배정 |
| Work in Process | wip | 재공 | ERP-CST, MES, MOM | 생산공정에 투입되어 아직 완제품이 되지 않은 상태·수량. WIP는 동일 개념의 업계 약칭 | 기존 WIP 별칭 통합 |
| Production Output | pou | 생산산출 | MES, ERP-CST | 생산 작업으로 산출된 수량 | 신규 후보 · 약어 배정 |
| Material Consumption | mco | 자재소모 | ERP-PRD, MES | 생산에 실제 투입·소모된 자재량 | 신규 후보 · 약어 배정 |
| Scrap | scr | 폐기 | MES, QMS, ERP-CST | 재사용·정상품 처리할 수 없어 폐기하는 수량·금액 | 신규 후보 · 약어 배정 |
| Rework | rew | 재작업 | MES, QMS | 부적합 또는 미완료 결과를 다시 처리하는 작업 | 신규 후보 · 약어 배정 |
| Yield | yie | 수율 | MES, QMS | 투입 대비 산출 비율. 합격률과 계산 정의 구분 | 신규 후보 · 약어 배정 |
| Cost Center | cce | 원가센터 | ERP-CST, ERP-FIN | 비용·원가 책임을 집계하는 조직 단위 | 신규 후보 · 약어 배정 |
| Cost Element | cel | 원가요소 | ERP-CST, ERP-FIN | 원가를 구성하는 분류 항목 | 신규 후보 · 약어 배정 |
| Standard Cost | stc | 표준원가 | ERP-CST | 사전에 설정한 기준 원가 | 신규 후보 · 약어 배정 |
| Actual Cost | aco | 실제원가 | ERP-CST | 실제 발생 자료로 계산한 원가 | 신규 후보 · 약어 배정 |
| Cost Allocation | coa | 원가배부 | ERP-CST | 공통 원가를 대상 원가 객체에 배분 | 신규 후보 · 약어 배정 |
| General Ledger | gle | 총계정원장 | ERP-FIN | 회계 계정별 거래·잔액의 공식 기록 | 신규 후보 · 약어 배정 |
| Journal Entry | jen | 회계전표 | ERP-FIN | 회계 거래를 차변·대변으로 기록하는 단위 | 신규 후보 · 약어 배정 |
| Account | acc | 계정 | ERP-FIN | 회계 분류·집계 기준 | 신규 후보 · 약어 배정 |
| Fiscal Period | fpe | 회계기간 | ERP-FIN | 회계보고·결산을 위한 기간 | 신규 후보 · 약어 배정 |
| Budget | bud | 예산 | ERP-FIN | 승인된 수입·지출·자원 한도 | 신규 후보 · 약어 배정 |
| Payroll | payr | 급여 | ERP-HR | 직원 보상액 산정·지급 업무 | 신규 후보 · 약어 배정 |
| Attendance | att | 근태 | ERP-HR | 출근·퇴근·근무시간 기록 | 신규 후보 · 약어 배정 |
| Qualification | qua | 자격 | ERP-HR, MES, QMS | 특정 업무 수행에 필요한 자격·승인 상태 | 신규 후보 · 약어 배정 |
| Manufacturing Order | mor | 제조오더 | MES, MOM | 제조 실행의 상위 오더. ERP Production Order와의 연계키·소유권 정의 필요 | 신규 후보 · 약어 배정 |
| Manufacturing Operation | mop | 공정작업 | MES, MOM | 라우팅 내 개별 수행 단계. Process와 구분 | 의미 분리 |
| Process | proc | 공정 | MES, MOM, QMS | 투입물을 산출물로 변환하는 논리적 제조 단계 | 신규 후보 · 약어 배정 |
| Process Segment | pse | 공정구간 | MES, MOM | 공정 내 더 세밀한 실행·추적 구간 | 신규 후보 · 약어 배정 |
| Work Unit | wun | 작업단위 | MES, MOM | 현장에서 실행·기록하는 최소 업무 단위 | 신규 후보 · 약어 배정 |
| Job | job | 작업건 | MES, MOM, EDAN | 실행 인스턴스. Work Order·Task와 별도 | 기존 26개 후보 통합 |
| Work Request | wre | 작업요청 | MES, EAM | 작업 수행을 요청하는 문서·사건. Work Order와 구분 | 신규 후보 · 약어 배정 |
| Dispatch List | dli | 작업배포목록 | MES, MOM | 작업장·자원에 배정할 작업의 순서 목록 | 신규 후보 · 약어 배정 |
| Production Run | pru | 생산실행 | MES, MOM | 특정 생산오더·조건으로 수행한 생산 실행 단위 | 신규 후보 · 약어 배정 |
| Production Lot | plo | 생산로트 | MES, QMS | 동일 생산 조건으로 추적하는 산출물 집합 | 신규 후보 · 약어 배정 |
| Production Line | prl | 생산라인 | MES, MOM | 공정·설비·작업장을 연결한 생산 흐름 | 신규 후보 · 약어 배정 |
| Cell | cell | 제조셀 | MES, MOM | 특정 생산 목적을 위해 구성한 작업·설비 단위 | 신규 후보 · 약어 배정 |
| Production Resource | prr | 생산자원 | MES, MOM | 인력·설비·도구·자재 등 실행에 필요한 자원 | 의미 분리 |
| Personnel | pers | 작업인력 | MES, MOM | 생산작업을 수행하는 사람. Employee와 역할·시점 구분 | 신규 후보 · 약어 배정 |
| Tool | too | 공구 | MES, EAM | 제조·검사 작업에 사용하는 도구 | 신규 후보 · 약어 배정 |
| Fixture | fix | 치구 | MES, EAM | 가공·조립 대상물을 고정·위치시키는 장치 | 신규 후보 · 약어 배정 |
| Recipe | reci | 제조레시피 | MES, MOM | 제품·공정별 제조 파라미터와 절차 | 신규 후보 · 약어 배정 |
| Parameter | par | 공정변수 | MES, MOM, TAG | 공정·설비 동작을 결정하는 설정·측정 변수 | 신규 후보 · 약어 배정 |
| Setpoint | set | 설정값 | MES, MOM, TAG | 제어 시스템에 지정한 목표 값. 측정값과 구분 / 제어 목표로 설정한 값 | 신규 후보 · 약어 배정 |
| Actual Value | ava | 실측값 | MES, MOM, TAG | 실행 중 관측된 값 | 신규 후보 · 약어 배정 |
| Downtime | dow | 비가동시간 | MES, MOM, EAM | 생산 가능한 시간이었으나 가동하지 못한 시간 / 설비가 가동하지 못한 시간. 고장 외 계획정지도 존재 | 신규 후보 · 약어 배정 |
| Uptime | upt | 가동시간 | MES, MOM, EAM | 설비·라인이 가동 상태였던 시간 | 신규 후보 · 약어 배정 |
| Overall Equipment Effectiveness | oee | 설비종합효율 | MES, MOM, EAM | 가용성·성능·품질 요소를 결합한 지표 | 신규 후보 · 약어 배정 |
| Andon | and | 안돈 | MES, MOM | 현장 이상·지원 요청·상태를 시각적으로 알리는 체계 | 신규 후보 · 약어 배정 |
| Traceability | trac | 추적성 | MES, QMS, PLM | 원자재부터 생산·검사·출하까지 관계를 역·정방향으로 추적하는 능력 | 신규 후보 · 약어 배정 |
| Genealogy | gen | 제품계보 | MES, QMS, PLM | 투입 로트·부품과 산출 제품 간 계보 관계 | 신규 후보 · 약어 배정 |
| Nonconformance | non | 부적합 | MES, QMS | 요구사항을 충족하지 못한 상태·사건 / 명시된 요구사항을 충족하지 못한 상태 | 신규 후보 · 약어 배정 |
| Production Confirmation | pco | 생산확정 | MES, ERP-PRD | 생산 수량·시간·자원 사용을 확정 기록하는 행위 | 신규 후보 · 약어 배정 |
| Quality | qual | 품질 | QMS, MES | 제품·공정이 요구사항을 만족하는 정도 | 신규 후보 · 약어 배정 |
| Quality Plan | qpl | 품질계획 | QMS | 품질 확보를 위한 검사·관리 활동 계획 | 신규 후보 · 약어 배정 |
| Inspection Plan | ipl | 검사계획 | QMS | 검사 항목·방법·표본·빈도 정의 | 신규 후보 · 약어 배정 |
| Inspection Characteristic | ich | 검사특성 | QMS | 검사 대상이 되는 측정·판정 항목 | 신규 후보 · 약어 배정 |
| Inspection Method | ime | 검사방법 | QMS | 검사 수행 절차·기술 | 신규 후보 · 약어 배정 |
| Inspection | ins | 검사 | QMS, ERP-PUR, MES | 요구사항에 대한 적합성 확인 활동 | 신규 후보 · 약어 배정 |
| Inspection Lot | ilo | 검사로트 | QMS | 하나의 검사 판정 단위로 묶은 대상 | 신규 후보 · 약어 배정 |
| Sample | sam | 시료 | QMS | 검사·시험을 위해 추출한 대상 | 신규 후보 · 약어 배정 |
| Sample Size | ssi | 표본수 | QMS | 검사에 사용하는 시료의 개수 | 신규 후보 · 약어 배정 |
| Measurement Result | mre | 측정결과 | QMS | 검사에서 얻은 측정 데이터 | 신규 후보 · 약어 배정 |
| Inspection Result | inr | 검사결과 | QMS | 검사 결과와 적합·부적합 판정 | 신규 후보 · 약어 배정 |
| Acceptance Criteria | acr | 합격기준 | QMS | 합격 여부를 결정하는 기준 | 신규 후보 · 약어 배정 |
| Specification | spe | 규격 | QMS, PLM | 제품·공정이 만족해야 할 요구 조건 / 제품·부품의 기술 요구사항 | 신규 후보 · 약어 배정 |
| Tolerance | tol | 공차 | QMS, PLM | 기준값에 허용되는 편차 | 신규 후보 · 약어 배정 |
| Defect | defe | 결함 | QMS, MES | 제품·공정에서 발견된 결함 현상 | 신규 후보 · 약어 배정 |
| Defect Type | dty | 결함유형 | QMS | 결함을 유형별로 분류한 기준 | 신규 후보 · 약어 배정 |
| Nonconformance Report | nre | 부적합보고서 | QMS | 부적합 사실·영향·조치를 기록하는 문서 | 신규 후보 · 약어 배정 |
| Corrective Action | cac | 시정조치 | QMS | 발견된 부적합 원인을 제거하는 조치 | 신규 후보 · 약어 배정 |
| Preventive Action | pra | 예방조치 | QMS | 잠재 부적합 발생 가능성을 제거하는 조치 | 신규 후보 · 약어 배정 |
| Root Cause | rca | 근본원인 | QMS, EAM | 문제를 발생시킨 근본 원인 | 신규 후보 · 약어 배정 |
| Cause Analysis | can | 원인분석 | QMS, EAM | 현상·데이터로 원인을 규명하는 과정 | 신규 후보 · 약어 배정 |
| Corrective and Preventive Action | capa | 시정·예방조치 | QMS | 시정조치와 예방조치를 묶어 관리하는 프로세스 | 신규 후보 · 약어 배정 |
| Compliance Audit | adq | 심사 | QMS, IAM, ERP | 기준·요구사항 준수 여부를 독립적으로 확인 | 의미 분리 |
| Calibration | cali | 교정 | QMS, EAM, TAG | 측정기 표시값과 표준값의 관계를 확인·조정 / 계측 결과와 표준값의 관계를 확인·조정 | 신규 후보 · 약어 배정 |
| Gauge | gau | 게이지 | QMS, EAM | 측정에 사용하는 기기. Sensor와 구분 | 신규 후보 · 약어 배정 |
| Certificate | cer | 인증서 | QMS, PLM | 적합성·검사·자격을 증명하는 문서 | 신규 후보 · 약어 배정 |
| Quality Certificate | qce | 품질증명서 | QMS | 제품·로트의 품질 결과를 증명하는 문서 | 신규 후보 · 약어 배정 |
| Deviation | dev | 일탈 | QMS, MES | 승인된 기준·절차에서 벗어난 사건·상태 | 신규 후보 · 약어 배정 |
| Waiver | wai | 특채승인 | QMS, PLM | 부적합을 조건부로 사용·승인하는 결정 | 신규 후보 · 약어 배정 |
| Complaint | comp | 고객불만 | QMS, CRM | 고객이 제기한 품질·서비스 불만 | 신규 후보 · 약어 배정 |
| Supplier Quality | suq | 공급업체품질 | QMS, SRM | 공급업체가 제공하는 제품·서비스의 품질 수준 | 신규 후보 · 약어 배정 |
| Product | prod | 제품 | PLM, ERP, MES | 설계·판매·생산의 대상이 되는 제품 정의 | 신규 후보 · 약어 배정 |
| Product Family | pfa | 제품군 | PLM, ERP-SAL | 공통 특성을 가진 제품의 묶음 | 신규 후보 · 약어 배정 |
| Product Model | pmo | 제품모델 | PLM, ERP-SAL | 제품의 모델·구성 변형 기준 | 신규 후보 · 약어 배정 |
| Item | ite | 품목 | PLM, ERP | 구매·생산·재고·판매에서 관리하는 카탈로그 항목 | 신규 후보 · 약어 배정 |
| Part | part | 부품 | PLM, ERP-PRD | 제품을 구성하거나 조립되는 개별 구성 요소 | 신규 후보 · 약어 배정 |
| Component | cot | 구성요소 | PLM, MES | 상위 제품·구조에 포함되는 요소 | 신규 후보 · 약어 배정 |
| Bill of Materials | bom | 자재명세서 | PLM, ERP-PRD, MES | 상위 품목과 하위 구성품·수량·유효성을 표현하는 구조. BOM은 동일 개념의 업계 약칭 | 신규 후보 · 약어 배정 |
| BOM Line | bli | BOM 항목 | PLM, ERP-PRD | BOM 내 하위 품목·수량·위치 관계 | 신규 후보 · 약어 배정 |
| Engineering BOM | ebo | 설계BOM | PLM | 설계 관점 제품 구조 | 신규 후보 · 약어 배정 |
| Manufacturing BOM | mbo | 제조BOM | PLM, ERP-PRD, MES | 제조·조립 관점 제품 구조 | 신규 후보 · 약어 배정 |
| Drawing | dra | 도면 | PLM, DOC | 제품·부품의 형상·치수·요구사항을 표현한 문서 | 신규 후보 · 약어 배정 |
| Engineering Change Request | ecr | 설계변경요청 | PLM | 설계 변경 필요성을 요청하는 기록 | 신규 후보 · 약어 배정 |
| Engineering Change Order | eco | 설계변경명령 | PLM | 승인된 설계 변경을 실행하도록 지시 | 신규 후보 · 약어 배정 |
| Engineering Change Notice | ecn | 설계변경통보 | PLM, ERP, MES | 승인된 변경 내용을 관련 조직·시스템에 통보 | 신규 후보 · 약어 배정 |
| Revision | rev | 개정 | PLM, QMS, DOC | 승인된 설계·문서 내용의 개정 차수 / 승인된 내용 변경 차수 | 기존 개념 통합 |
| Version | ver | 버전 | PLM, DOC, EDAN | 파일·소프트웨어·데이터의 배포·기술 버전 / 배포·기술 호환 단위 | 기존 개념 통합 |
| Effectivity | effe | 적용유효성 | PLM, ERP, MES | 개정·BOM·공정이 어느 기간·로트·일련번호부터 적용되는지 정의 | 신규 후보 · 약어 배정 |
| Lifecycle State | lis | 수명주기상태 | PLM, DOC | 작성·검토·승인·배포·폐기 단계 | 신규 후보 · 약어 배정 |
| Document | doc | 문서 | PLM, DOC, QMS | 관리 대상 콘텐츠와 메타데이터 / 내용과 메타데이터를 관리하는 기록 객체 | 신규 후보 · 약어 배정 |
| CAD Model | cmo | CAD 모델 | PLM | 제품 형상·설계 데이터를 표현하는 모델 | 신규 후보 · 약어 배정 |
| Change Notice | chn | 변경통지 | PLM, DOC | 변경 사실을 전달하는 통지. 변경명령과 구분 | 신규 후보 · 약어 배정 |
| Asset | ass | 자산 | EAM, ERP-FIN | 소유·가치·감가상각·책임 관점의 자산 | 신규 후보 · 약어 배정 |
| Equipment | equ | 설비 | EAM, MES, MOM | 기능·운영·정비 관점에서 관리하는 설비 개체 | 신규 후보 · 약어 배정 |
| Functional Location | flo | 기능위치 | EAM | 설비가 수행하는 기능을 기준으로 구성한 위치 계층 | 신규 후보 · 약어 배정 |
| Technical Object | tob | 기술객체 | EAM | 설비·기능위치 등 기술적으로 관리하는 대상의 상위 개념 | 신규 후보 · 약어 배정 |
| Equipment Category | eca | 설비분류 | EAM | 설비를 기능·종류·관리기준으로 분류 | 신규 후보 · 약어 배정 |
| Equipment Class | ecl | 설비클래스 | EAM | 공통 속성·특성 집합을 정의하는 분류 | 신규 후보 · 약어 배정 |
| Equipment Hierarchy | ehi | 설비계층 | EAM, MOM | 공장·라인·설비·부품의 상하위 관계 | 신규 후보 · 약어 배정 |
| Installation | inst | 설치 | EAM | 설비·부품을 특정 기능위치에 장착하는 행위 | 신규 후보 · 약어 배정 |
| Removal | rem | 탈거 | EAM | 설비·부품을 설치 위치에서 제거하는 행위 | 신규 후보 · 약어 배정 |
| Maintenance | mai | 보전 | EAM | 설비 기능을 유지·복구하는 전체 업무 | 신규 후보 · 약어 배정 |
| Preventive Maintenance | pma | 예방보전 | EAM | 고장 전에 주기·상태 기준으로 수행하는 보전 | 신규 후보 · 약어 배정 |
| Corrective Maintenance | cma | 사후보전 | EAM | 발생한 고장을 복구하는 보전 | 신규 후보 · 약어 배정 |
| Predictive Maintenance | prm | 예지보전 | EAM, IIOT, MOM | 상태 데이터로 고장 가능성을 예측해 수행하는 보전 | 신규 후보 · 약어 배정 |
| Condition-based Maintenance | cbm | 상태기반보전 | EAM, IIOT | 설비 상태 기준에 따라 수행하는 보전 | 신규 후보 · 약어 배정 |
| Maintenance Plan | mpl | 보전계획 | EAM | 반복·예정 보전 활동의 기준 계획 | 신규 후보 · 약어 배정 |
| Maintenance Schedule | msc | 보전일정 | EAM | 보전 작업의 실행 예정 시점 | 신규 후보 · 약어 배정 |
| Maintenance Order | mao | 정비오더 | EAM | 정비 자원·작업·비용·일정을 관리하는 실행 단위 | 신규 후보 · 약어 배정 |
| Maintenance Request | mar | 정비요청 | EAM | 정비 필요를 요청하는 기록. 정비오더와 구분 | 신규 후보 · 약어 배정 |
| Work Permit | wpe | 작업허가 | EAM, MOM | 위험 작업 수행 전 안전 조건을 승인하는 절차·문서 | 신규 후보 · 약어 배정 |
| Equipment Failure | efl | 고장 | EAM, QMS | 설비가 요구 기능을 수행하지 못하는 상태 | 의미 분리 |
| Failure Mode | fmo | 고장형태 | EAM | 고장이 나타나는 구체적 방식 | 신규 후보 · 약어 배정 |
| Failure Cause | fca | 고장원인 | EAM | 고장을 발생시킨 원인 | 신규 후보 · 약어 배정 |
| Failure Effect | fef | 고장영향 | EAM | 고장이 설비·생산·안전에 미친 결과 | 신규 후보 · 약어 배정 |
| Breakdown | bre | 돌발고장 | EAM, MES | 계획되지 않은 기능 상실 사건 | 신규 후보 · 약어 배정 |
| Maintenance History | mhi | 보전이력 | EAM | 설비에 수행된 점검·정비·교체 기록 | 신규 후보 · 약어 배정 |
| Spare Part | spa | 예비품 | EAM, ERP-INV | 설비 정비·교체를 위해 보유하는 부품 | 신규 후보 · 약어 배정 |
| Lubrication | lub | 윤활 | EAM | 마찰·마모 방지를 위한 윤활 작업·상태 | 신규 후보 · 약어 배정 |
| Inspection Route | iro | 점검경로 | EAM | 여러 설비·점검지점을 순서대로 방문하는 경로 | 신규 후보 · 약어 배정 |
| Meter Reading | mer | 계량기검침 | EAM, IIOT | 누적 계량기에서 읽은 값. 순간 측정값과 구분 | 신규 후보 · 약어 배정 |
| Warranty | warr | 보증 | EAM, ERP-PUR, ERP-SAL | 제품·설비의 보증 책임·기간·조건 | 신규 후보 · 약어 배정 |
| Risk Priority Number | rpn | 위험우선순위수 | EAM, QMS | 고장·위험 분석에서 심각도·발생도·검출도 등을 결합한 점수 | 신규 후보 · 약어 배정 |
| Data Source | dso | 데이터원천 | IIOT, EDAN | 데이터를 생성·제공하는 장치·시스템 | 신규 후보 · 약어 배정 |
| Device | devi | 장치 | IIOT, TAG | 데이터를 생성·전송·수신하는 물리·논리 장치 | 신규 후보 · 약어 배정 |
| Sensor | sen | 센서 | IIOT, TAG, EAM | 물리량·상태를 감지하는 장치 | 신규 후보 · 약어 배정 |
| Actuator | actu | 액추에이터 | IIOT, TAG, EAM | 제어 신호에 따라 물리 동작을 수행하는 장치 | 신규 후보 · 약어 배정 |
| Controller | cor | 제어기 | IIOT, MES, MOM | 입력·로직에 따라 장치·공정을 제어하는 시스템 | 신규 후보 · 약어 배정 |
| Programmable Logic Controller | plc | 프로그래머블 로직 컨트롤러 | IIOT, TAG | 산업 현장 자동화 로직을 실행하는 제어 장치 | 신규 후보 · 약어 배정 |
| Distributed Control System | dcs | 분산제어시스템 | IIOT, MOM | 공정 제어를 분산 수행하는 제어 시스템 | 신규 후보 · 약어 배정 |
| Supervisory Control and Data Acquisition | scad | 감시제어·데이터취득 시스템 | IIOT, MOM | 설비·공정의 원격 감시·제어 시스템 | 신규 후보 · 약어 배정 |
| Gateway | gat | 게이트웨이 | IIOT | 서로 다른 네트워크·프로토콜 간 데이터 중계 장치 | 신규 후보 · 약어 배정 |
| Edge Device | ede | 엣지장치 | IIOT | 데이터 원천 근처에서 수집·전처리·제어를 수행하는 장치 | 신규 후보 · 약어 배정 |
| Protocol | prot | 통신프로토콜 | IIOT | 장치 간 데이터 교환 규칙 | 신규 후보 · 약어 배정 |
| Connection | conn | 연결 | IIOT, EDAN | 원천과 수집 시스템 사이의 통신 관계·세션 | 신규 후보 · 약어 배정 |
| Endpoint | end | 엔드포인트 | IIOT, EDAN | 통신 대상의 주소·서비스 접점 | 신규 후보 · 약어 배정 |
| Channel | cha | 채널 | IIOT | 통신·수집 데이터의 논리적 경로 | 신규 후보 · 약어 배정 |
| Topic | top | 토픽 | IIOT | 메시지 브로커에서 메시지를 분류·라우팅하는 이름 | 신규 후보 · 약어 배정 |
| Subscription | subs | 구독 | IIOT, EDAN | 특정 데이터·이벤트 수신을 신청한 관계 | 신규 후보 · 약어 배정 |
| Polling | poll | 폴링 | IIOT | 일정 간격으로 원천에 데이터를 요청하는 수집 방식 | 신규 후보 · 약어 배정 |
| Sampling | samp | 샘플링 | IIOT, TAG | 연속 신호에서 일정 시점·주기로 값을 취득하는 행위 | 신규 후보 · 약어 배정 |
| Sampling Interval | sai | 샘플링주기 | IIOT, TAG | 연속 측정·취득 간 시간 간격 | 신규 후보 · 약어 배정 |
| Acquisition | acq | 데이터취득 | IIOT | 원천에서 원시 데이터를 가져오는 행위 | 신규 후보 · 약어 배정 |
| Ingestion | ing | 데이터적재 | IIOT, EDAN | 취득한 데이터를 플랫폼 저장·처리 흐름에 넣는 단계 | 신규 후보 · 약어 배정 |
| Buffer | buf | 버퍼 | IIOT, EDAN | 전송·처리 지연을 흡수하는 임시 데이터 영역 | 신규 후보 · 약어 배정 |
| Store and Forward | stf | 저장후전송 | IIOT | 연결 단절 시 저장 후 복구되면 전달하는 방식 | 신규 후보 · 약어 배정 |
| Telemetry | tel | 원격계측데이터 | IIOT | 원격 장치에서 전달되는 측정·상태 데이터 | 신규 후보 · 약어 배정 |
| Time Series | tse | 시계열 | IIOT, EDAN | 시간 순서로 기록된 값의 연속 | 신규 후보 · 약어 배정 |
| Event | evt | 이벤트 | IIOT, MES, EDAN | 특정 시점에 발생한 의미 있는 변화 | 기존 개념 통합 |
| Alarm | ala | 경보 | IIOT, MOM, EAM | 조건·한계를 초과하거나 이상을 감지했을 때 알리는 신호 | 신규 후보 · 약어 배정 |
| Notification | not | 통지 | EDAN, MOM | 사용자·시스템에 정보를 전달하는 메시지. Alarm과 구분 | 신규 후보 · 약어 배정 |
| Quality Flag | qfl | 데이터품질표시 | IIOT, TAG | 측정값의 신뢰·유효·대체 상태를 표시 | 신규 후보 · 약어 배정 |
| Deadband | dea | 데드밴드 | IIOT, TAG | 변화량이 지정 폭을 넘을 때만 전송·기록하는 기준 / 작은 변화의 전송·경보를 억제하는 폭 | 신규 후보 · 약어 배정 |
| Heartbeat | hea | 생존신호 | IIOT | 장치·연결의 정상 응답 여부를 확인하는 주기 신호 | 신규 후보 · 약어 배정 |
| Keepalive | kee | 연결유지 | IIOT | 통신 세션의 유지를 확인하는 기능 | 신규 후보 · 약어 배정 |
| Latency | lat | 지연시간 | IIOT, EDAN | 데이터 발생부터 전달·처리까지의 지연 | 신규 후보 · 약어 배정 |
| Throughput | thro | 처리량 | IIOT, EDAN | 단위 시간에 처리·전달한 데이터 양 | 신규 후보 · 약어 배정 |
| Data Quality | dqu | 데이터품질 | IIOT, DQM | 완전성·정확성·유효성·적시성 등 데이터 상태 | 신규 후보 · 약어 배정 |
| Edge Processing | epr | 엣지처리 | IIOT | 원천 가까이에서 필터·변환·집계를 수행 | 신규 후보 · 약어 배정 |
| Protocol Adapter | pad | 프로토콜어댑터 | IIOT, EDAN | 특정 프로토콜을 공통 수집 인터페이스로 변환 | 신규 후보 · 약어 배정 |
| Tag Definition | tde | 태그정의 | TAG, IIOT | 태그의 의미·데이터형·단위·수집 규칙을 정의하는 기준 | 신규 후보 · 약어 배정 |
| Tag Instance | tin | 태그인스턴스 | TAG | 특정 설비·장치에 실제 연결된 태그의 개별 인스턴스 | 신규 후보 · 약어 배정 |
| Tag | tag | 태그 | TAG, IIOT | 설비 데이터 항목을 식별하는 논리 명칭. 물리 주소와 구분 | 신규 후보 · 약어 배정 |
| Tag Path | tpa | 태그경로 | TAG | 태그가 속한 장치·설비·그룹 계층 경로 | 신규 후보 · 약어 배정 |
| Tag Address | tad | 태그주소 | TAG, IIOT | 실제 데이터가 위치한 통신·제어기 주소 | 신규 후보 · 약어 배정 |
| Tag Alias | tal | 태그별칭 | TAG | 동일 태그를 외부 시스템에서 부르는 별도 이름 | 신규 후보 · 약어 배정 |
| Tag Group | tgr | 태그그룹 | TAG | 관리·수집·화면 구성을 위한 태그 묶음 | 신규 후보 · 약어 배정 |
| Tag Namespace | tna | 태그네임스페이스 | TAG | 태그 식별자의 유일성이 적용되는 범위 | 신규 후보 · 약어 배정 |
| Signal | sig | 신호 | TAG, IIOT | 물리·논리적으로 변화하는 값 또는 상태 | 신규 후보 · 약어 배정 |
| Signal Type | sty | 신호유형 | TAG | 아날로그·디지털·펄스 등 신호의 유형 | 신규 후보 · 약어 배정 |
| Data Type | dat | 데이터형 | TAG, EDAN | 값의 표현·저장 형식 | 신규 후보 · 약어 배정 |
| Engineering Unit | eun | 공학단위 | TAG, IIOT | 태그 값이 표현하는 물리량의 단위 | 신규 후보 · 약어 배정 |
| Raw Value | rva | 원시값 | TAG, IIOT | 변환·보정 전 원천에서 읽은 값 | 신규 후보 · 약어 배정 |
| Scaled Value | sva | 환산값 | TAG, IIOT | 원시값에 범위 변환을 적용한 값 | 신규 후보 · 약어 배정 |
| Process Value | pva | 공정값 | TAG, MOM | 공정에서 현재 측정·계산된 값 | 신규 후보 · 약어 배정 |
| Output Value | ova | 출력값 | TAG, MOM | 제어기가 구동 대상으로 내보내는 값 | 신규 후보 · 약어 배정 |
| Status Value | stv | 상태값 | TAG, IIOT | 장치·설비의 상태를 표현하는 값 | 신규 후보 · 약어 배정 |
| Alarm Limit | ali | 경보한계 | TAG, IIOT | 경보 발생 여부를 판단하는 상·하한 | 신규 후보 · 약어 배정 |
| High Limit | hli | 상한 | TAG, QMS | 허용·경보 범위의 상단 경계 | 신규 후보 · 약어 배정 |
| Low Limit | lli | 하한 | TAG, QMS | 허용·경보 범위의 하단 경계 | 신규 후보 · 약어 배정 |
| Scan Rate | sra | 스캔주기 | TAG, IIOT | 제어기·수집기가 태그를 읽는 주기 | 신규 후보 · 약어 배정 |
| Update Rate | ura | 갱신주기 | TAG, IIOT | 저장·전달 값이 갱신되는 주기 | 신규 후보 · 약어 배정 |
| Timestamp | tim | 시각표시 | TAG, IIOT | 측정·수집·저장 중 어떤 사건의 시각인지 유형을 함께 명시 | 신규 후보 · 약어 배정 |
| Source Timestamp | sti | 원천시각 | TAG, IIOT | 원천 장치가 값에 부여한 시각 | 신규 후보 · 약어 배정 |
| Server Timestamp | stm | 서버시각 | TAG, IIOT | 수집 서버가 수신·처리 시 부여한 시각 | 신규 후보 · 약어 배정 |
| Quality Code | qco | 품질코드 | TAG, IIOT | 값의 신뢰·유효 상태를 코드화한 값 | 신규 후보 · 약어 배정 |
| Offset | off | 영점편차 | TAG | 원시값에 더하거나 빼는 보정량 | 신규 후보 · 약어 배정 |
| Gain | gai | 이득 | TAG | 입력 변화에 대한 출력 변화 비율 | 신규 후보 · 약어 배정 |
| Range | ran | 측정범위 | TAG | 장치·태그가 표현할 수 있는 값의 범위 | 신규 후보 · 약어 배정 |
| Resolution | res | 분해능 | TAG | 구분 가능한 최소 변화량 | 신규 후보 · 약어 배정 |
| Historian | his | 시계열이력저장소 | IIOT, TAG | 산업 시계열 데이터를 장기 저장·조회하는 시스템 | 신규 후보 · 약어 배정 |
| Snapshot | snp | 스냅샷 | TAG, EDAN | 특정 시점의 여러 태그 상태를 함께 보존한 기록 | 기존 26개 후보 통합 |
| Attachment | atta | 첨부파일 | DOC, 모든 모듈 | 다른 업무 객체에 연결된 파일 | 신규 후보 · 약어 배정 |
| Evidence | evi | 증적 | QMS, IAM, ERP | 특정 사실·승인·검증을 입증하는 자료 | 신규 후보 · 약어 배정 |
| Approval | app | 승인 | 모든 모듈 | 권한 있는 주체가 업무 상태·내용을 허가하는 결정 | 신규 후보 · 약어 배정 |
| Review | revi | 검토 | PLM, QMS, DOC | 승인 전 내용·적합성을 평가하는 활동 | 신규 후보 · 약어 배정 |
| Workflow | work | 업무흐름 | 모든 모듈 | 업무 단계와 전이 규칙의 정의 | 신규 후보 · 약어 배정 |
| Business Task | btk | 업무작업 | 모든 모듈 | 수행자·기한·상태를 가진 작업 단위 | 의미 분리 |
| Comment | comm | 의견 | 모든 모듈 | 객체에 대한 사용자 의견 | 신규 후보 · 약어 배정 |
| Note | note | 메모 | 모든 모듈 | 참고를 위한 자유 형식 기록 | 신규 후보 · 약어 배정 |
| Reason | rsn | 사유 | 모든 모듈 | 행위·결정의 이유 | 기존 개념 통합 |
| Status | sta | 상태 | 모든 모듈 | 객체의 현재 업무 단계·상태 | 기존 개념 통합 |
| State Transition | sttr | 상태전이 | 모든 모듈 | 한 상태에서 다른 상태로 이동하는 사건 | 신규 후보 · 약어 배정 |
| Effective Date | eda | 적용일 | ERP, PLM, QMS | 규칙·가격·개정 등이 효력을 갖기 시작하는 날짜 | 신규 후보 · 약어 배정 |
| Expiration | exr | 만료 | ERP, IAM, COML | 유효성이 끝나는 시점·사건 | 기존 26개 후보 통합 |
| History | hist | 이력 | 모든 모듈 | 시간순으로 축적한 사건·변경 기록 | 신규 후보 · 약어 배정 |
| Audit Trail | atr | 감사추적 | 모든 모듈 | 누가 언제 무엇을 변경·수행했는지 증명하는 기록 | 신규 후보 · 약어 배정 |
| Retention | rtn | 보존 | DOC, QMS, ERP | 기록을 유지해야 하는 기간·정책 | 기존 개념 통합 |
| Archive | arc | 보관 | DOC, EDAN | 활성 저장소에서 분리해 장기 보존하는 처리 | 기존 개념 통합 |
| Access Control | acn | 접근통제 | IAM, 모든 모듈 | 자원 접근 허용·거부를 결정하는 통제 | 신규 후보 · 약어 배정 |
| Organizational Role | hrl | 역할 | IAM, ERP-HR | 권한 집합 또는 조직상 역할. 직무와 권한역할 구분 | 의미 분리 |
| Permission | perm | 권한 | IAM | 특정 자원에 대해 수행 가능한 행위 | 신규 후보 · 약어 배정 |
| Policy | plcy | 정책 | IAM, 모든 모듈 | 결정·통제의 원칙과 적용 범위 | 기존 개념 통합 |
| Rule | rul | 규칙 | 모든 모듈 | 구체적인 조건·결과를 정의하는 개별 규칙 | 기존 26개 후보 통합 |
| Identity | idn | 신원 | IAM | 인증 주체를 나타내는 속성·주장 집합 | 기존 26개 후보 통합 |
| Identifier | idr | 식별자 | 모든 모듈 | 특정 객체를 식별하기 위한 값 | 기존 개념 통합 · 기존 약어 전환 |
| Actor | actr | 행위 주체 | 모든 모듈 | 행위를 수행한 사용자·시스템·서비스 | 기존 26개 후보 통합 |
| Action | act | 행위 | 모든 모듈 | 수행된 행위 자체. 행위주체와 구분 | 기존 개념 통합 |
| Signature | sign | 서명 | DOC, QMS, IAM | 무결성·승인·책임을 증명하는 서명 데이터 | 신규 후보 · 약어 배정 |
| Acknowledgement | ack | 수신확인 | IIOT, EDAN | 메시지·요청을 수신했음을 확인. 승인과 구분 | 신규 후보 · 약어 배정 |
| Next | nxt | 다음 | 공통 | 순서상 바로 다음 항목 | 기존 26개 후보 추가 |
| Last | lst | 마지막 | 공통 | 시간·순서상 마지막 항목 | 기존 26개 후보 추가 |
| Plan | pln | 계획 | ERP, MOM | 목표 달성을 위한 사전 계획 | 기존 26개 후보 추가 |
| Identity Provider | idp | 인증 제공자 | IAM | 신원을 확인·주장하는 서비스 | 기존 26개 후보 추가 |
| Issuer | iss | 발급자 | IAM, COML | 토큰·인증서·라이선스 발급 주체 | 기존 26개 후보 추가 |
| Period | prd | 기간 | 공통, ERP | 시작·종료 경계를 갖는 시간 범위 | 기존 26개 후보 추가 |
| Capture | cap | 수집·포착 | IIOT, TAG | 원천 데이터 획득 | 기존 26개 후보 추가 |
| Used State | use | 사용 상태 | 공통 | 사용 여부 상태. 사용량과 분리 | 기존 26개 후보 추가 |
| Used Quantity | usq | 사용량 | 공통 | 소비·사용된 수량 또는 용량 | 기존 26개 후보 추가 |
| Usage | usg | 사용 활동 | 공통 | 사용 활동·실적 | 기존 26개 후보 추가 |
| Utilization | util | 활용률·가동률 | MOM, EAM | 가용 자원 대비 사용·가동 비율 | 기존 26개 후보 추가 |
| Definition | def | 정의 | 공통 | 개념·규칙·태그 등의 명세 | 기존 26개 후보 추가 |
| Truncated | trn | 잘림·절단 | Platform | 길이·범위 제한으로 데이터가 잘린 상태 | 기존 26개 후보 추가 |
| Row | row | 행 | DataSet | 표 형식 데이터의 한 행 | 기존 26개 후보 추가 |
| Hash | hsh | 해시 | Platform, Security | 입력 데이터의 고정 길이 요약값 | 기존 26개 후보 추가 |
| Stage | stg | 단계 | MES, MOM | 프로세스·실행 흐름의 단계 | 기존 26개 후보 추가 |
| Token | tkn | 토큰 | IAM, Security | 인증·권한 문맥의 보안 증표 | 기존 26개 후보 추가 |
| Ready | rdy | 준비 상태 | Worker, MES | 실행 시작 조건을 충족한 상태 | 기존 26개 후보 추가 |
| Unix Epoch | uep | Unix 기준시각 | Platform | 1970-01-01T00:00:00Z 기준 경과 시간 | 기존 26개 후보 추가 |
| Millisecond | msec | 밀리초 | Platform, IIOT | 1/1000초 시간 단위. ms는 SI 단위 기호로 별도 관리 | 기존 26개 후보 추가 |
| Seen | seen | 관찰됨·확인됨 | Audit, Platform | 관찰 상태. Read·Acknowledged와 구분 | 기존 26개 후보 추가 |


## 6.1 동의어·약칭 및 인접 개념의 의미 판정

### 동일 개념으로 통합하거나 약칭으로 연결한 항목

| 대표 개념 | 동의어·약칭 | 처리 |
|---|---|---|
| Work in Process | WIP | Work in Process에 통합하고 wip를 대표 약어로 사용 |
| Bill of Materials | BOM | Bill of Materials에 통합하고 bom을 대표 약어로 사용 |
| Unit of Measure | UOM | Unit of Measure에 통합하고 uom을 대표 약어로 사용 |
| Request for Quotation | RFQ | Request for Quotation에 통합하고 rfq를 대표 약어로 사용 |
| Proof of Delivery | POD | Proof of Delivery에 통합하고 pod를 대표 약어로 사용 |
| Programmable Logic Controller | PLC | 풀네임을 대표명으로 사용하고 plc를 대표 약어로 사용 |
| Distributed Control System | DCS | 풀네임을 대표명으로 사용하고 dcs를 대표 약어로 사용 |
| Supervisory Control and Data Acquisition | SCADA | 풀네임을 대표명으로 사용하고 scad를 내부 약어로 사용 |
| Overall Equipment Effectiveness | OEE | 풀네임을 대표명으로 사용하고 oee를 대표 약어로 사용 |

### 독립 개념으로 유지한 인접 용어

| 개념 A | 개념 B | 분리 근거 |
|---|---|---|
| Production Order | Manufacturing Order | ERP 생산 관리 오더와 MES/MOM 제조 실행 상위 오더. 연계키·소유권으로 연결 |
| Purchase Receipt | Goods Receipt | 구매 주문에 대한 수령 사건과 재고 거래 기록을 구분 |
| Goods Receipt | Inspection Receipt | 물품 수령·재고 거래와 검사 대상 등록을 구분 |
| Sales Order | Purchase Order | 고객 판매 약정과 공급업체 구매 약정 |
| Production Order | Work Order | 생산 관리 단위와 개별 작업 지시 |
| Work Order | Maintenance Order | 일반 작업 지시와 설비 정비 지시 |
| Shipment | Goods Delivery | 물류 출하·인계와 수령지 납품 이행 |
| Process | Manufacturing Operation | 제조 변환 공정과 라우팅 내 개별 실행 단계 |
| Tag | Signal | 논리적 데이터 식별자와 수집되는 물리·논리 신호 |
| Signal | Measurement | 신호 정의와 특정 시점에 관측된 값 |
| Traceability | Genealogy | 추적 능력·기능과 실제 투입·산출 계보 관계 |
| Corrective Action | Preventive Action | 발생 부적합 시정과 잠재 문제 예방 |
| Revision | Version | 승인된 내용 개정 차수와 기술·배포 버전 |
| Equipment | Asset | 설비 운영 객체와 재무·관리 자산 |
| Employee | Person | 고용 관계가 있는 인력과 일반 개인 |
| Employee | User | 인사 인력 정보와 시스템 계정 |
| Used State | Used Quantity | 사용 여부 상태와 소비·사용 수량 |
| Expected | Expiration | 기대값·예정값과 유효기간 만료 |
| Action | Actor | 수행 행위와 행위 주체 |
| Identifier | Identity | 객체 식별값과 인증·권한 신원 |

이 표의 인접 개념은 자동 통합하지 않는다. 관계 용어 또는 상위·하위·문맥 관계로 연결하고, 물리 컬럼명은 구체적인 대표 개념을 선택한다.

## 7. 확정 범위 및 후속 적용 원칙

- EDAN 전역 대표 개념 445개와 대표 약어를 표준 버전 1.0으로 확정했다.
- 확정 목록의 전역 약어 중복은 0건이며, 모든 대표 약어는 3자 또는 4자 소문자다.
- 관계 용어·동의어에는 별도 약어를 발급하지 않는다.
- Used, Unix, Seen은 의미를 분리하거나 구체화한 최종 대표 개념을 기준으로 사용한다.
- 본 확정은 업무 용어와 약어 표준에 한정한다. 실제 DB 파일 구성, 물리 컬럼명, EF Core 매핑, Migration, API·외부 SQL 적용은 DB 구조 재검토 후 별도 수행한다.
- 기존 물리 컬럼명은 자동 변경하지 않는다. 레거시 매핑과 Migration 영향 분석 후 변경한다.

## 8. 참고 문서

- [기존 확장 사전](enterprise-business-term-dictionary.md)
- [전역 DB 명명 규칙](database-naming-conventions.md)

## 9. 기존 110개 전자사전 항목 전체 정규화 대조표

기존 116행에서 영문명 기준 중복을 통합한 110개 항목이다. 이 중 103개는 독립 대표 개념이며, Creator·Updater 및 시간·여부·정렬 관련 7개는 독립 약어가 아니라 복합 조합 규칙으로 분류했다.

| 기존 영문명 | 정규화 대표 영문명 | 기존 약어 | 정규화 약어·조합 | 한글명 | 분류 | 판정 |
|---|---|---|---|---|---|---|
| Role | Access Role | role / rol | rol | 접근 권한 역할 | 권한 | 의미 분리 |
| Action | Action | act / act | act | 행위·행위자 | 공통 | 중복 통합 |
| Adapter | Adapter | adp | adp | 어댑터 | 공통 | 기존 약어 유지 |
| After | After | aft | aft | 이후 |  | 기존 약어 유지 |
| Allowed | Allowed | alw | alw | 허용 여부 | 권한 | 기존 약어 유지 |
| Archive | Archive | arc / arc | arc | 아카이브 | 보존 | 중복 통합 |
| Attempt | Attempt | attm | attm | 시도 | 수량 | 기존 약어 유지 |
| Authorization | Authorization | auth | auth | 권한 부여 |  | 기존 약어 유지 |
| Audit | Authorization Audit | aud | aud | 권한 감사 | 권한 | 의미 분리 |
| Resource | Authorization Resource | rsrc | rsrc | 권한 적용 리소스 | 권한 | 의미 분리 |
| Before | Before | bef | bef | 이전 |  | 기존 약어 유지 |
| Bytes | Bytes | byt | byt | 바이트 | 공통 | 기존 약어 유지 |
| Change | Change | chg | chg | 변경 | 권한 | 기존 약어 유지 |
| Checkpoint | Checkpoint | ckpt | ckpt | 체크포인트 | 공통 | 기존 약어 유지 |
| Code | Code | cd | cod | 코드 | 공통 | 표준 약어 전환 |
| Completed | Completed | cmp | cmp | 완료 | 공통 | 기존 약어 유지 |
| Content | Content | ctt | ctt | 콘텐츠 | 콘텐츠 | 기존 약어 유지 |
| Count | Count | cnt | cnt | 개수 | 공통 | 기존 약어 유지 |
| Created | Created | crt | crt | 생성, 추가 | 공통 | 기존 약어 유지 |
| Created DateTime | Created DateTime | crtdt | crt_dtt | 최초 등록일시 | 공통 | 복합 조합 규칙 |
| Creator | Creator | crtmer | crt_by | 최초 등록자 | 공통 | 복합 조합 규칙 |
| DataSet | DataSet | dset | dset | 데이터셋 | 데이터 | 기존 약어 유지 |
| Date | Date | date | date | 일자 | 공통 | 기존 약어 유지 |
| DateTime | DateTime | dt | dtt | 일시 | 공통 | 표준 약어 전환 |
| Delete | Delete | del | del | 삭제 | 공통 | 기존 약어 유지 |
| Delete Yes/No | Delete Yes/No | delyn | del_ynf | 삭제여부 | 공통 | 복합 조합 규칙 |
| Description | Description | desc | desc | 설명 | 작업 | 기존 약어 유지 |
| Detail | Detail | dtl | dtl | 상세 | 데이터 | 기존 약어 유지 |
| Display | Display | dsp | dsp | 표시 | 공통 | 기존 약어 유지 |
| Effect | Effect | eff / eff | eff | 효과 | 공통 | 중복 통합 |
| Email | Email | eml | eml | 전자우편 | 공통 | 기존 약어 유지 |
| Enabled | Enabled | ena / ena | ena | 활성 | 공통 | 중복 통합 |
| End Date and Time | End Date and Time | edt | edt | 종료일시, 종료일자, 완료일시, 완료일자, |  | 기존 약어 유지 |
| Error | Error | err | err | 오류 | 공통 | 기존 약어 유지 |
| Event | Event | evt | evt | 이벤트 | 공통 | 기존 약어 유지 |
| Execution | Execution | exc | exc | 실행 | 데이터 | 기존 약어 유지 |
| Failure | Execution Failure | fail | fail | 실행 실패 | 오류 | 의미 분리 |
| Schedule | Execution Schedule | schd | schd | 예약 실행 일정 | 운영 | 의미 분리 |
| Expected | Expected | exp | exp | 예상·기대 | 공통 | 기존 약어 유지 |
| Delivery | External Delivery | dlv | dlv | 외부 전달 | 작업 | 의미 분리 |
| Fencing | Fencing | fence | fcn | 펜싱 | 공통 | 표준 약어 전환 |
| Field | Field | fld | fld | 필드 | 공통 | 기존 약어 유지 |
| Format | Format | fmt | fmt | 형식 | 공통 | 기존 약어 유지 |
| Hold | Hold | hld | hld | 보류 | 보류 | 기존 약어 유지 |
| Hour | Hour | hr | hou | 시간 | 시간 | 표준 약어 전환 |
| Idempotency | Idempotency | idem | idem | 멱등성 | 공통 | 기존 약어 유지 |
| Identifier | Identifier | id | idr | 식별자 | 공통 | 표준 약어 전환 |
| Interval | Interval | intv | intv | 간격 | 시간 | 기존 약어 유지 |
| JSON | JSON | json | json | JSON | 공통 | 기존 약어 유지 |
| Key | Key | key | key | 키 | 공통 | 기존 약어 유지 |
| Lease | Lease | lease | lse | 임대·Lease | 공통 | 표준 약어 전환 |
| Legal | Legal | lgl | lgl | 법적 | 법무 | 기존 약어 유지 |
| Limit | Limit | lmt | lmt | 한도 | 공통 | 기존 약어 유지 |
| Master | Master | mst | mst | 마스터 | 공통 | 기존 약어 유지 |
| Maximum | Maximum | max | max | 최대 | 수량 | 기존 약어 유지 |
| Membership | Membership | mbr | mbr | 소속·멤버십 | 업무 | 기존 약어 유지 |
| Message | Message | msg | msg | 메시지 | 공통 | 기존 약어 유지 |
| Name | Name | nm | nam | 이름 | 공통 | 표준 약어 전환 |
| Number | Number | num | num | 번호 | 공통 | 기존 약어 유지 |
| Occurred | Occurred | occ | occ | 발생 | 공통 | 기존 약어 유지 |
| Organization | Organization | org | org | 조직 | 업무 | 기존 약어 유지 |
| Original | Original | orig | orig | 원본 | 공통 | 기존 약어 유지 |
| Payload | Payload | pld | pld | 페이로드 | 공통 | 기존 약어 유지 |
| Percent | Percent | pct | pct | 백분율 | 공통 | 기존 약어 유지 |
| Policy | Policy | plcy | plcy | 정책 | 권한 | 기존 약어 유지 |
| Priority | Priority | pri / pri | pri | 우선순위 | 공통 | 중복 통합 |
| Processed | Processed | prc | prc | 처리됨 | 처리 | 기존 약어 유지 |
| Profile | Profile | prf | prf | 프로파일 | 검증 | 기존 약어 유지 |
| Progress | Progress | prgrs | prg | 진행률 | 공통 | 표준 약어 전환 |
| Project | Project | pjt | pjt | 프로젝트 | 업무 | 기존 약어 유지 |
| Quota | Quota | qta | qta | 할당량 | 운영 | 기존 약어 유지 |
| Reason | Reason | rsn | rsn | 사유 | 사유 | 기존 약어 유지 |
| Record | Record | rec | rec | 레코드 | 수량 | 기존 약어 유지 |
| Reference | Reference | ref | ref | 참조 | 공통 | 기존 약어 유지 |
| Replay | Replay | rpl | rpl | 재실행 | 작업 | 기존 약어 유지 |
| Request | Request | req | req | 요청 | 공통 | 기존 약어 유지 |
| Result | Result | rslt | rslt | 결과 | 공통 | 기존 약어 유지 |
| Retention | Retention | rtn | rtn | 보존 | 보존 | 기존 약어 유지 |
| Revision | Revision | rev | rev | 개정 버전 | 버전 | 기존 약어 유지 |
| Schema | Schema | schm | schm | 스키마 | 데이터 | 기존 약어 유지 |
| Scope | Scope | scp | scp | 범위 | 공통 | 기존 약어 유지 |
| Sequence | Sequence | seq | seq | 일련번호 | 공통 | 기존 약어 유지 |
| Severity | Severity | sev | sev | 심각도 | 공통 | 기존 약어 유지 |
| SHA-256 | SHA-256 | sha | sha | SHA-256 해시 | 공통 | 기존 약어 유지 |
| Sort Number | Sort Number | sortnum | srt_num | 정렬번호 | 공통 | 복합 조합 규칙 |
| Start   Date and Time | Start   Date and Time | sdt | sdt | 시작일시, 시작일시 |  | 기존 약어 유지 |
| Status | Status | sta | sta | 상태 | 공통 | 기존 약어 유지 |
| Storage | Storage | str | str | 저장소 | 공통 | 기존 약어 유지 |
| Subject | Subject | sub | sub | 인증 주체 | 인증 | 기존 약어 유지 |
| Successful | Successful | succ | succ | 성공 여부 | 결과 | 기존 약어 유지 |
| Operation | System Operation | op | opr | 시스템 작업 동작 | 공통 | 표준 약어 전환 |
| Target | Target | tgt | tgt | 대상 | 공통 | 기존 약어 유지 |
| Tenant | Tenant | tnt | tnt | 테넌트 | 업무 | 기존 약어 유지 |
| Threshold | Threshold | thr | thr | 임계값 | 공통 | 기존 약어 유지 |
| Total | Total | tot | tot | 전체 | 수량 | 기존 약어 유지 |
| Type | Type | typ | typ | 타입, 유형 |  | 기존 약어 유지 |
| Updated | Updated | upd | upd | 수정 | 공통 | 기존 약어 유지 |
| Updated DateTime | Updated DateTime | upddt | upd_dtt | 마지막 등록일시 | 공통 | 복합 조합 규칙 |
| Updater | Updater | updmer | upd_by | 마지막 등록자 | 공통 | 복합 조합 규칙 |
| URI | URI | uri | uri | URI | 공통 | 기존 약어 유지 |
| Use Yes/No | Use Yes/No | useyn | use_ynf | 사용여부 | 공통 | 복합 조합 규칙 |
| User | User | usr | usr | 사용자 | 업무 | 기존 약어 유지 |
| UTC | UTC | utc | utc | UTC 시각 | 공통 | 기존 약어 유지 |
| Validation | Validation | vld | vld | 검증 | 데이터 | 기존 약어 유지 |
| Value | Value | val | val | 값 | 공통 | 기존 약어 유지 |
| Version | Version | ver | ver | 버전 | 공통 | 기존 약어 유지 |
| Warning | Warning | warn | warn | 경고 | 공통 | 기존 약어 유지 |
| Worker | Worker | wkr | wkr | 워커 | 작업 | 기존 약어 유지 |
| Task | Worker Task | tsk | tsk | 비동기 작업 요청 | 작업 | 의미 분리 |
| Yes/No | Yes/No | yn | ynf | 여부 | 공통 | 표준 약어 전환 |

기존 110개 항목 중 대표 약어로 남는 3자·4자 항목은 103개이며, 복합 조합 규칙 7건은 대표 약어 유일성 검사 대상에서 제외했다.
