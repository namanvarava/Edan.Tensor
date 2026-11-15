# EDAN ERP 업무 모듈 세분화 및 제조·품질·제품수명주기 공유 용어 검토

## 1. 목적과 적용 범위

이 문서는 ERP를 기준 업무 체계로 삼아 영업, 구매, 재고, 창고, 생산계획, 회계, 원가, 인사 모듈을 세분화하고, MES·MOM·QMS·PLM·EAM·IIoT·TAG와 공유하는 대표 업무 개념 및 관계 용어를 정의한다.

이 문서는 모듈 경계와 용어 사전의 설계 기준이다. 특정 ERP 제품의 기능 목록이나 회계·세무 법규 구현을 확정하지 않는다. 국가별 세무·회계 기준, 제조 산업별 규정, 고객별 운영 정책은 별도 정책·지역화 영역으로 관리한다.

## 2. 전역 모듈 구조

| 상위 영역 | 모듈 코드 | 모듈명 | 주요 책임 |
| --- | --- | --- | --- |
| 공통 | COM | 공통 마스터 | 조직·사업장·거래처·품목·단위·공통 코드 |
| ERP | ERP-SAL | 영업관리 | 수요·견적·수주·계약·출하요청·매출 연계 |
| ERP | ERP-PUR | 구매관리 | 구매요청·소싱·견적·발주·입고·매입 연계 |
| ERP | ERP-INV | 재고관리 | 재고수불·가용성·예약·로트·일련번호·재고평가 수량 |
| ERP | ERP-WHS | 창고관리 | 창고구조·입고작업·적치·피킹·포장·출고·실사 |
| ERP | ERP-PLN | 생산계획관리 | 수요·공급계획·MPS·MRP·능력계획·생산오더 계획 |
| ERP | ERP-FIN | 회계관리 | 총계정원장·채권·채무·자금·세금·결산·고정자산 회계 |
| ERP | ERP-CST | 원가관리 | 원가요소·원가센터·표준·실제·배부·차이·수익성 |
| ERP | ERP-HR | 인사관리 | 인사기본·조직·직무·근태·급여·역량·교육 |
| 제조 | MES | 제조실행관리 | 제조오더 실행·작업실적·자재소비·생산추적 |
| 제조 | MOM | 제조운영관리 | 생산운영·자원조정·운영성과·현장 의사결정 |
| 품질 | QMS | 품질관리 | 검사계획·검사실행·부적합·시정예방·품질문서 |
| 제품 | PLM | 제품수명주기관리 | 제품정의·EBOM·개정·설계변경·기술문서 |
| 자산 | EAM | 설비·자산관리 | 설비계층·보전전략·정비요청·정비오더·고장 |
| 수집 | IIOT | 설비정보수집 | 장치연결·프로토콜·수집파이프라인·시계열·알람 |
| 태그 | TAG | 태그관리 | 태그정의·주소·단위·스캔·품질코드·교정연계 |

ERP-WHS와 ERP-INV는 별도 모듈로 분리한다. 재고관리(무엇을 얼마나 보유하는가)와 창고관리(물품을 어디에 어떻게 보관·이동·작업하는가)는 책임과 트랜잭션이 다르다. 작은 설치에서는 하나의 배포 단위로 운영할 수 있지만 논리 모듈 경계는 유지한다.

모듈은 업무 책임과 데이터 소유권을 기준으로 나눈다. 공통 마스터 개념을 각 모듈에 복제하지 않는다. 모듈 코드와 대표 약어는 별도 네임스페이스가 아니며 대표 약어는 EDAN 전역에서 하나만 승인한다.

## 3. 공통 마스터(COM)

| 대표 개념 | 한글명 | 관계 용어·검색어 | 기준 소유 | 공유 모듈 및 경계 |
| --- | --- | --- | --- | --- |
| Legal Entity | 법인 | 회사, 사업주체 | COM | FIN·HR·SAL·PUR. 법적 회계·계약 주체 |
| Organization | 조직 | 조직단위, 부서(문맥 구분) | COM | 전 모듈. 조직 계층 노드 |
| Department | 부서 | 부문, 팀(계층 구분) | COM | HR·FIN·CST. 조직의 하위 단위일 수 있음 |
| Business Unit | 사업부 | 사업단위, BU | COM | SAL·CST·FIN. 손익·운영 책임 단위 |
| Site | 사업장 | 거점, 현장(문맥 구분) | COM | ERP·MES·EAM. 운영 주소·거점 |
| Plant | 공장 | 생산공장, 플랜트(산업 문맥) | COM | PLN·MES·MOM·INV·QMS·EAM. 제조·재고·원가 책임 단위 |
| Location | 위치 | 장소, 위치노드 | COM | WHS·EAM·TAG. 범용 위치. 창고 내부 저장위치와 구분 |
| Business Partner | 거래처 | 파트너, 거래상대방 | COM | SAL·PUR·FIN·QMS·SCM. 고객·공급업체 역할을 가질 수 있음 |
| Customer | 고객 | 매출처, 수요처(문맥 구분) | COM | SAL·FIN·QMS. 구매 측 거래 역할 |
| Supplier | 공급업체 | 매입처, 벤더, 공급자 | COM | PUR·QMS·FIN·SCM. 조달 공급 역할 |
| Person | 개인 | 인물, 자연인 | COM | HR·IAM. 사람 자체 |
| Employee | 종업원 | 직원, 사원, 임직원(범위 정의) | HR | MES·MOM·QMS·EAM의 작업자 참조 |
| User Account | 사용자계정 | 계정, 로그인 사용자 | IAM | 전 모듈. 인증 주체. Employee와 동일 개념 아님 |
| Item | 품목 | 품목마스터 | COM | SAL·PUR·INV·WHS·PLN·MES·QMS·PLM |
| Product | 제품 | 완제품, 판매제품 | PLM/COM 정책 | SAL·PLN·MES·QMS. 설계·판매 산출물 |
| Material | 자재 | 원자재, 부자재, 투입재 | COM/ERP 정책 | PUR·INV·PLN·MES·QMS. 생산·서비스 투입물 |
| Part | 부품 | 구성품, 파트 | PLM | BOM·PUR·INV·MES. 제품 구성요소 |
| Unit of Measure | 측정단위 | UOM, 단위 | COM | 전 모듈. 수량·중량·길이·시간 등의 단위 |
| Currency | 통화 | 화폐단위 | COM/FIN | SAL·PUR·FIN·CST. 금액 통화 |
| Tax Code | 세금코드 | 과세코드, 세율코드 | FIN | SAL·PUR·FIN. 관할별 적용 |
| Common Code Group | 공통코드그룹 | 코드분류, 코드그룹 | COM | 전 모듈. 코드 분류 체계 |
| Common Code | 공통코드 | 코드값, 공통코드항목 | COM | 전 모듈. 코드그룹에 속하는 선택값 |
| Document | 문서 | 업무문서, 기술문서(유형 구분) | DOC | 전 모듈. 문서유형과 업무객체 분리 |
| Attachment | 첨부파일 | 첨부, 파일 | DOC | 전 모듈. 업무객체에 연결되는 파일 |

공통 개념 구분:
- Business Partner는 거래 상대방의 공통 마스터이고 Customer·Supplier는 거래 역할이다. 한 거래처가 두 역할을 모두 가질 수 있다.
- Employee는 사람·고용관계 데이터이며 User Account는 인증·접근 계정이다. 자동으로 1:1 통합하지 않는다.
- Item은 공통 품목 식별 개념이다. Product·Material·Part는 유형·역할·설계 문맥에 따라 분리된 개념으로 연결한다.
- Site와 Plant는 조직마다 같은 뜻으로 쓰이지 않는다. 사업장 아래 공장이 여러 개 있을 수 있다.

## 4. ERP-SAL 영업관리

### 4.1 하위 기능
| 하위 기능 | 책임 | 주요 객체 |
| --- | --- | --- |
| 영업기회 | 잠재 수요와 영업 파이프라인 | Opportunity |
| 고객수요·예측 | 고객 요청·판매 예측 | Customer Demand, Sales Forecast |
| 견적 | 견적 요청과 가격 제안 | RFQ, Quotation |
| 계약 | 판매 계약과 조건 | Sales Contract |
| 수주 | 고객 주문 접수·확정 | Sales Order |
| 주문이행 | 납기·가용성·분할이행 | Order Fulfillment, Schedule Line |
| 가격·할인 | 가격표·할인·가격결정 | Price List, Discount |
| 출하요청 | 출고·운송 실행 요청 | Shipment Request |
| 청구·매출연계 | 청구·수익인식 연결 | Billing Document, Revenue |
| 반품·클레임 | 반품·교환·고객불만 | Sales Return, Customer Claim |

### 4.2 대표 용어와 관계 용어
| 대표 개념 | 한글명 | 관계 용어 | 공유 모듈 | 의미 경계 |
| --- | --- | --- | --- | --- |
| Opportunity | 영업기회 | 잠재영업, Sales Lead(문맥 구분) | CRM, SAL | 수주 전 잠재 거래 가능성 |
| Demand | 수요 | 고객수요, 시장수요 | PLN, SCM, SAL | 필요한 제품·수량·시점 |
| Sales Forecast | 영업예측 | 판매예측, 수요예측(범위 구분) | PLN, SCM | 미래 판매량 추정. 확정 주문 아님 |
| Request for Quotation | 견적요청 | RFQ, 가격견적요청 | PUR, SAL | 가격 제안을 요청하는 문서 |
| Quotation | 견적 | 견적서, 가격제안 | SAL, PUR | 요청에 대한 가격·조건 제안 |
| Sales Contract | 판매계약 | 매매계약, 공급계약(문맥 구분) | FIN, DOC, PLM | 법적·상업적 계약조건 |
| Sales Order | 수주 | 고객주문, 판매주문, 수주서 | PLN, INV, WHS, MES, FIN | 고객의 구매 의사를 접수한 판매 주문 |
| Sales Order Line | 수주항목 | 주문상세, 주문라인 | PLN, INV, MES | 주문의 품목·수량·가격 단위 |
| Schedule Line | 납품일정항목 | 분할납기, 납기라인 | PLN, WHS | 주문 항목의 날짜·수량별 이행계획 |
| Available-to-Promise | 납기약속가능량 | ATP, 약속가능재고(주의) | INV, PLN | 수요·공급·예약을 고려한 약속 가능량 |
| Order Fulfillment | 주문이행 | 주문충족, 이행관리 | WHS, TMS, FIN | 주문요건을 충족하는 종단 업무 흐름 |
| Shipment Request | 출하요청 | 출고요청(창고 작업과 구분) | WHS, TMS | 출하 실행을 요청하는 문서 |
| Shipment | 출하 | 선적, 운송인계(문맥) | WHS, TMS, FIN | 물품을 운송 단계로 인계하는 사건·문서 |
| Delivery | 납품 | 배송완료, 인도(계약 문맥 구분) | TMS, FIN, QMS | 고객에게 전달·인도하는 이행 결과 |
| Billing Document | 청구문서 | 청구서, 매출청구서 | FIN | 고객에게 지급을 청구하는 문서 |
| Revenue | 수익 | 매출(회계 문맥 주의) | FIN, CST | 회계 기준에 따른 수익 인식 금액·사건 |
| Sales Return | 판매반품 | 고객반품, 반품요청 | INV, QMS, FIN | 판매한 제품의 반환 거래 |
| Customer Claim | 고객클레임 | 고객불만, 품질클레임 | QMS | 고객이 제기한 문제·보상 요구 |

수주금액, 청구금액, 수금액, 수익인식액은 서로 다른 금액 개념이다. Sales Order Amount, Billing Amount, Cash Receipt, Revenue를 하나의 Amount 개념으로 통합하지 않는다.

## 5. ERP-PUR 구매관리

### 5.1 하위 기능
| 하위 기능 | 책임 | 주요 객체 |
| --- | --- | --- |
| 구매요청 | 내부 구매 수요 접수 | Purchase Requisition |
| 소싱·업체선정 | 공급원 탐색·평가 | Sourcing Event, Supplier Evaluation |
| 견적요청·비교 | 공급업체 가격·조건 비교 | RFQ, Supplier Quotation |
| 구매계약 | 장기 공급조건 | Purchase Contract |
| 발주 | 구매 주문 확정 | Purchase Order |
| 납품일정 | 공급 납기·분할 납품 | Supplier Schedule |
| 입고예정 | 공급 납품예정 관리 | Expected Receipt, ASN |
| 구매입고 연계 | 입고·검사 요청 | Goods Receipt, Inspection Lot |
| 매입·송장검증 | 공급업체 청구 대사 | Supplier Invoice, Invoice Match |
| 구매반품 | 공급업체 반송 | Supplier Return |

### 5.2 대표 용어와 관계 용어
| 대표 개념 | 한글명 | 관계 용어 | 공유 모듈 | 의미 경계 |
| --- | --- | --- | --- | --- |
| Purchase Requisition | 구매요청 | 구매신청, 구매의뢰 | PLN, FIN | 내부 부서가 구매 필요를 요청 |
| Sourcing Event | 소싱행사 | 입찰, 공급원선정 | SRM, SCM | 공급업체를 경쟁·선정하는 절차 |
| Supplier Quotation | 공급업체 견적 | 매입견적, 견적회신 | PUR | 공급업체가 제시한 가격·조건 |
| Purchase Contract | 구매계약 | 공급계약, 장기구매계약 | FIN, DOC | 구매 조건·기간·가격 계약 |
| Purchase Order | 발주 | 구매주문, 발주서 | INV, WHS, PLN, FIN, QMS | 공급업체에 발행하는 구매 주문 |
| Purchase Order Line | 발주항목 | 발주상세, 발주라인 | INV, WHS | 발주 품목·수량·단가 단위 |
| Supplier Schedule | 공급일정 | 납품일정, 공급계획 | PLN, WHS | 공급업체 납품 예정 |
| Expected Receipt | 입고예정 | 입고예정정보 | WHS, INV | 미래 입고 예상. 실제 입고 아님 |
| Advance Shipping Notice | 사전출하통지 | ASN, 납품예고 | WHS, TMS | 공급업체가 발송 전에 보내는 통지 |
| Goods Receipt | 입고 | 구매입고, 자재수령 | INV, WHS, QMS, FIN | 물품을 실제 수령하고 재고거래로 기록 |
| Inspection Lot | 검사로트 | 입고검사대상, 검사단위 | QMS | 검사 대상 묶음. 입고 거래와 분리 |
| Supplier Invoice | 공급업체 송장 | 매입송장, 매입청구서 | FIN | 공급업체가 지급을 청구하는 문서 |
| Invoice Matching | 송장대사 | 2-way match, 3-way match | FIN, PUR | 발주·입고·송장 간 수량·금액 검증 |
| Supplier Return | 공급업체반품 | 구매반품, 반송 | INV, QMS, FIN | 공급업체로 되돌려 보내는 거래 |

구매요청은 내부 요청, Purchase Order는 외부 발주, Goods Receipt는 실물 수령, Supplier Invoice는 금액 청구다. 하나의 주문 상태로 합치지 않는다.

## 6. ERP-INV 재고관리

### 6.1 하위 기능
| 하위 기능 | 책임 | 주요 객체 |
| --- | --- | --- |
| 재고마스터 | 품목별 재고 관리 속성 | Inventory Item |
| 재고수불 | 입고·출고·이동·조정 | Inventory Transaction |
| 재고잔량 | 시점별 보유 수량 | Inventory Balance |
| 재고상태 | 가용·보류·검사·불량 | Stock Status |
| 예약·할당 | 주문·생산용 재고 확보 | Reservation, Allocation |
| 로트·일련번호 | 추적 단위 관리 | Lot, Serial Number |
| 재고실사 | 장부·실물 수량 대사 | Stock Count |
| 재고이동 | 창고·위치 간 이동 | Stock Transfer |
| 재고평가 연계 | 재고금액·평가 | Inventory Valuation |

### 6.2 대표 용어와 관계 용어
| 대표 개념 | 한글명 | 관계 용어 | 공유 모듈 | 의미 경계 |
| --- | --- | --- | --- | --- |
| Inventory | 재고 | 재고자산(회계 문맥 구분), Stock | WHS, FIN, CST, PLN | 특정 시점 보유 품목·수량 |
| Inventory Balance | 재고잔량 | 재고수량, 재고현황 | WHS, PLN | 기준시점의 수량 상태 |
| Inventory Transaction | 재고거래 | 재고수불, 수불내역 | WHS, FIN, CST | 재고를 증감·이동시키는 원장 사건 |
| Stock Status | 재고상태 | 재고구분, 품질상태(별도 속성) | WHS, QMS | 사용 가능 여부·관리 상태 |
| Available Quantity | 가용수량 | 가용재고, 사용가능수량 | SAL, PLN, MES | 예약·보류 등을 제외하고 사용할 수 있는 수량 |
| Reserved Quantity | 예약수량 | 할당재고, 예약재고 | SAL, PLN, MES | 특정 수요를 위해 확보한 수량 |
| Safety Stock | 안전재고 | 안전재고량, 완충재고 | PLN | 수요·공급 변동 대비 최소 보유 목표 |
| Reorder Point | 재주문점 | 발주점, 보충기준점 | PLN, PUR | 보충을 시작할 재고 수준 |
| Lot | 로트 | 배치, Batch(문맥 구분) | QMS, MES, PLM | 동일 조건·생산 묶음의 추적 단위 |
| Serial Number | 일련번호 | 시리얼, 개체번호 | MES, QMS, PLM, EAM | 개별 실물 단위 식별자 |
| Stock Transfer | 재고이동 | 창고이동, 위치이동 | WHS | 위치·창고 간 재고 이동 지시·거래 |
| Stock Adjustment | 재고조정 | 수량조정, 장부조정 | WHS, FIN, CST | 실사·오류·손상 등으로 장부 수량 조정 |
| Stock Count | 재고실사 | 재고조사, 순환실사 | WHS, FIN | 실물 수량을 세고 장부와 대사 |
| Inventory Valuation | 재고평가 | 재고금액평가 | FIN, CST | 회계·원가 기준으로 재고 금액 산정 |
| Ownership | 소유권 | 재고소유, 위탁소유 | FIN, PUR, SAL | 물리 보관과 법적 소유의 구분 |

Inventory Quantity와 Inventory Value는 분리한다. 물리적으로 창고에 존재하는 수량과 회사가 소유한 재고 수량도 위탁·보관 계약에 따라 다를 수 있다.

## 7. ERP-WHS 창고관리

### 7.1 하위 기능
| 하위 기능 | 책임 | 주요 객체 |
| --- | --- | --- |
| 창고구조 | 창고·구역·통로·랙·빈 정의 | Warehouse, Zone, Bin |
| 입고작업 | 도크·하역·검수대기·입고확정 | Receiving |
| 적치 | 보관위치 결정·이동 | Putaway |
| 보충 | 피킹 위치 재고 보충 | Replenishment |
| 피킹 | 주문·생산 투입품 집품 | Picking |
| 포장 | 포장단위·라벨·출하포장 | Packing |
| 출고 | 출고작업·상차·인계 | Dispatch |
| 내부이동 | 창고 내 이동 | Warehouse Movement |
| 실사 | 위치별 실사·차이조정 요청 | Cycle Count |
| 작업배정 | 작업자·장비 작업큐 | Warehouse Task |

### 7.2 대표 용어와 관계 용어
| 대표 개념 | 한글명 | 관계 용어 | 공유 모듈 | 의미 경계 |
| --- | --- | --- | --- | --- |
| Warehouse | 창고 | 물류창고, 보관창고 | INV, PUR, SAL, MES | 재고를 보관·운영하는 시설·논리 단위 |
| Warehouse Zone | 창고구역 | 존, 보관구역 | INV, EAM | 창고 내 운영·보관 구역 |
| Storage Location | 저장위치 | 로케이션, Bin(세부 유형) | INV, EAM | 창고 안에서 재고를 찾는 위치 |
| Dock | 도크 | 하역장, 입출고장 | TMS | 차량·화물 상하차 지점 |
| Receiving | 입고작업 | 입하작업, 수령작업 | PUR, INV, QMS | 입고 처리의 현장 작업 절차 |
| Putaway | 적치 | 로케이션 적치, 보관배치 | INV | 수령 물품을 저장위치에 배치 |
| Replenishment | 보충 | 피킹보충, 재고보충 | INV, PLN | 저장·피킹 위치에 필요한 수량 보충 |
| Picking | 피킹 | 집품, 피킹작업 | SAL, MES | 주문·생산요청에 맞는 품목 집품 |
| Packing | 포장 | 패킹, 포장작업 | SAL, TMS | 운송·보관 단위로 포장 |
| Dispatch | 출고작업 | 출고처리, 출문 | SAL, TMS, INV | 창고에서 물품을 내보내는 현장 실행 |
| Loading | 상차 | 적재, 차량적재 | TMS | 차량에 화물을 싣는 작업 |
| Warehouse Task | 창고작업지시 | 작업지시, 창고태스크 | MOM | 창고 작업자·장비가 수행할 단위 작업 |
| Handling Unit | 취급단위 | HU, 팔레트단위, 컨테이너단위 | TMS, INV | 함께 이동·추적하는 포장·적재 단위 |
| Pallet | 팔레트 | 파렛트, 적재판 | TMS, EAM | 물품 적재용 물리 운반대 |
| Cycle Count | 순환실사 | 주기실사, 부분실사 | INV | 일부 위치·품목을 주기적으로 실사 |

Warehouse, Warehouse Zone, Storage Location은 계층이다. 범용 Location과 창고 전용 Storage Location을 구분한다.

## 8. ERP-PLN 생산계획관리

### 8.1 하위 기능
| 하위 기능 | 책임 | 주요 객체 |
| --- | --- | --- |
| 수요계획 | 판매·고객·예측 수요 통합 | Demand Plan |
| 공급계획 | 공급원·재고·생산·구매 계획 | Supply Plan |
| 기준생산계획 | 기간별 완제품 생산량 | Master Production Schedule |
| 자재소요계획 | BOM·재고·리드타임 기반 자재 계산 | MRP Run |
| 능력계획 | 설비·인력·라인 능력 검토 | Capacity Plan |
| 생산일정 | 기간·라인·작업장 배치 | Production Schedule |
| 생산오더 계획 | 생산오더 생성·확정 | Planned Order, Production Order |
| 외주계획 | 외주 공정·공급능력 | Subcontract Plan |
| 계획시나리오 | 대안·시뮬레이션 비교 | Planning Scenario |

### 8.2 대표 용어와 관계 용어
| 대표 개념 | 한글명 | 관계 용어 | 공유 모듈 | 의미 경계 |
| --- | --- | --- | --- | --- |
| Demand Plan | 수요계획 | 수요계획안, Demand Planning Result | SAL, SCM | 기간별 예상 수요 |
| Supply Plan | 공급계획 | 공급계획안 | PUR, INV, MES | 수요 충족을 위한 생산·구매·재고 계획 |
| Master Production Schedule | 기준생산계획 | MPS, 주생산일정 | MES, MOM | 제품·기간별 생산 기준량 |
| Material Requirements Planning | 자재소요계획 | MRP, 자재계획 | PUR, INV, PLM | BOM·재고·리드타임 기반 자재 필요량 계산 |
| Planned Order | 계획오더 | 계획주문, 제안오더 | PUR, MES | 실행 전 계획 단계의 주문 제안 |
| Production Order | 생산오더 | 제조오더, 생산지시(문맥) | MES, MOM, QMS, CST | 제품·수량·기간을 가진 생산 실행 명령 |
| Work Order | 작업오더 | 작업지시, 현장작업오더 | MES, EAM, MOM | 작업 수행 명령. 생산·정비 유형 구분 필요 |
| Capacity Plan | 능력계획 | 생산능력계획, 용량계획 | MOM, EAM, HR | 자원별 가용능력과 부하 |
| Resource | 자원 | 생산자원, 설비·인력·도구 | MOM, EAM, HR | 계획·실행에 투입되는 대상 |
| Routing | 공정경로 | 라우팅, 작업순서 | PLM, MES | 제품을 만드는 공정 단계와 순서 |
| Production Schedule | 생산일정 | 생산스케줄, 작업일정(문맥) | MOM, MES | 생산오더를 시간·자원에 배치 |
| Lead Time | 리드타임 | 조달기간, 생산소요기간(유형 구분) | PUR, MES, EAM | 시작부터 완료까지의 경과기간 |
| Planning Scenario | 계획시나리오 | What-if, 대안계획 | SCM | 가정 조건별 계획 비교 |

Planned Order는 계획 제안이며 Production Order는 실행 통제 대상이다. Routing은 공정 정의이고 Production Schedule은 시간 배치다.

## 9. ERP-FIN 회계관리

### 9.1 하위 기능
| 하위 기능 | 책임 | 주요 객체 |
| --- | --- | --- |
| 총계정원장 | 회계전표·계정잔액 | Journal Entry, Ledger |
| 매출채권 | 고객 청구·수금·연체 | Accounts Receivable |
| 매입채무 | 공급업체 송장·지급 | Accounts Payable |
| 자금관리 | 현금·은행·자금예측 | Cash Position |
| 세금관리 | 세금코드·세금계산·신고자료 | Tax Transaction |
| 고정자산회계 | 자산취득·감가상각·처분 | Fixed Asset |
| 결산관리 | 기간마감·결산조정 | Financial Closing |
| 예산관리 | 예산 편성·통제 | Budget |
| 재무보고 | 재무제표·관리보고 | Financial Statement |

### 9.2 대표 용어와 관계 용어
| 대표 개념 | 한글명 | 관계 용어 | 공유 모듈 | 의미 경계 |
| --- | --- | --- | --- | --- |
| Chart of Accounts | 계정과목체계 | COA, 계정체계 | CST, ERP 전 모듈 | 회계 계정의 구조·분류 |
| General Ledger | 총계정원장 | GL, 원장 | CST | 회계 분개가 누적되는 원장 |
| Journal Entry | 회계전표 | 분개전표, 전표 | SAL, PUR, INV, CST | 차변·대변 회계 기록 단위 |
| Journal Line | 전표항목 | 분개라인, 전표상세 | CST | 전표의 개별 계정·금액 행 |
| Accounts Receivable | 매출채권 | AR, 외상매출금(계정 문맥) | SAL | 고객으로부터 받을 금액 |
| Accounts Payable | 매입채무 | AP, 외상매입금(계정 문맥) | PUR | 공급업체에 지급할 금액 |
| Invoice | 송장 | 청구서, 세금계산서(법적 문서 구분) | SAL, PUR | 거래·지급 청구의 증빙 문서 |
| Payment | 지급 | 결제, 지급처리 | SAL, PUR, COML | 금전 지급 행위 |
| Cash Receipt | 현금수금 | 입금, 수금전표 | SAL, AR | 고객으로부터 현금을 수령한 금융 사건 |
| Goods Receipt | 입고 | 물품수령, 구매입고 | PUR, INV, WHS | 물품 수령 거래. 현금수금과 다름 |
| Disbursement | 자금지급 | 출금, 지급집행 | PUR, AP | 회사 자금을 외부로 지급 |
| Bank Account | 은행계좌 | 계좌, 금융계좌 | COML | 금융기관 계좌 |
| Tax Transaction | 세금거래 | 과세거래, 세금기록 | SAL, PUR | 과세 기준으로 기록하는 거래 |
| Fixed Asset | 고정자산 | 유형자산(회계 분류) | EAM, CST | 장기간 사용하는 회계 자산 |
| Depreciation | 감가상각 | 상각, 감가 | EAM, CST | 자산 취득원가의 기간 배분 |
| Fiscal Period | 회계기간 | 회계연도, 결산기간 | CST | 회계보고·마감 기간 |
| Financial Closing | 결산 | 월마감, 연마감 | CST | 기간 장부를 검증·마감하는 절차 |
| Budget | 예산 | 예산액, 편성예산 | CST, HR, PLN | 미래 기간의 승인된 재정 계획 |
| Financial Statement | 재무제표 | 재무보고서 | CST | 재무상태·성과·현금흐름 보고 |

Receipt는 물품 수령과 현금 수령에 모두 쓰이는 다의어다. 업무 개념은 Goods Receipt와 Cash Receipt로 구분하고 Receipt만으로 컬럼 의미를 결정하지 않는다.

## 10. ERP-CST 원가관리

### 10.1 하위 기능
| 하위 기능 | 책임 | 주요 객체 |
| --- | --- | --- |
| 원가 기준정보 | 원가요소·원가센터·원가대상 | Cost Element, Cost Center |
| 표준원가 | 표준 자재·노무·경비 | Standard Cost |
| 실제원가 | 실제 소비·실적 기반 원가 | Actual Cost |
| 제조원가 | 재료비·노무비·제조경비 | Manufacturing Cost |
| 원가집계 | 공정·오더·제품별 집계 | Cost Rollup |
| 원가배부 | 간접비·공통비 배부 | Cost Allocation |
| 원가차이 | 표준 대비 실제 차이 | Cost Variance |
| 수익성 분석 | 제품·고객·사업부 손익 | Profitability Analysis |
| 재고평가 연계 | 재고 원가 반영 | Inventory Valuation |

### 10.2 대표 용어와 관계 용어
| 대표 개념 | 한글명 | 관계 용어 | 공유 모듈 | 의미 경계 |
| --- | --- | --- | --- | --- |
| Cost Element | 원가요소 | 원가항목, 비용요소(회계 정책) | FIN, PLN | 원가를 구성하는 재료·노무·경비 등의 분류 |
| Cost Center | 원가센터 | 비용센터, 원가부문 | HR, MOM, EAM | 비용 발생 책임 단위 |
| Cost Object | 원가대상 | 원가집계대상 | MES, PLM | 원가를 집계할 제품·오더·공정 등 |
| Standard Cost | 표준원가 | 기준원가, 예정원가(구분) | PLN, MES | 사전 정의한 기준 원가 |
| Actual Cost | 실제원가 | 실적원가, 발생원가 | MES, FIN | 실제 소비·활동으로 계산된 원가 |
| Material Cost | 재료비 | 자재비, 직접재료비 | PUR, INV, MES | 생산에 투입된 재료 원가 |
| Labor Cost | 노무비 | 인건비(범위 구분) | HR, MES | 작업 인력 활동에 귀속한 원가 |
| Overhead Cost | 제조간접비 | 간접비, 경비 | EAM, MOM | 직접 추적이 어려운 제조 지원 비용 |
| Cost Allocation | 원가배부 | 비용배부, 배부처리 | FIN | 공통 비용을 원가대상에 배분 |
| Cost Rollup | 원가집계 | 원가롤업, 상위품 원가계산 | PLM, PLN | 하위 구성·공정 원가를 상위 대상에 누적 |
| Cost Variance | 원가차이 | 원가편차, 표준실제차이 | FIN, MES | 기준원가와 실제원가의 차이 |
| Work Center | 작업장 | 워크센터, 생산작업장 | MES, MOM, EAM | 생산 작업을 수행하는 물리·논리 단위 |
| Activity Type | 활동유형 | 작업활동유형 | HR, MOM | 원가 배부·능력 계산에 쓰는 활동 분류 |
| Profitability Segment | 수익성분석단위 | 수익성 세그먼트 | SAL, FIN | 고객·제품·시장별 수익성 분석 단위 |

Cost Center는 비용 책임 조직, Work Center는 생산 작업 수행 자원이다. 하나의 Work Center가 여러 Cost Center의 비용을 발생시킬 수 있다.

## 11. ERP-HR 인사관리

### 11.1 하위 기능
| 하위 기능 | 책임 | 주요 객체 |
| --- | --- | --- |
| 인사기본 | 개인·고용·인사번호 | Employee, Employment |
| 조직관리 | 조직·직위·보고관계 | Organization Assignment |
| 직무·직책 | 직무·직책·직급 | Job, Position |
| 근태관리 | 출퇴근·근무시간·교대 | Attendance, Shift |
| 근무계획 | 교대·근무조·작업배치 | Work Schedule |
| 급여관리 | 급여항목·급여계산 | Payroll |
| 역량·자격 | 역량·자격·면허 | Competency, Qualification |
| 교육관리 | 교육과정·이수·평가 | Training |
| 인력계획 | 인원·기술·가용성 계획 | Workforce Plan |
| 현장 작업자 연계 | 작업자 자격·작업실적 연결 | Operator Assignment |

### 11.2 대표 용어와 관계 용어
| 대표 개념 | 한글명 | 관계 용어 | 공유 모듈 | 의미 경계 |
| --- | --- | --- | --- | --- |
| Employment | 고용관계 | 재직관계, 근로관계 | FIN, IAM | 개인과 고용주 사이의 고용 계약·기간 |
| Employee | 종업원 | 직원, 사원, 임직원 | MES, MOM, QMS, EAM | 조직에 고용된 사람 |
| Organization Assignment | 조직배치 | 부서배치, 조직소속 | FIN, MOM | 종업원의 조직 소속 이력 |
| Job | 직무 | 직무유형, 업무직종 | MOM, QMS | 수행해야 하는 일의 유형 |
| Position | 직위 | 정원직위, 포지션 | COM, MOM | 조직 내 정원·직위 슬롯 |
| Grade | 직급 | 등급, 직급체계 | FIN | 인사상 등급. 직무·직책과 구분 |
| Role | 역할 | 권한역할, 직무역할 | IAM, MOM | 권한역할과 업무역할은 문맥으로 구분 |
| Attendance | 근태 | 출결, 출퇴근 기록 | MOM, FIN | 출근·결근·근무시간 기록 |
| Shift | 교대조 | 근무조, 시프트 | MOM, MES | 시간대별 근무 편성 |
| Work Schedule | 근무일정 | 근무표, 스케줄 | MOM | 개인·조직·교대별 계획 근무시간 |
| Payroll | 급여 | 임금계산, 급여처리 | FIN | 급여 기간별 계산·지급 결과 |
| Pay Element | 급여항목 | 수당항목, 공제항목 | FIN | 급여 계산의 구성 항목 |
| Competency | 역량 | 능력, 숙련역량 | MOM, QMS | 업무 수행에 필요한 지식·기술 수준 |
| Qualification | 자격 | 자격증, 인증자격 | QMS, EAM, MOM | 특정 작업을 수행할 자격·승인 |
| Training | 교육 | 훈련, 직무교육 | QMS, MOM | 역량·자격을 확보·유지하는 교육 활동 |
| Operator | 작업자 | 오퍼레이터, 현장작업자 | MES, MOM | 생산·설비 작업을 실제 수행하는 주체 |
| Workforce Plan | 인력계획 | 인력수급계획, 인원계획 | PLN, MOM | 미래 기간 인력 수요·공급 계획 |

Job(직무), Position(직위), Role(역할), Qualification(자격), Operator(실제 작업자)는 각각 다른 개념이다. Role은 IAM 권한 역할과 HR·MOM 업무 역할을 문맥으로 구분한다.

## 12. ERP와 MES·MOM·QMS·PLM·EAM 공유 개념 및 소유권

소유 모듈은 EDAN 논리 개념의 기준 정의를 책임지는 모듈이다. 다른 모듈은 같은 개념을 복제하지 않고 참조·확장한다.

| 대표 개념 | 기준 소유 영역 | 공유 모듈 | 공유 방식·경계 |
| --- | --- | --- | --- |
| Item | COM | ERP 전 모듈, MES, QMS, PLM | 공통 품목 식별. 제품·자재·부품 유형과 연결 |
| Product | PLM/COM 정책 | SAL, PLN, MES, QMS, CST | 설계·판매 산출물의 동일 식별자 공유 |
| Material | COM/ERP 정책 | PUR, INV, PLN, MES, QMS | 자재 마스터와 생산·검사 속성 분리 |
| Bill of Material | PLM | PLN, MES, PUR, CST | EBOM·MBOM 등 관점별 구조 구분 |
| Routing | PLM/제조기술 | PLN, MES, MOM, CST | 공정 정의와 실제 실행 이력 구분 |
| Production Order | ERP-PLN | MES, MOM, QMS, CST | 계획·승인 정보 소유, 실행실적은 MES 소유 |
| Work Order | 업무별 소유 | MES, EAM, MOM | 생산작업·정비작업 유형을 구분해 사용 |
| Work Center | MOM/제조기준정보 | PLN, MES, CST, EAM | 생산 작업장 정의와 설비 자산 연결 |
| Resource | COM/MOM | PLN, MES, EAM, HR | 자원 유형별 세부 소유자 유지 |
| Operation | MES/MOM | PLM, PLN, QMS, CST | 수행 작업 단위. Process와 구분 |
| Process | PLM/제조기술 | PLN, MES, MOM, QMS | 공정 정의·흐름·변환 단계 |
| Production Result | MES | MOM, QMS, CST, INV | 실제 생산 수량·시간·실적 |
| Material Consumption | MES | INV, CST, QMS | 실제 투입 자재와 로트 추적 |
| Lot | INV/MES 추적정책 | PUR, MES, QMS, PLM | 입고·생산·검사 전 과정의 추적 단위 |
| Serial Number | INV/MES 추적정책 | SAL, MES, QMS, PLM, EAM | 개별 제품·설비 단위 추적 |
| Inspection Plan | QMS | PUR, MES, PLM, EAM | 검사 기준·샘플링·특성 정의 |
| Inspection Result | QMS | PUR, MES, INV, PLM | 검사 실행 결과와 판정 |
| Nonconformance | QMS | PUR, MES, EAM, PLM | 규격·요구사항 위반 사건 |
| Corrective Action | QMS | EAM, MES, PLM | 발생한 문제의 원인 제거 조치 |
| Preventive Action | QMS | EAM, MES, PLM | 잠재 문제 발생 방지 조치 |
| Engineering Change | PLM | ERP, MES, QMS, EAM | 설계·기술 변경의 승인·적용성 관리 |
| Revision | PLM/DOC | ERP, MES, QMS, EAM | 승인된 개정 식별. Version과 구분 |
| Equipment | EAM | MOM, MES, QMS, IIOT, TAG, CST | 설비 자산·운영 식별자 공유 |
| Functional Location | EAM | MOM, MES, QMS, TAG | 설비 설치·기능 책임 위치 |
| Maintenance Plan | EAM | MOM, IIOT, TAG, HR | 예방·상태기반 보전 일정 |
| Maintenance Work Order | EAM | MOM, MES, INV, PUR, CST | 설비 정비 실행 명령 |
| Failure Event | EAM | MOM, QMS, IIOT, TAG, CST | 설비 기능 고장 사건 |
| Tag | TAG | IIOT, EAM, MOM, MES, QMS | 논리 데이터 식별자 |
| Signal | TAG/IIOT | EAM, MOM, MES, QMS | 실제 물리·논리 신호 정의 |
| Measurement | IIOT/TAG | MOM, MES, QMS, EAM | 특정 시점에 취득된 값 |
| Alarm | IIOT/MOM 정책 | EAM, MES, QMS | 기술적 임계 경보와 업무 통지 구분 |
| Operator | HR | MES, MOM, QMS, EAM | 작업자 신원 참조, 작업 배정은 현장 모듈 소유 |

### 12.1 데이터 소유권의 중요 분리
- ERP-PLN은 Production Order의 계획·승인 정보를 관리하고 MES는 실제 작업 시작·완료·소비·산출 실적을 관리한다.
- PLM은 BOM·제품개정·설계변경의 기준을 관리하고 ERP·MES는 적용 버전과 유효일을 참조한다.
- QMS는 검사기준·검사결과·부적합 판정의 기준을 관리한다. 검사 결과로 재고 상태를 바꾸는 실행은 INV와 계약으로 연계한다.
- EAM은 설비 자산·기능위치·정비계획·정비오더를 관리한다. IIoT/TAG는 설비 관측 데이터를 수집하며 설비 자산 원장을 대체하지 않는다.
- MOM은 현장 운영을 조정·감시한다. MES 실행 거래나 ERP 회계 원장을 중복 소유하지 않는다.
- HR은 종업원·자격·근무시간의 기준을 관리한다. MES/MOM은 실제 작업 배정·작업실적을 관리한다.

## 13. 모듈 간 대표 업무 흐름

### 13.1 주문에서 생산·출하·매출까지
1. ERP-SAL에서 Sales Order와 납기요청을 확정한다.
2. ERP-PLN에서 수요·재고·능력을 검토하고 Planned Order 및 Production Order를 계획한다.
3. ERP-INV에서 자재 가용성·예약을 확인한다.
4. ERP-WHS에서 자재 피킹·불출을 실행한다.
5. MES에서 Production Order에 따라 생산을 실행하고 Material Consumption·Production Result를 기록한다.
6. QMS에서 공정·최종 검사 및 Nonconformance를 관리한다.
7. ERP-INV에서 합격 재고의 입고·상태 전환을 기록한다.
8. ERP-WHS에서 Picking·Packing·Dispatch를 수행한다.
9. TMS가 포함된 경우 Shipment·Delivery 진행을 관리한다.
10. ERP-FIN에서 Billing·Accounts Receivable·Revenue Recognition을 처리한다.
11. ERP-CST에서 실제원가·차이·수익성을 집계한다.

### 13.2 구매에서 입고·지급까지
1. ERP-PLN 또는 현업이 Purchase Requisition을 생성한다.
2. ERP-PUR에서 공급업체 선정·견적·Purchase Order를 확정한다.
3. 공급업체가 Advance Shipping Notice를 전달할 수 있다.
4. ERP-WHS에서 Receiving·Putaway를 수행한다.
5. ERP-INV에서 Goods Receipt와 재고거래를 기록한다.
6. QMS에서 Inspection Lot·Inspection Result를 기록한다.
7. 부적합이면 격리·반품·시정조치를 연결한다.
8. ERP-FIN에서 Supplier Invoice를 발주·입고와 대사하고 Accounts Payable·지급을 처리한다.
9. ERP-CST에서 자재 취득원가·차이를 반영한다.

### 13.3 설비 이상에서 정비·품질·원가까지
1. TAG가 태그정의·주소·단위·품질코드를 관리한다.
2. IIoT가 신호·측정값·알람을 수집한다.
3. MOM이 생산운영 영향과 현장 대응을 조정한다.
4. EAM에서 Failure Event·Maintenance Request·Maintenance Work Order를 관리한다.
5. QMS에서 제품 영향·부적합·시정조치를 관리한다.
6. ERP-WHS/INV에서 예비품 출고·재고이동을 처리한다.
7. ERP-PUR에서 외부 수리·부품 구매를 처리할 수 있다.
8. ERP-CST/FIN에서 정비비·자산화·원가 귀속을 처리한다.

## 14. 중복·의미 충돌 심사표

| 후보 표현 | 처리 원칙 | 잘못된 통합 방지 |
| --- | --- | --- |
| Order | 단독 대표 개념으로 사용하지 않고 문맥 특정 | Sales Order, Purchase Order, Production Order 분리 |
| Request | 요청 일반 개념과 업무별 요청 객체 구분 | Purchase Requisition, Shipment Request 별도 |
| Receipt | 문맥을 반드시 특정 | Goods Receipt와 Cash Receipt 분리 |
| Delivery | 운송·납품·인도 문맥 구분 | Shipment와 Delivery를 무조건 동의어 처리하지 않음 |
| Issue | 재고불출·문제발생·문서발행 문맥 구분 | Goods Issue, Problem/Issue, Document Issue 분리 |
| Return | 반품·반환·수익률 문맥 구분 | Sales Return, Supplier Return, Return Rate 분리 |
| Plan | 계획 객체 유형 특정 | Demand Plan, Supply Plan, Production Plan 분리 |
| Schedule | 시간표·일정·생산계획 문맥 구분 | Schedule Line, Production Schedule, Work Schedule 분리 |
| Operation | 생산·정비·시스템 실행 문맥 구분 | Operation, Maintenance Operation, System Operation 분리 |
| Process | 업무절차·제조공정·데이터처리 흐름 구분 | Business Process, Manufacturing Process, Data Processing 분리 |
| Resource | 자원 유형 특정 | Equipment, Employee, Material, Tool을 Resource 별칭으로 통합하지 않음 |
| Asset | 회계자산과 운영설비 구분 | Fixed Asset와 Equipment는 연결하되 동일 개념 강제 금지 |
| Cost | 비용·원가·가격·금액 측정 기준 구분 | Expense, Cost, Price, Amount, Revenue 분리 |
| Quantity | 수량 의미·단위 명시 | Ordered, Received, Accepted, Rejected, Available Quantity 분리 |
| Status | 상태값·상태유형·상태전이 구분 | Inspection Status, Inventory Status 문맥 관리 |
| Code | 코드그룹·코드값·외부코드 구분 | Common Code, Code Group, External Code 분리 |

## 15. 관계 용어 분류 기준

| 관계 유형 | 등록 대상 | 예 |
| --- | --- | --- |
| SYNONYM | 의미가 동일한 표현 | 수주 ↔ 고객주문(정의가 같을 때) |
| ABBREVIATION | 공식·업계 약칭 | Material Requirements Planning ↔ MRP |
| SPELLING_VARIANT | 띄어쓰기·철자 변형 | Work Center ↔ Workcentre |
| INDUSTRY_TERM | 특정 산업·현장 표현 | 파렛트 ↔ 팔레트 |
| LEGACY_TERM | 기존 시스템·문서의 구 표현 | 기존 컬럼·화면 용어 |
| BROADER_TERM | 상위 개념 표현 | Item과 Product의 관계 |
| NARROWER_TERM | 하위 개념 표현 | Warehouse Zone과 Warehouse |
| RELATED_TERM | 연관되지만 동일하지 않은 표현 | Shipment와 Delivery |
| CONFUSION_WARNING | 혼동 주의 표현 | Equipment와 Machine |
| CONTEXTUAL_TERM | 특정 모듈 문맥에서만 유효 | Receipt(물품 수령/현금 수령) |

SYNONYM이 아닌 관계 유형은 검색어로 연결할 수 있어도 대표 개념을 자동 치환하지 않는다. 동일한 표현이 서로 다른 개념을 가리키면 모듈 문맥과 정의를 함께 저장한다.

## 16. 대표 개념 등록·약어 승인 체크리스트
- [ ] 기존 대표 개념 ID와 중복 여부 확인
- [ ] 대표 영문명·한글명·정의·포함범위·제외범위 작성
- [ ] 관계 용어별 관계 유형·언어·모듈 문맥 지정
- [ ] 상위·하위·연관·혼동 개념 별도 연결
- [ ] 기준 소유 모듈과 참조 모듈 지정
- [ ] ERP와 MES·MOM·QMS·PLM·EAM 간 데이터 소유권 확인
- [ ] 3자 약어 전역 검색
- [ ] 충돌 시 4자 후보 검토, 미해결은 보류
- [ ] 관계 용어에 독립 물리 약어를 발급하지 않음
- [ ] 기존 DB 이름과 매핑
- [ ] API·메시지·리포트·Migration 영향도 확인
- [ ] 업무 담당자 검토 및 사전 버전 승인 기록

## 17. 현재 상태와 후속 작업

이 문서는 ERP 중심 모듈 세분화와 제조·품질·제품·설비 도메인 간 공유 용어 검토 기준이다. 모든 후보가 확정된 전역 사전이나 약어 승인 목록은 아니다.

후속 작업:
1. 기존 136개 용어 후보 및 확장 사전과 이 문서의 대표 개념 후보 대조
2. 동일 개념은 하나의 대표 개념으로 통합하고 관계 용어는 별도 등록
3. 모호한 단어는 모듈 문맥과 정의를 추가하고 해소되지 않으면 검토 보류
4. 전역 대표 개념 ID 부여
5. 3자·4자 약어를 전체 개념에 대해 일괄 충돌 검사
6. 기존 287개 컬럼 인벤토리와 별도 Commercial DDL 54개 테이블을 서로 다른 범위로 매핑
7. 실제 DB 파일을 확보한 뒤 공급자별 메타데이터와 대조

실제 물리 컬럼 변경과 DB 마이그레이션은 위 검토·승인과 별도의 실행 단계로 진행한다.
