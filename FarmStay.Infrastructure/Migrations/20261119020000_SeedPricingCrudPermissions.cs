using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace FarmStay.Infrastructure.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261119020000_SeedPricingCrudPermissions")]
public sealed class SeedPricingCrudPermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DECLARE @moduleId int = (SELECT ModuleId FROM dbo.Modules WHERE ModuleName=N'WebsiteSettings');
        IF @moduleId IS NULL THROW 50022, 'Website Settings module is missing.', 1;
        INSERT dbo.Permissions (PermissionName,Description,DisplayOrder,IsSystemPermission,IsActive,CreatedDate)
        SELECT v.Name,v.Description,v.Sort,1,1,'2026-11-19T02:00:00'
        FROM (VALUES(N'Pricing.Create',N'Create farmhouse pricing',32),(N'Pricing.Delete',N'Delete farmhouse pricing',33)) v(Name,Description,Sort)
        WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions p WHERE p.PermissionName=v.Name);
        INSERT dbo.RoleModulePermissions (RoleModuleId,PermissionId,IsAllowed,IsActive,IsDeleted,CreatedDate)
        SELECT rm.RoleModuleId,p.PermissionId,1,1,0,'2026-11-19T02:00:00'
        FROM dbo.RoleModules rm JOIN dbo.Roles r ON r.RoleId=rm.RoleId CROSS JOIN dbo.Permissions p
        WHERE rm.ModuleId=@moduleId AND r.RoleName IN(N'Admin',N'Farm Owner',N'FarmOwner')
          AND p.PermissionName IN(N'Pricing.Create',N'Pricing.Delete')
          AND NOT EXISTS(SELECT 1 FROM dbo.RoleModulePermissions x WHERE x.RoleModuleId=rm.RoleModuleId AND x.PermissionId=p.PermissionId);
        """);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE g FROM dbo.RoleModulePermissions g JOIN dbo.Permissions p ON p.PermissionId=g.PermissionId
        WHERE p.PermissionName IN(N'Pricing.Create',N'Pricing.Delete') AND g.CreatedDate='2026-11-19T02:00:00';
        DELETE p FROM dbo.Permissions p WHERE p.PermissionName IN(N'Pricing.Create',N'Pricing.Delete')
          AND p.CreatedDate='2026-11-19T02:00:00'
          AND NOT EXISTS(SELECT 1 FROM dbo.RoleModulePermissions g WHERE g.PermissionId=p.PermissionId);
        """);
}
