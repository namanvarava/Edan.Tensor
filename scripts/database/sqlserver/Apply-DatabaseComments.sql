-- DBeaver에서 edan_framework 또는 Edan DB에 연결한 뒤 SQL Server로 실행합니다.
DECLARE @schema sysname, @table sysname, @column sysname, @description nvarchar(4000), @sql nvarchar(max);
DECLARE tables_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT s.name, t.name
    FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE t.name LIKE 'edw[_]%' OR t.name LIKE 'ed[_]%';
OPEN tables_cursor;
FETCH NEXT FROM tables_cursor INTO @schema, @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @description = CASE @table
        WHEN N'edw_01001' THEN N'EdanFramework 테넌트'
        WHEN N'edw_01004' THEN N'EdanFramework 사용자'
        WHEN N'edw_04001' THEN N'EdanFramework 테넌트 고객 DB 바인딩'
        WHEN N'edw_04002' THEN N'EdanFramework 테넌트 Routine 허용목록'
        WHEN N'edw_04003' THEN N'EdanFramework 테넌트 다중 DB 변경 감사'
        WHEN N'ed_00001' THEN N'Edan SYS 메뉴'
        WHEN N'ed_00002' THEN N'Edan SYS 메뉴 버튼'
        WHEN N'ed_00003' THEN N'Edan SYS 역할'
        WHEN N'ed_00004' THEN N'Edan SYS 역할 버튼 권한'
        WHEN N'ed_00005' THEN N'Edan SYS 공통코드'
        WHEN N'ed_01001' THEN N'Edan BAS 회사'
        WHEN N'ed_01002' THEN N'Edan BAS 공장'
        WHEN N'ed_01003' THEN N'Edan BAS 공정'
        WHEN N'ed_01004' THEN N'Edan BAS 생산 라인'
        WHEN N'ed_01005' THEN N'Edan BAS 설비'
        ELSE N'EDAN.Tensor 제품 테이블 ' + @table END;
    IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.tables t ON ep.major_id=t.object_id JOIN sys.schemas s ON t.schema_id=s.schema_id WHERE ep.name=N'MS_Description' AND ep.minor_id=0 AND s.name=@schema AND t.name=@table)
        EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=@description, @level0type=N'SCHEMA', @level0name=@schema, @level1type=N'TABLE', @level1name=@table;
    ELSE
        EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@description, @level0type=N'SCHEMA', @level0name=@schema, @level1type=N'TABLE', @level1name=@table;
    DECLARE columns_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT c.name FROM sys.columns c JOIN sys.tables t ON c.object_id=t.object_id JOIN sys.schemas s ON t.schema_id=s.schema_id WHERE s.name=@schema AND t.name=@table;
    OPEN columns_cursor; FETCH NEXT FROM columns_cursor INTO @column;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @description = N'전자사전 컬럼: ' + @column;
        IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.tables t ON ep.major_id=t.object_id JOIN sys.schemas s ON t.schema_id=s.schema_id JOIN sys.columns c ON c.object_id=t.object_id AND c.column_id=ep.minor_id WHERE ep.name=N'MS_Description' AND s.name=@schema AND t.name=@table AND c.name=@column)
            EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=@description, @level0type=N'SCHEMA', @level0name=@schema, @level1type=N'TABLE', @level1name=@table, @level2type=N'COLUMN', @level2name=@column;
        ELSE
            EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@description, @level0type=N'SCHEMA', @level0name=@schema, @level1type=N'TABLE', @level1name=@table, @level2type=N'COLUMN', @level2name=@column;
        FETCH NEXT FROM columns_cursor INTO @column;
    END
    CLOSE columns_cursor; DEALLOCATE columns_cursor;
    FETCH NEXT FROM tables_cursor INTO @schema, @table;
END
CLOSE tables_cursor; DEALLOCATE tables_cursor;
