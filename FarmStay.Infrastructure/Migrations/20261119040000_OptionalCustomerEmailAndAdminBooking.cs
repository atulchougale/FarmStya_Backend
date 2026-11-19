using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace FarmStay.Infrastructure.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261119040000_OptionalCustomerEmailAndAdminBooking")]
public sealed class OptionalCustomerEmailAndAdminBooking : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.DropIndex("IX_Users_FarmHouseId_Email","Users");
        m.AlterColumn<string>("Email","Users",type:"nvarchar(150)",maxLength:150,nullable:true,oldClrType:typeof(string),oldType:"nvarchar(150)",oldMaxLength:150);
        m.AlterColumn<string>("Email","Bookings",type:"nvarchar(150)",maxLength:150,nullable:true,oldClrType:typeof(string),oldType:"nvarchar(150)",oldMaxLength:150);
        m.Sql("UPDATE dbo.Users SET Email=NULL WHERE LTRIM(RTRIM(Email))=''; UPDATE dbo.Bookings SET Email=NULL WHERE LTRIM(RTRIM(Email))='';");
        m.CreateIndex("IX_Users_FarmHouseId_Email","Users",new[]{"FarmHouseId","Email"},unique:true,filter:"[IsDeleted] = 0 AND [Email] IS NOT NULL");
        m.AddColumn<string>("BookingSource","Bookings",type:"nvarchar(20)",maxLength:20,nullable:false,defaultValue:"Online");
        m.Sql("""
            DECLARE @module int=(SELECT ModuleId FROM dbo.Modules WHERE ModuleName='Bookings');
            INSERT dbo.Permissions(PermissionName,Description,DisplayOrder,IsSystemPermission,IsActive,CreatedDate)
            SELECT 'Bookings.Create','Create bookings for verified customers',3,1,1,GETDATE()
            WHERE NOT EXISTS(SELECT 1 FROM dbo.Permissions WHERE PermissionName='Bookings.Create');
            INSERT dbo.RoleModulePermissions(RoleModuleId,PermissionId,IsAllowed,IsActive,IsDeleted,CreatedDate)
            SELECT rm.RoleModuleId,p.PermissionId,1,1,0,GETDATE() FROM dbo.RoleModules rm JOIN dbo.Roles r ON r.RoleId=rm.RoleId
            CROSS JOIN dbo.Permissions p WHERE rm.ModuleId=@module AND p.PermissionName='Bookings.Create'
            AND r.RoleName IN('Admin','Farm Owner','FarmOwner')
            AND NOT EXISTS(SELECT 1 FROM dbo.RoleModulePermissions g WHERE g.RoleModuleId=rm.RoleModuleId AND g.PermissionId=p.PermissionId);
            """);
    }
    protected override void Down(MigrationBuilder m)
    {
        m.Sql("IF EXISTS(SELECT 1 FROM dbo.Users WHERE Email IS NULL) OR EXISTS(SELECT 1 FROM dbo.Bookings WHERE Email IS NULL) THROW 50025,'Populate missing emails before reverting this migration.',1;");
        m.DropColumn("BookingSource","Bookings");
        m.DropIndex("IX_Users_FarmHouseId_Email","Users");
        m.AlterColumn<string>("Email","Users",type:"nvarchar(150)",maxLength:150,nullable:false,oldClrType:typeof(string),oldType:"nvarchar(150)",oldMaxLength:150,oldNullable:true);
        m.AlterColumn<string>("Email","Bookings",type:"nvarchar(150)",maxLength:150,nullable:false,oldClrType:typeof(string),oldType:"nvarchar(150)",oldMaxLength:150,oldNullable:true);
        m.CreateIndex("IX_Users_FarmHouseId_Email","Users",new[]{"FarmHouseId","Email"},unique:true,filter:"[IsDeleted] = 0");
    }
}
