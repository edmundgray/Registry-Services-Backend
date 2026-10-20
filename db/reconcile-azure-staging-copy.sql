-- One-off fix for a copy of the Azure staging database (RegistryDatabaseAzure).
-- Its schema already contains every change from the migrations after
-- 20250522141006_AddStatusFieldsToSpecificationIdentifyingInformation except one
-- (UserGroupID required + FK ON DELETE NO ACTION), but __EFMigrationsHistory doesn't record
-- them, so Database.Migrate() would fail re-adding existing columns.
-- This applies that one change (same SQL EF generates) and records the migrations as applied.
-- Safe to run more than once.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'SpecificationIdentifyingInformation' AND COLUMN_NAME = 'UserGroupID' AND IS_NULLABLE = 'YES')
BEGIN
    IF OBJECT_ID('FK_SpecificationIdentifyingInformation_UserGroups_UserGroupID', 'F') IS NOT NULL
        ALTER TABLE [SpecificationIdentifyingInformation] DROP CONSTRAINT [FK_SpecificationIdentifyingInformation_UserGroups_UserGroupID];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SpecificationIdentifyingInformation_UserGroupID'
               AND object_id = OBJECT_ID('SpecificationIdentifyingInformation'))
        DROP INDEX [IX_SpecificationIdentifyingInformation_UserGroupID] ON [SpecificationIdentifyingInformation];

    UPDATE [SpecificationIdentifyingInformation] SET [UserGroupID] = 0 WHERE [UserGroupID] IS NULL;
    ALTER TABLE [SpecificationIdentifyingInformation] ALTER COLUMN [UserGroupID] int NOT NULL;
    ALTER TABLE [SpecificationIdentifyingInformation] ADD DEFAULT 0 FOR [UserGroupID];
    CREATE INDEX [IX_SpecificationIdentifyingInformation_UserGroupID] ON [SpecificationIdentifyingInformation] ([UserGroupID]);
    ALTER TABLE [SpecificationIdentifyingInformation] ADD CONSTRAINT [FK_SpecificationIdentifyingInformation_UserGroups_UserGroupID]
        FOREIGN KEY ([UserGroupID]) REFERENCES [UserGroups] ([UserGroupID]) ON DELETE NO ACTION;
END;

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT m.Id, '9.0.5'
FROM (VALUES ('20250522200124_authorization'),
             ('20250527131148_AddRowPos2Core'),
             ('20250527154728_AddType'),
             ('20250527163222_addConformanceLevel'),
             ('20250703102217_remove-GoverningEntity'),
             ('20261020102247_SyncModelChanges')) m(Id)
WHERE NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory h WHERE h.MigrationId = m.Id);

COMMIT;
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY 1;
