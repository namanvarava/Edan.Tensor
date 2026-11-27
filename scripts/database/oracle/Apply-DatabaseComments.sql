-- DBeaver에서 edan_framework 또는 Edan 스키마로 연결한 뒤 실행합니다.
BEGIN
  FOR t IN (SELECT table_name FROM user_tables WHERE table_name LIKE 'EDW\_%' ESCAPE '\' OR table_name LIKE 'ED\_%' ESCAPE '\') LOOP
    EXECUTE IMMEDIATE 'COMMENT ON TABLE "' || t.table_name || '" IS ''EDAN.Tensor 제품 테이블 ' || t.table_name || '''';
    FOR c IN (SELECT column_name FROM user_tab_columns WHERE table_name = t.table_name) LOOP
      EXECUTE IMMEDIATE 'COMMENT ON COLUMN "' || t.table_name || '"."' || c.column_name || '" IS ''전자사전 컬럼: ' || c.column_name || '''';
    END LOOP;
  END LOOP;
END;
/
