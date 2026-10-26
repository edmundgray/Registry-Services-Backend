-- One-off fix for a database restored from "Backup Tuesday 27th.bak".
-- That backup already contains (some of) the columns from migration 20250703102217_remove-GoverningEntity
-- but its __EFMigrationsHistory doesn't record it, so the API's startup migration fails with
-- "Column name 'ConformanceType' ... is specified more than once".
-- This adds any of that migration's columns that are still missing and marks it applied.
-- Safe to run more than once.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;
IF COL_LENGTH('CoreInvoiceModel','BusinessRules') IS NULL ALTER TABLE CoreInvoiceModel ADD BusinessRules nvarchar(50) NULL;
IF COL_LENGTH('CoreInvoiceModel','DataType') IS NULL ALTER TABLE CoreInvoiceModel ADD DataType nvarchar(50) NULL;
IF COL_LENGTH('CoreInvoiceModel','ParentID') IS NULL ALTER TABLE CoreInvoiceModel ADD ParentID nvarchar(50) NULL;
IF COL_LENGTH('CoreInvoiceModel','UsageNote') IS NULL ALTER TABLE CoreInvoiceModel ADD UsageNote text NULL;
IF COL_LENGTH('ExtensionComponentModelElements','ConformanceType') IS NULL ALTER TABLE ExtensionComponentModelElements ADD ConformanceType nvarchar(50) NULL;
IF COL_LENGTH('ExtensionComponentModelElements','Justification') IS NULL ALTER TABLE ExtensionComponentModelElements ADD Justification text NULL;
IF COL_LENGTH('ExtensionComponentModelElements','UsageNoteCore') IS NULL ALTER TABLE ExtensionComponentModelElements ADD UsageNoteCore text NULL;
IF COL_LENGTH('ExtensionComponentModelElements','UsageNoteExtension') IS NULL ALTER TABLE ExtensionComponentModelElements ADD UsageNoteExtension text NULL;
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20250703102217_remove-GoverningEntity')
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20250703102217_remove-GoverningEntity', '9.0.5');
COMMIT;
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY 1;
