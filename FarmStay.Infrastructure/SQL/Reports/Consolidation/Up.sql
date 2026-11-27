SET NOCOUNT ON;
SET XACT_ABORT ON;
IF OBJECT_ID('dbo.__Phase2ReportingBackup') IS NOT NULL THROW 51020,'Reporting consolidation snapshot already exists.',1;
IF (SELECT COUNT(*) FROM dbo.Modules WHERE ModuleName='Reporting')<>1 OR (SELECT COUNT(*) FROM dbo.Modules WHERE ModuleName='Reports')>1
 THROW 51020,'Reporting module identity is ambiguous; review metadata.',1;
IF EXISTS(SELECT PermissionName FROM dbo.Permissions GROUP BY PermissionName HAVING COUNT(*)>1)
 THROW 51020,'Duplicate permission names must be reviewed before consolidation.',1;
CREATE TABLE dbo.__Phase2ReportingBackup(ObjectName nvarchar(40) NOT NULL PRIMARY KEY,BeforeJson nvarchar(max) NOT NULL,AfterJson nvarchar(max) NULL);
INSERT dbo.__Phase2ReportingBackup(ObjectName,BeforeJson) VALUES('Modules',COALESCE((SELECT * FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports') ORDER BY ModuleId FOR JSON PATH, INCLUDE_NULL_VALUES),N'[]'));
INSERT dbo.__Phase2ReportingBackup(ObjectName,BeforeJson) VALUES('RoleModules',COALESCE((SELECT * FROM dbo.RoleModules WHERE ModuleId IN (SELECT ModuleId FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports')) ORDER BY RoleModuleId FOR JSON PATH, INCLUDE_NULL_VALUES),N'[]'));
INSERT dbo.__Phase2ReportingBackup(ObjectName,BeforeJson) VALUES('RoleModulePermissions',COALESCE((SELECT * FROM dbo.RoleModulePermissions WHERE RoleModuleId IN (SELECT RoleModuleId FROM dbo.RoleModules WHERE ModuleId IN (SELECT ModuleId FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports'))) ORDER BY RoleModulePermissionId FOR JSON PATH, INCLUDE_NULL_VALUES),N'[]'));
INSERT dbo.__Phase2ReportingBackup(ObjectName,BeforeJson) VALUES('FarmHouseModules',COALESCE((SELECT * FROM dbo.FarmHouseModules WHERE ModuleId IN (SELECT ModuleId FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports')) ORDER BY FarmHouseModuleId FOR JSON PATH, INCLUDE_NULL_VALUES),N'[]'));
SELECT * INTO #EffectiveBefore FROM (SELECT DISTINCT fm.FarmHouseId,rm.RoleId,g.PermissionId FROM dbo.Modules m
JOIN dbo.FarmHouseModules fm ON fm.ModuleId=m.ModuleId AND fm.IsEnabled=1 AND fm.IsDeleted=0
JOIN dbo.RoleModules rm ON rm.ModuleId=m.ModuleId AND rm.IsActive=1
JOIN dbo.RoleModulePermissions g ON g.RoleModuleId=rm.RoleModuleId AND g.IsAllowed=1 AND g.IsActive=1 AND g.IsDeleted=0
JOIN dbo.Permissions p ON p.PermissionId=g.PermissionId AND p.IsActive=1
WHERE m.ModuleName IN ('Reporting','Reports') AND m.IsActive=1) effective;
DECLARE @canonical int=(SELECT ModuleId FROM dbo.Modules WHERE ModuleName='Reporting');
DECLARE @legacy int=(SELECT ModuleId FROM dbo.Modules WHERE ModuleName='Reports');
-- Include inactive assignments in the snapshot/merge; only effective grants become active.
SELECT rm.RoleId,MAX(CONVERT(int,rm.IsActive)*CONVERT(int,m.IsActive)) IsActive
INTO #Roles FROM dbo.RoleModules rm JOIN dbo.Modules m ON m.ModuleId=rm.ModuleId
WHERE m.ModuleName IN ('Reporting','Reports') GROUP BY rm.RoleId;
SELECT fm.FarmHouseId,MAX(CASE WHEN fm.IsEnabled=1 AND fm.IsDeleted=0 AND m.IsActive=1 THEN 1 ELSE 0 END) Enabled
INTO #Farms FROM dbo.FarmHouseModules fm JOIN dbo.Modules m ON m.ModuleId=fm.ModuleId
WHERE m.ModuleName IN ('Reporting','Reports') GROUP BY fm.FarmHouseId;
SELECT rm.RoleId,g.PermissionId,MAX(CASE WHEN m.IsActive=1 AND rm.IsActive=1 AND g.IsAllowed=1 AND g.IsActive=1 AND g.IsDeleted=0 THEN 1 ELSE 0 END) Allowed
INTO #Grants FROM dbo.RoleModulePermissions g JOIN dbo.RoleModules rm ON rm.RoleModuleId=g.RoleModuleId
JOIN dbo.Modules m ON m.ModuleId=rm.ModuleId WHERE m.ModuleName IN ('Reporting','Reports') GROUP BY rm.RoleId,g.PermissionId;
INSERT dbo.RoleModules(RoleId,ModuleId,IsActive,CreatedDate)
SELECT r.RoleId,@canonical,r.IsActive,GETDATE() FROM #Roles r WHERE NOT EXISTS(SELECT 1 FROM dbo.RoleModules x WHERE x.RoleId=r.RoleId AND x.ModuleId=@canonical);
UPDATE rm SET IsActive=r.IsActive,ModifiedDate=GETDATE() FROM dbo.RoleModules rm JOIN #Roles r ON r.RoleId=rm.RoleId WHERE rm.ModuleId=@canonical;
INSERT dbo.FarmHouseModules(FarmHouseId,ModuleId,IsEnabled,IsDeleted,CreatedDate)
SELECT f.FarmHouseId,@canonical,f.Enabled,0,GETDATE() FROM #Farms f WHERE NOT EXISTS(SELECT 1 FROM dbo.FarmHouseModules x WHERE x.FarmHouseId=f.FarmHouseId AND x.ModuleId=@canonical);
UPDATE fm SET IsEnabled=f.Enabled,IsDeleted=0,ModifiedDate=GETDATE() FROM dbo.FarmHouseModules fm JOIN #Farms f ON f.FarmHouseId=fm.FarmHouseId WHERE fm.ModuleId=@canonical;
INSERT dbo.RoleModulePermissions(RoleModuleId,PermissionId,IsAllowed,IsActive,IsDeleted,CreatedDate)
SELECT rm.RoleModuleId,g.PermissionId,g.Allowed,1,0,GETDATE() FROM #Grants g JOIN dbo.RoleModules rm ON rm.RoleId=g.RoleId AND rm.ModuleId=@canonical
WHERE NOT EXISTS(SELECT 1 FROM dbo.RoleModulePermissions x WHERE x.RoleModuleId=rm.RoleModuleId AND x.PermissionId=g.PermissionId);
UPDATE p SET IsAllowed=g.Allowed,IsActive=1,IsDeleted=0,ModifiedDate=GETDATE()
FROM dbo.RoleModulePermissions p JOIN dbo.RoleModules rm ON rm.RoleModuleId=p.RoleModuleId
JOIN #Grants g ON g.RoleId=rm.RoleId AND g.PermissionId=p.PermissionId WHERE rm.ModuleId=@canonical;
UPDATE dbo.Modules SET DisplayName='Reporting',Route='/reporting',Icon='bi bi-file-earmark-bar-graph',IsActive=1,ModifiedDate=GETDATE() WHERE ModuleId=@canonical;
-- Retain the legacy row and all original assignment IDs for audit; it cannot authorize/menu-render.
UPDATE dbo.Modules SET IsActive=0,ModifiedDate=GETDATE() WHERE ModuleId=@legacy;
UPDATE dbo.RoleModules SET IsActive=0,ModifiedDate=GETDATE() WHERE ModuleId=@legacy;
UPDATE dbo.FarmHouseModules SET IsEnabled=0,ModifiedDate=GETDATE() WHERE ModuleId=@legacy;
SELECT * INTO #EffectiveAfter FROM (SELECT DISTINCT fm.FarmHouseId,rm.RoleId,g.PermissionId FROM dbo.Modules m
JOIN dbo.FarmHouseModules fm ON fm.ModuleId=m.ModuleId AND fm.IsEnabled=1 AND fm.IsDeleted=0
JOIN dbo.RoleModules rm ON rm.ModuleId=m.ModuleId AND rm.IsActive=1
JOIN dbo.RoleModulePermissions g ON g.RoleModuleId=rm.RoleModuleId AND g.IsAllowed=1 AND g.IsActive=1 AND g.IsDeleted=0
JOIN dbo.Permissions p ON p.PermissionId=g.PermissionId AND p.IsActive=1
WHERE m.ModuleName IN ('Reporting','Reports') AND m.IsActive=1) effective;
IF EXISTS(SELECT * FROM #EffectiveBefore EXCEPT SELECT * FROM #EffectiveAfter)
 OR EXISTS(SELECT * FROM #EffectiveAfter EXCEPT SELECT * FROM #EffectiveBefore)
 THROW 51020,'Consolidation would change effective tenant/role permissions; transaction rolled back for review.',1;
UPDATE dbo.__Phase2ReportingBackup SET AfterJson=COALESCE((SELECT * FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports') ORDER BY ModuleId FOR JSON PATH, INCLUDE_NULL_VALUES),N'[]') WHERE ObjectName='Modules';
UPDATE dbo.__Phase2ReportingBackup SET AfterJson=COALESCE((SELECT * FROM dbo.RoleModules WHERE ModuleId IN (SELECT ModuleId FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports')) ORDER BY RoleModuleId FOR JSON PATH, INCLUDE_NULL_VALUES),N'[]') WHERE ObjectName='RoleModules';
UPDATE dbo.__Phase2ReportingBackup SET AfterJson=COALESCE((SELECT * FROM dbo.RoleModulePermissions WHERE RoleModuleId IN (SELECT RoleModuleId FROM dbo.RoleModules WHERE ModuleId IN (SELECT ModuleId FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports'))) ORDER BY RoleModulePermissionId FOR JSON PATH, INCLUDE_NULL_VALUES),N'[]') WHERE ObjectName='RoleModulePermissions';
UPDATE dbo.__Phase2ReportingBackup SET AfterJson=COALESCE((SELECT * FROM dbo.FarmHouseModules WHERE ModuleId IN (SELECT ModuleId FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports')) ORDER BY FarmHouseModuleId FOR JSON PATH, INCLUDE_NULL_VALUES),N'[]') WHERE ObjectName='FarmHouseModules';
DROP TABLE #EffectiveBefore,#EffectiveAfter,#Roles,#Farms,#Grants;
