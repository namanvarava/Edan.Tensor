-- DBeaver에서 실행: edan_framework 또는 Edan 업무 DB에 연결한 뒤 실행합니다.
-- 제품 테이블의 설명과 컬럼 전자사전 기본 설명을 안전하게 재생성합니다.
DO $$
DECLARE
    t record;
    c record;
    description text;
BEGIN
    FOR t IN
        SELECT table_schema, table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND (table_name LIKE 'edw\_%' ESCAPE '\' OR table_name LIKE 'ed\_%' ESCAPE '\')
    LOOP
        description := CASE t.table_name
            WHEN 'edw_01001' THEN 'EdanFramework 테넌트'
            WHEN 'edw_01004' THEN 'EdanFramework 사용자'
            WHEN 'edw_04001' THEN 'EdanFramework 테넌트 고객 DB 바인딩'
            WHEN 'edw_04002' THEN 'EdanFramework 테넌트 Routine 허용목록'
            WHEN 'edw_04003' THEN 'EdanFramework 테넌트 다중 DB 변경 감사'
            WHEN 'edw_03004' THEN 'EdanFramework 외부 전달 작업'
            WHEN 'edw_03005' THEN 'EdanFramework Worker 작업 큐'
            WHEN 'edw_03006' THEN 'EdanFramework Worker 재실행 이력'
            WHEN 'edw_03007' THEN 'EdanFramework Worker 작업 유형'
            WHEN 'edw_05001' THEN 'EdanFramework DataSet'
            WHEN 'ed_00001' THEN 'Edan SYS 메뉴'
            WHEN 'ed_00002' THEN 'Edan SYS 메뉴 버튼'
            WHEN 'ed_00003' THEN 'Edan SYS 역할'
            WHEN 'ed_00004' THEN 'Edan SYS 역할 버튼 권한'
            WHEN 'ed_00005' THEN 'Edan SYS 공통코드'
            WHEN 'ed_01001' THEN 'Edan BAS 회사'
            WHEN 'ed_01002' THEN 'Edan BAS 공장'
            WHEN 'ed_01003' THEN 'Edan BAS 공정'
            WHEN 'ed_01004' THEN 'Edan BAS 생산 라인'
            WHEN 'ed_01005' THEN 'Edan BAS 설비'
            ELSE 'EDAN.Tensor 제품 테이블 ' || t.table_name
        END;
        EXECUTE format('COMMENT ON TABLE %I.%I IS %L', t.table_schema, t.table_name, description);
        FOR c IN
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = t.table_schema AND table_name = t.table_name
        LOOP
            EXECUTE format('COMMENT ON COLUMN %I.%I.%I IS %L', t.table_schema, t.table_name, c.column_name,
                '전자사전 컬럼: ' || c.column_name);
        END LOOP;
    END LOOP;
END $$;
