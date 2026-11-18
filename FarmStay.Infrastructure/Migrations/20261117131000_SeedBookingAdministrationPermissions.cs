using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FarmStay.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261117131000_SeedBookingAdministrationPermissions")]
public sealed class SeedBookingAdministrationPermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DECLARE @moduleId int = (SELECT ModuleId FROM dbo.Modules WHERE ModuleName = N'Bookings');
            DECLARE @seededAt datetime2 = '2026-11-17T13:10:00';
            IF @moduleId IS NULL THROW 50011, 'Bookings module is missing.', 1;

            INSERT dbo.Permissions (PermissionName, Description, DisplayOrder, IsSystemPermission, IsActive, CreatedDate)
            SELECT p.Name, p.Description, p.DisplayOrder, 1, 1, @seededAt
            FROM (VALUES (N'Bookings.View', N'View farmhouse bookings', 1),
                         (N'Bookings.Decide', N'Approve or reject booking requests', 2)) p(Name, Description, DisplayOrder)
            WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions existing WHERE existing.PermissionName = p.Name);

            INSERT dbo.RoleModules (RoleId, ModuleId, IsActive, CreatedDate)
            SELECT r.RoleId, @moduleId, 1, @seededAt
            FROM dbo.Roles r
            WHERE r.RoleName IN (N'Admin', N'Farm Owner', N'FarmOwner') AND NOT EXISTS (
                SELECT 1 FROM dbo.RoleModules rm WHERE rm.RoleId = r.RoleId AND rm.ModuleId = @moduleId);

            INSERT dbo.RoleModulePermissions (RoleModuleId, PermissionId, IsAllowed, IsActive, IsDeleted, CreatedDate)
            SELECT rm.RoleModuleId, p.PermissionId, 1, 1, 0, @seededAt
            FROM dbo.RoleModules rm
            JOIN dbo.Roles r ON r.RoleId = rm.RoleId
            CROSS JOIN dbo.Permissions p
            WHERE rm.ModuleId = @moduleId AND r.RoleName IN (N'Admin', N'Farm Owner', N'FarmOwner')
              AND p.PermissionName IN (N'Bookings.View', N'Bookings.Decide')
              AND NOT EXISTS (SELECT 1 FROM dbo.RoleModulePermissions existing
                  WHERE existing.RoleModuleId = rm.RoleModuleId AND existing.PermissionId = p.PermissionId);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DECLARE @seededAt datetime2 = '2026-11-17T13:10:00';
            DELETE grantRow
            FROM dbo.RoleModulePermissions grantRow
            JOIN dbo.Permissions p ON p.PermissionId = grantRow.PermissionId
            WHERE p.PermissionName IN (N'Bookings.View', N'Bookings.Decide')
              AND grantRow.CreatedDate = @seededAt;

            DELETE p FROM dbo.Permissions p
            WHERE p.PermissionName IN (N'Bookings.View', N'Bookings.Decide') AND p.CreatedDate = @seededAt
              AND NOT EXISTS (SELECT 1 FROM dbo.RoleModulePermissions grantRow WHERE grantRow.PermissionId = p.PermissionId);

            DELETE rm FROM dbo.RoleModules rm
            JOIN dbo.Modules m ON m.ModuleId = rm.ModuleId
            JOIN dbo.Roles r ON r.RoleId = rm.RoleId
            WHERE m.ModuleName = N'Bookings' AND r.RoleName IN (N'Admin', N'Farm Owner', N'FarmOwner')
              AND rm.CreatedDate = @seededAt
              AND NOT EXISTS (SELECT 1 FROM dbo.RoleModulePermissions grantRow WHERE grantRow.RoleModuleId = rm.RoleModuleId);
            """);
    }
}
