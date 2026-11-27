DECLARE @module int=(SELECT ModuleId FROM dbo.Modules WHERE ModuleName='Dashboard');
INSERT dbo.Permissions(PermissionName,Description,DisplayOrder,IsSystemPermission,IsActive,CreatedDate)
 SELECT 'Dashboard.View','View dashboard summaries and booked dates',1,1,1,GETDATE()
 WHERE NOT EXISTS(SELECT 1 FROM dbo.Permissions WHERE PermissionName='Dashboard.View');
INSERT dbo.RoleModules(RoleId,ModuleId,IsActive,CreatedDate)
 SELECT RoleId,@module,1,GETDATE() FROM dbo.Roles r WHERE RoleName IN('Admin','Farm Owner','FarmOwner')
 AND NOT EXISTS(SELECT 1 FROM dbo.RoleModules m WHERE m.RoleId=r.RoleId AND m.ModuleId=@module);
INSERT dbo.FarmHouseModules(FarmHouseId,ModuleId,IsEnabled,IsDeleted,CreatedDate)
 SELECT FarmHouseId,@module,1,0,GETDATE() FROM dbo.FarmHouses f WHERE IsDeleted=0
 AND NOT EXISTS(SELECT 1 FROM dbo.FarmHouseModules m WHERE m.FarmHouseId=f.FarmHouseId AND m.ModuleId=@module);
INSERT dbo.RoleModulePermissions(RoleModuleId,PermissionId,IsAllowed,IsActive,IsDeleted,CreatedDate)
 SELECT rm.RoleModuleId,p.PermissionId,1,1,0,GETDATE() FROM dbo.RoleModules rm JOIN dbo.Roles r ON r.RoleId=rm.RoleId
 CROSS JOIN dbo.Permissions p WHERE rm.ModuleId=@module AND r.RoleName IN('Admin','Farm Owner','FarmOwner') AND p.PermissionName='Dashboard.View'
 AND NOT EXISTS(SELECT 1 FROM dbo.RoleModulePermissions g WHERE g.RoleModuleId=rm.RoleModuleId AND g.PermissionId=p.PermissionId);
