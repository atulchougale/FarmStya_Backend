
SET NOCOUNT ON;
IF NOT EXISTS(SELECT 1 FROM dbo.Modules WHERE ModuleName='Reports')
 INSERT dbo.Modules(ModuleName,DisplayName,Icon,Route,DisplayOrder,IsActive,CreatedDate)
 VALUES('Reports','Reports','bi bi-file-earmark-bar-graph','/reports',6,1,GETDATE());
DECLARE @module int=(SELECT ModuleId FROM dbo.Modules WHERE ModuleName='Reports');
INSERT dbo.FarmHouseModules(FarmHouseId,ModuleId,IsEnabled,IsDeleted,CreatedDate)
 SELECT f.FarmHouseId,@module,1,0,GETDATE() FROM dbo.FarmHouses f WHERE f.IsDeleted=0
 AND NOT EXISTS(SELECT 1 FROM dbo.FarmHouseModules x WHERE x.FarmHouseId=f.FarmHouseId AND x.ModuleId=@module);
INSERT dbo.RoleModules(RoleId,ModuleId,IsActive,CreatedDate)
 SELECT r.RoleId,@module,1,GETDATE() FROM dbo.Roles r WHERE r.RoleName IN('Admin','Farm Owner','FarmOwner')
 AND NOT EXISTS(SELECT 1 FROM dbo.RoleModules x WHERE x.RoleId=r.RoleId AND x.ModuleId=@module);
DECLARE @p TABLE(Name nvarchar(150));
INSERT @p VALUES ('BookingReport.View'),('BookingReport.Export'),('RevenueReport.View'),('RevenueReport.Export'),
 ('CustomerReport.View'),('CustomerReport.Export'),('OccupancyReport.View'),('OccupancyReport.Export');
INSERT dbo.Permissions(PermissionName,Description,DisplayOrder,IsSystemPermission,IsActive,CreatedDate)
 SELECT Name,Name,1,1,1,GETDATE() FROM @p WHERE NOT EXISTS(SELECT 1 FROM dbo.Permissions x WHERE x.PermissionName=Name);
INSERT dbo.RoleModulePermissions(RoleModuleId,PermissionId,IsAllowed,IsActive,IsDeleted,CreatedDate)
 SELECT rm.RoleModuleId,p.PermissionId,1,1,0,GETDATE() FROM dbo.RoleModules rm JOIN dbo.Roles r ON r.RoleId=rm.RoleId
 CROSS JOIN dbo.Permissions p JOIN @p v ON v.Name=p.PermissionName WHERE rm.ModuleId=@module
 AND r.RoleName IN('Admin','Farm Owner','FarmOwner')
 AND NOT EXISTS(SELECT 1 FROM dbo.RoleModulePermissions x WHERE x.RoleModuleId=rm.RoleModuleId AND x.PermissionId=p.PermissionId);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Bookings') AND name='IX_Reporting_Bookings_Created')
 CREATE INDEX IX_Reporting_Bookings_Created ON dbo.Bookings(FarmHouseId,CreatedDate DESC,BookingId DESC)
 INCLUDE(CustomerUserId,BookingReference,FullName,MobileNumber,CheckInDate,CheckOutDate,BookingType,BookingSource,PersonCount,Status,TotalAmount) WHERE IsDelete=0;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.PaymentTransactions') AND name='IX_Reporting_Payments_Received')
 CREATE INDEX IX_Reporting_Payments_Received ON dbo.PaymentTransactions(FarmHouseId,PaidAt DESC,PaymentTransactionId DESC)
 INCLUDE(BookingId,Amount,PaymentType,PaymentMethod,ProviderPaymentId,CreatedDate,VerifiedAt) WHERE IsDelete=0 AND PaymentStatus='Success';
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.BookingStayDates') AND name='IX_Reporting_StayDates')
 CREATE INDEX IX_Reporting_StayDates ON dbo.BookingStayDates(FarmHouseId,StayDate,BookingId) WHERE IsDelete=0;
