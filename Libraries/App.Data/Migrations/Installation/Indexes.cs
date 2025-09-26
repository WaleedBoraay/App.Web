using FluentMigrator;
using App.Core.MigratorServices;

namespace App.Data.Migrations.Installation
{
	[Migration(25517782)]
	public class Indexes : ForwardOnlyMigration
	{
		public override void Up()
		{
			Execute.Sql(@"
SET NOCOUNT ON;

-------------------------------------------------------------------------------
-- 1) اكتشاف جداول المابينج (بالضبط جدول فيه 2 FK) وإنشاء UNIQUE INDEX مركب
-------------------------------------------------------------------------------
;WITH fkcols AS (
    SELECT  t.object_id            AS table_id,
            SCHEMA_NAME(t.schema_id) AS schema_name,
            t.name                 AS table_name,
            c.name                 AS column_name,
            fk.name                AS fk_name
    FROM sys.foreign_key_columns fkc
    JOIN sys.foreign_keys fk           ON fk.object_id = fkc.constraint_object_id
    JOIN sys.tables t                  ON t.object_id  = fk.parent_object_id
    JOIN sys.columns c                 ON c.object_id  = t.object_id AND c.column_id = fkc.parent_column_id
    WHERE t.is_ms_shipped = 0
),
map AS (
    SELECT  table_id, schema_name, table_name,
            STRING_AGG(QUOTENAME(cast(column_name as nvarchar(128))), N',') WITHIN GROUP (ORDER BY column_name) AS fk_cols,
            COUNT(*) AS fk_count
    FROM fkcols
    GROUP BY table_id, schema_name, table_name
)
SELECT * INTO #map FROM map WHERE fk_count = 2;

DECLARE @tbl sysname, @schema sysname, @idx sysname, @cols nvarchar(800);
DECLARE cur_map CURSOR LOCAL FAST_FORWARD FOR
SELECT m.table_name, m.schema_name,
       N'UX_' + m.table_name + N'_Map',
       m.fk_cols
FROM #map m
WHERE NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID(QUOTENAME(m.schema_name) + N'.' + QUOTENAME(m.table_name))
      AND i.is_unique = 1
);

OPEN cur_map;
FETCH NEXT FROM cur_map INTO @tbl, @schema, @idx, @cols;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @sql nvarchar(max) =
        N'CREATE UNIQUE NONCLUSTERED INDEX ' + QUOTENAME(@idx) + N' ON ' + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl) + N' (' + @cols + N');';
    EXEC sp_executesql @sql;
    FETCH NEXT FROM cur_map INTO @tbl, @schema, @idx, @cols;
END
CLOSE cur_map; DEALLOCATE cur_map;

DROP TABLE #map;

-------------------------------------------------------------------------------
-- 2) فهرس لكل عمود FK منفردًا لو مش موجود
-------------------------------------------------------------------------------
;WITH fks AS (
    SELECT  SCHEMA_NAME(t.schema_id) AS schema_name,
            t.name  AS table_name,
            c.name  AS column_name
    FROM sys.foreign_key_columns fkc
    JOIN sys.tables t  ON t.object_id = fkc.parent_object_id
    JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = fkc.parent_column_id
    WHERE t.is_ms_shipped = 0
)
SELECT * INTO #fk FROM fks;

DECLARE @col sysname, @ix sysname;
DECLARE cur_fk CURSOR LOCAL FAST_FORWARD FOR
SELECT schema_name, table_name, column_name,
       N'IX_' + table_name + N'_' + column_name AS ix
FROM #fk f
WHERE NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    JOIN sys.columns c        ON c.object_id  = ic.object_id AND c.column_id = ic.column_id
    JOIN sys.tables  t        ON t.object_id  = i.object_id
    JOIN sys.schemas s        ON s.schema_id  = t.schema_id
    WHERE s.name = f.schema_name AND t.name = f.table_name AND c.name = f.column_name
);

OPEN cur_fk;
FETCH NEXT FROM cur_fk INTO @schema, @tbl, @col, @ix;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'CREATE NONCLUSTERED INDEX ' + QUOTENAME(@ix) + N' ON ' + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl)
             + N' (' + QUOTENAME(@col) + N' ASC);';
    EXEC sp_executesql @sql;

    FETCH NEXT FROM cur_fk INTO @schema, @tbl, @col, @ix;
END
CLOSE cur_fk; DEALLOCATE cur_fk;
DROP TABLE #fk;

-------------------------------------------------------------------------------
-- 3) فهرس على CreatedOnUtc (لو العمود موجود ومفيش فهرس)
-------------------------------------------------------------------------------
;WITH t_has_created AS (
    SELECT SCHEMA_NAME(t.schema_id) AS schema_name, t.name AS table_name
    FROM sys.tables t
    JOIN sys.columns c ON c.object_id = t.object_id AND c.name = N'CreatedOnUtc'
    WHERE t.is_ms_shipped = 0
)
SELECT * INTO #ct FROM t_has_created;

DECLARE @ix2 sysname;
DECLARE cur_ct CURSOR LOCAL FAST_FORWARD FOR
SELECT schema_name, table_name, N'IX_' + table_name + N'_CreatedOnUtc' AS ix
FROM #ct tc
WHERE NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID(QUOTENAME(tc.schema_name) + N'.' + QUOTENAME(tc.table_name))
      AND i.name = N'IX_' + tc.table_name + N'_CreatedOnUtc'
);

OPEN cur_ct;
FETCH NEXT FROM cur_ct INTO @schema, @tbl, @ix2;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'CREATE NONCLUSTERED INDEX ' + QUOTENAME(@ix2) + N' ON '
             + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl) + N' ([CreatedOnUtc] DESC);';
    EXEC sp_executesql @sql;

    FETCH NEXT FROM cur_ct INTO @schema, @tbl, @ix2;
END
CLOSE cur_ct; DEALLOCATE cur_ct;
DROP TABLE #ct;

-------------------------------------------------------------------------------
-- 4) Index للأعمدة Email/Username/SystemName (مع معالجة MAX تلقائيًا)
-------------------------------------------------------------------------------
DECLARE @colname sysname;

DECLARE cur_names CURSOR LOCAL FAST_FORWARD FOR
SELECT s.name AS schema_name, t.name AS table_name, c.name AS column_name, N'IX_' + t.name + N'_' + c.name AS ix
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id AND c.name IN (N'Email', N'Username', N'SystemName')
WHERE t.is_ms_shipped = 0
  AND NOT EXISTS (
      SELECT 1
      FROM sys.indexes i
      JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
      JOIN sys.columns cc       ON cc.object_id = ic.object_id AND cc.column_id = ic.column_id
      WHERE i.object_id = t.object_id AND cc.name = c.name
  );

OPEN cur_names;
FETCH NEXT FROM cur_names INTO @schema, @tbl, @colname, @ix;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- لو العمود MAX، حوّله NVARCHAR(450) أولًا مع الحفاظ على الـ NULL/NOT NULL
    IF EXISTS (
        SELECT 1
        FROM sys.columns c
        JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        WHERE c.object_id = OBJECT_ID(QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl))
          AND c.name = @colname
          AND ( (ty.name IN (N'nvarchar', N'varchar') AND c.max_length = -1) OR ty.name IN (N'ntext', N'text') )
    )
    BEGIN
        DECLARE @nullable nvarchar(20) =
            (SELECT CASE WHEN c.is_nullable = 1 THEN N' NULL' ELSE N' NOT NULL' END
             FROM sys.columns c
             WHERE c.object_id = OBJECT_ID(QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl))
               AND c.name = @colname);
        SET @sql = N'ALTER TABLE ' + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl)
                 + N' ALTER COLUMN ' + QUOTENAME(@colname) + N' NVARCHAR(450)' + @nullable + N';';
        EXEC sp_executesql @sql;
    END

    SET @sql = N'CREATE NONCLUSTERED INDEX ' + QUOTENAME(@ix) + N' ON '
             + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl) + N' (' + QUOTENAME(@colname) + N' ASC);';
    EXEC sp_executesql @sql;

    FETCH NEXT FROM cur_names INTO @schema, @tbl, @colname, @ix;
END
CLOSE cur_names; DEALLOCATE cur_names;
");
		}
	}
}
