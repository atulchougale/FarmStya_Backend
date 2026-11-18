using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FarmStay.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261119010000_SeedPricingManagementPermissions")]
public sealed class SeedPricingManagementPermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DECLARE @moduleId int = (SELECT ModuleId FROM dbo.Modules WHERE ModuleName=N'WebsiteSettings');
        IF @moduleId IS NULL THROW 50022, 'Website Settings module is missing.', 1;
        DECLARE @seededAt datetime2 = '2026-11-19T01:00:00';
        INSERT dbo.Permissions (PermissionName, Description, DisplayOrder, IsSystemPermission, IsActive, CreatedDate)
        SELECT v.Name, v.Description, v.Sort, 1, 1, @seededAt
        FROM (VALUES (N'Pricing.View',N'View farmhouse daily pricing',30),
                     (N'Pricing.Edit',N'Manage farmhouse daily pricing',31)) v(Name,Description,Sort)
        WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions p WHERE p.PermissionName=v.Name);
        INSERT dbo.RoleModules (RoleId,ModuleId,IsActive,CreatedDate)
        SELECT r.RoleId,@moduleId,1,@seededAt FROM dbo.Roles r
        WHERE r.RoleName IN (N'Admin',N'Farm Owner',N'FarmOwner')
          AND NOT EXISTS (SELECT 1 FROM dbo.RoleModules rm WHERE rm.RoleId=r.RoleId AND rm.ModuleId=@moduleId);
        INSERT dbo.RoleModulePermissions (RoleModuleId,PermissionId,IsAllowed,IsActive,IsDeleted,CreatedDate)
        SELECT rm.RoleModuleId,p.PermissionId,1,1,0,@seededAt FROM dbo.RoleModules rm
        JOIN dbo.Roles r ON r.RoleId=rm.RoleId CROSS JOIN dbo.Permissions p
        WHERE rm.ModuleId=@moduleId AND r.RoleName IN (N'Admin',N'Farm Owner',N'FarmOwner')
          AND p.PermissionName IN (N'Pricing.View',N'Pricing.Edit')
          AND NOT EXISTS (SELECT 1 FROM dbo.RoleModulePermissions x WHERE x.RoleModuleId=rm.RoleModuleId AND x.PermissionId=p.PermissionId);
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE rmp FROM dbo.RoleModulePermissions rmp JOIN dbo.Permissions p ON p.PermissionId=rmp.PermissionId
        WHERE p.PermissionName IN (N'Pricing.View',N'Pricing.Edit') AND rmp.CreatedDate='2026-11-19T01:00:00';
        DELETE p FROM dbo.Permissions p WHERE p.PermissionName IN (N'Pricing.View',N'Pricing.Edit')
          AND p.CreatedDate='2026-11-19T01:00:00'
          AND NOT EXISTS (SELECT 1 FROM dbo.RoleModulePermissions rmp WHERE rmp.PermissionId=p.PermissionId);
        """);
}
