-- Read-only post-migration checks. Expected: exactly one active Reporting module at /reporting.
SELECT ModuleId,ModuleName,Route,IsActive FROM dbo.Modules WHERE ModuleName IN ('Reporting','Reports');
SELECT m.ModuleName,COUNT(*) AssignmentCount FROM dbo.RoleModules r JOIN dbo.Modules m ON m.ModuleId=r.ModuleId WHERE m.ModuleName IN ('Reporting','Reports') GROUP BY m.ModuleName;
SELECT ObjectName,DATALENGTH(BeforeJson) BeforeBytes,DATALENGTH(AfterJson) AfterBytes FROM dbo.__Phase2ReportingBackup;
SELECT PermissionName,COUNT(*) DuplicateCount FROM dbo.Permissions GROUP BY PermissionName HAVING COUNT(*)>1;
SELECT MigrationId FROM dbo.__EFMigrationsHistory WHERE MigrationId='20261125000100_ConsolidateReportingIdentity';
