using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FarmStay.Infrastructure.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261119050000_AddBookingConfirmationPermission")]
public sealed class AddBookingConfirmationPermission : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT dbo.Permissions(PermissionName,Description,DisplayOrder,IsSystemPermission,IsActive,CreatedDate)
        SELECT 'Bookings.Confirm','Upload customer ID and collect advance payments',4,1,1,GETDATE()
        WHERE NOT EXISTS(SELECT 1 FROM dbo.Permissions WHERE PermissionName='Bookings.Confirm');
        INSERT dbo.RoleModulePermissions(RoleModuleId,PermissionId,IsAllowed,IsActive,IsDeleted,CreatedDate)
        SELECT rm.RoleModuleId,p.PermissionId,1,1,0,GETDATE()
        FROM dbo.RoleModules rm JOIN dbo.Modules m ON m.ModuleId=rm.ModuleId JOIN dbo.Roles r ON r.RoleId=rm.RoleId
        CROSS JOIN dbo.Permissions p WHERE m.ModuleName='Bookings' AND p.PermissionName='Bookings.Confirm'
        AND r.RoleName IN('Admin','Farm Owner','FarmOwner')
        AND NOT EXISTS(SELECT 1 FROM dbo.RoleModulePermissions x WHERE x.RoleModuleId=rm.RoleModuleId AND x.PermissionId=p.PermissionId);
        """);
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DELETE x FROM dbo.RoleModulePermissions x JOIN dbo.Permissions p ON p.PermissionId=x.PermissionId WHERE p.PermissionName='Bookings.Confirm';
        DELETE dbo.Permissions WHERE PermissionName='Bookings.Confirm';
        """);
}
