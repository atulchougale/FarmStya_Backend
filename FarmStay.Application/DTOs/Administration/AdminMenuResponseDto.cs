namespace FarmStay.Application.DTOs.Administration
{
    public class AdminMenuResponseDto
    {
        public List<AdminModuleMenuDto> Modules { get; set; } = new();
    }

    public class AdminModuleMenuDto
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        public List<AdminSubMenuDto> SubMenus { get; set; } = new();
    }

    public class AdminSubMenuDto
    {
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}